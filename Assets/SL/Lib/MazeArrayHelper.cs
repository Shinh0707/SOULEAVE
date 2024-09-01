using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SL.Lib.TensorPadding;
using Random = System.Random;

namespace SL.Lib
{
    public class TensorLabel
    {
        private Tensor<int> _label;
        private int[] _labelKinds;

        public TensorLabel(Tensor<bool> tensor, bool backgroundValue = false)
        {
            if (tensor.Shape.Length != 2)
            {
                throw new ArgumentException("Input tensor must be 2-dimensional");
            }

            int rows = tensor.Shape[0];
            int cols = tensor.Shape[1];
            _label = new Tensor<int>(new int[rows * cols], rows, cols);

            int currentLabel = 1;
            var equivalenceTable = new Dictionary<int, int>();

            // First pass: assign temporary labels and record equivalences
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (tensor[i, j] != backgroundValue)
                    {
                        var neighbors = GetNeighbors(i, j, rows, cols)
                            .Select(p => _label[p.Item1, p.Item2])
                            .Where(l => l != 0)
                            .ToList();

                        if (neighbors.Count == 0)
                        {
                            _label[i, j] = currentLabel++;
                        }
                        else
                        {
                            int minLabel = neighbors.Min();
                            _label[i, j] = minLabel;

                            foreach (var neighbor in neighbors.Where(n => n != minLabel))
                            {
                                Union(equivalenceTable, minLabel, neighbor);
                            }
                        }
                    }
                }
            }

            // Second pass: resolve equivalences
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (_label[i, j] != 0)
                    {
                        _label[i, j] = Find(equivalenceTable, _label[i, j]);
                    }
                }
            }

            // Collect unique labels (excluding background)
            _labelKinds = _label.Data.Distinct().Where(l => l != 0).OrderBy(l => l).ToArray();
        }

        public void ApplyEachLabel<T>(Action<List<int>> func) where T : IComparable<T>
        {
            foreach (var kind in _labelKinds)
            {
                func((_label == kind).ArgWhere());
            }
        }
        public void ApplyEachLabel<T>(Action<Tensor<bool>> func) where T : IComparable<T>
        {
            foreach (var kind in _labelKinds)
            {
                func(_label == kind);
            }
        }

        private IEnumerable<Tuple<int, int>> GetNeighbors(int i, int j, int rows, int cols)
        {
            if (i > 0) yield return Tuple.Create(i - 1, j);
            if (j > 0) yield return Tuple.Create(i, j - 1);
        }

        private void Union(Dictionary<int, int> equivalenceTable, int a, int b)
        {
            int rootA = Find(equivalenceTable, a);
            int rootB = Find(equivalenceTable, b);

            if (rootA != rootB)
            {
                equivalenceTable[rootB] = rootA;
            }
        }

        private int Find(Dictionary<int, int> equivalenceTable, int label)
        {
            if (!equivalenceTable.ContainsKey(label))
            {
                equivalenceTable[label] = label;
            }

            if (equivalenceTable[label] != label)
            {
                equivalenceTable[label] = Find(equivalenceTable, equivalenceTable[label]);
            }

            return equivalenceTable[label];
        }
    }

    public class MazeCreater
    {
        private static Random _random;
        public static Random Random
        {
            get
            {
                return _random ??= new Random();
            }
        }

        public static T SelectRandom<T>(List<T> pool) => pool[Random.Next(pool.Count)];
        public static bool FindRandomPosition<T>(Tensor<T> field, T searchValue, Tensor<bool> mask, out int directIndex) where T : IComparable<T>
        {
            var selectMask = mask & (field == searchValue);
            if (selectMask.Any())
            {
                var selectList = selectMask.ArgWhere();
                directIndex = SelectRandom(selectList);
                return true;
            }
            directIndex = 0;
            return false;
        }
    }

    public class MazeArrayHelper
    {
        private static List<Tensor<float>> _rotMasks;
        private static Tensor<int> _rotLabel;
        private static Tensor<float> _surroundMask;
        private static Tensor<float> _neighborMask;
        private static Tensor<float> _deltaMask;
        

        public static List<Tensor<float>> RotMasks
        {
            get
            {
                if (_rotMasks == null)
                {
                    var s1 = new Tensor<float>(new[,]{
                        { 1f, 0f, 1f },
                        { 0f, 0f, 1f },
                        { 1f, 1f, 1f }
                    });

                    var s2 = new Tensor<float>(new[,]
                    {
                        { 0f, 1f, 0f },
                        { 0f, 0f, 0f },
                        { 1f ,1f, 1f }
                    });

                    _rotMasks = new List<Tensor<float>> { s1, s2 };

                    for (int i = 0; i < 3; i++)
                    {
                        s1 = s1.Rotate90();
                        s2 = s2.Rotate90();
                        RotMasks.Add(s1);
                        RotMasks.Add(s2);
                    }
                }
                return _rotMasks;
            }
        }

        public static Tensor<int> RotLabel
        {
            get
            {
                return _rotLabel ??= Tensor<int>.FromArray(new[,]
                {
                    { 0, 1, 2 },
                    { 7, 8, 3 },
                    { 6, 5, 4 }
                }).AsReadOnly();
            }
        }

        public static Tensor<float> SurroundMask
        {
            get
            {
                return _surroundMask ??= Tensor<float>.FromArray(new[,]
                {
                    { 1f, 1f, 1f },
                    { 1f, 0f, 1f },
                    { 1f, 1f, 1f }
                }).AsReadOnly();
            }
        }

        public static Tensor<float> NeighborMask
        {
            get
            {
                return _neighborMask ??= Tensor<float>.FromArray(new[,]
                {
                    { 0f, 1f, 0f },
                    { 1f, 0f, 1f },
                    { 0f, 1f, 0f }
                }).AsReadOnly();
            }
        }

        public static Tensor<float> DeltaMask
        {
            get
            {
                return _deltaMask ??= Tensor<float>.FromArray(new[,]
                {
                    { 0f, 1f, 0f },
                    { 1f, -4f, 1f },
                    { 0f, 1f, 0f }
                }).AsReadOnly();
            }
        }

        public static Tensor<float> NeighborScore(Tensor<float> field)
        {
            var padded = (1f - field).Pad(1, new ConstantPadMode<float>(0f));
            var mask = padded == 0f;
            var R = Tensor<float>.Zeros(field);
            R[mask] = padded.Convolve(NeighborMask)[mask];
            return R;
        }

        public static Tensor<float> DeltaScore(Tensor<float> field, Tensor<float> neighborScore)
        {
            var mask = field == 0;
            var R = Tensor<float>.Zeros(field);
            var paddedNeighborScore = neighborScore.Pad(1, new ConstantPadMode<float>(0f));
            R[mask] = paddedNeighborScore.Convolve(DeltaMask)[mask];
            return R;
        }

        public static Tensor<float> Difficulty(Tensor<float> field, Tensor<float> neighborScore, Tensor<float> deltaScore, TensorLabel tensorLabel, int steps = 100, float alpha = 0.5f)
        {
            float dx;
            float dy = dx = 1.0f;
            float dt = 1.0f;
            float ddx;
            float ddy = ddx = Mathf.Pow(1.0f / dx, 2);
            alpha = Mathf.Min(alpha, (0.5f / (ddx + ddy)) / dt);
            var normalizedDeltaScore = new Tensor<float>(deltaScore);
            tensorLabel.ApplyEachLabel<float>((List<int> indices) =>
            {
                normalizedDeltaScore[indices] = normalizedDeltaScore[indices].RN2C();
            });
            var R = new Tensor<float>(normalizedDeltaScore);
            var mask = field == 0;
            var maskedIndice = mask.ArgWhere();
            var notMaskedIndice = (!mask).ArgWhere();
            var kernel = Tensor<float>.FromArray(new[,]
            {
                 {0f, ddy, 0f},
                 {ddx, 0f, ddx},
                 {0f, ddy, 0f},
            }) * (alpha * dt);
            var neighborCount = alpha * dt * ddx * neighborScore;
            for (int i = 0; i < steps; i++) 
            {
                var lap = R.Pad(1, new ConstantPadMode<float>(0f)).Convolve(kernel);
                R[maskedIndice] += lap[maskedIndice] - neighborCount[maskedIndice] * R[maskedIndice];
                R[notMaskedIndice] = 0f;
            }
            return R;
        }

        public static (Tensor<bool>, Tensor<bool>, Tensor<float>) DifficultyPeaks(Tensor<float> field, Tensor<float> difficultyScore, TensorLabel tensorLabel)
        {
            var mask = field == 0;
            var maxPeaks = difficultyScore.GetPeak2D(mask, TensorPeakDetection.PeakMode.Maximum);
            var minPeaks = difficultyScore.GetPeak2D(mask, TensorPeakDetection.PeakMode.Minimum);
            var normalizedPeak = Tensor<float>.Zeros(field);

            void Norm(Tensor<bool> selector)
            {
                var MaxSelector = selector & maxPeaks;
                var MinSelector = selector & minPeaks;
                if (MaxSelector.Any())
                {
                    normalizedPeak[MaxSelector] = difficultyScore[MaxSelector].MinMaxNormalize() + 1f;
                }
                else
                {
                    MaxSelector = selector & (difficultyScore == difficultyScore[selector].Max);
                    normalizedPeak[MaxSelector] = 2;
                }
                if (MinSelector.Any())
                {
                    normalizedPeak[MinSelector] = -((-difficultyScore[MinSelector]).MinMaxNormalize() + 1f);
                }
                else
                {
                    MinSelector = selector & (difficultyScore == difficultyScore[selector].Min);
                    normalizedPeak[MinSelector] = -2;
                }
            }

            tensorLabel.ApplyEachLabel<float>(Norm);
            return (maxPeaks, minPeaks, normalizedPeak);
        }
    }
}