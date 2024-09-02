using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SL.Lib.SLSequential;

namespace SL.Lib
{
    /// <summary>
    /// 指定された位置を中心とする3x3の領域をフィールドから効率的に抽出します。
    ///
    /// 引数:
    ///     pos(tuple[int, int]) : 中心位置の(行, 列)座標
    ///     field(np.ndarray): 2次元のnumpy配列
    ///     return_generator(bool): ジェネレータを返すかどうか（デフォルトはFalse）
    ///     pad_value(int) : 境界外を埋める値（デフォルトは1）
    ///
    /// 戻り値:
    ///     np.ndarray: 3x3の抽出された領域
    ///     境界外の場合はパディングとしてpad_valueが使用されます。
    ///     return_generatorがTrueの場合は、抽出された領域とジェネレータ関数のタプルを返します。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ESEResult
    {
        private Range _rowRange;
        private Range _colRange;
        private (int row,int col) _center;

        public static ESEResult Get<T>(Indice[] pos,Tensor<T> field) where T : IComparable<T>
        {
            Indice[] center = pos;
            int rows = field.Size(0);
            int cols = field.Size(1);
            int row = center[0].Values[0];
            int col = center[1].Values[0];
            return new ESEResult((row,col), Mathf.Max(0, row - 1)..Mathf.Min(rows, row + 2), Mathf.Max(0, col - 1)..Mathf.Min(cols, col + 2));
        }

        public ESEResult((int,int) center,Range rowRange, Range colRange)
        {
            _center = center;
            _rowRange = rowRange;
            _colRange = colRange;
        }

        public Tensor<TTarget> Extract<TTarget>(Tensor<TTarget> target, TTarget padValue) where TTarget : IComparable<TTarget>
        {
            var extracted = target[_rowRange, _colRange];

            var result = Tensor<TTarget>.Full(padValue, 3, 3);

            // 抽出した領域を結果配列の適切な位置に配置
            var resultStartRow = 1 - (_center.row - _rowRange.Start.Value);
            var resultStartCol = 1 - (_center.col - _colRange.Start.Value);
            result[resultStartRow..(resultStartRow + extracted.Size(0)), resultStartCol..(resultStartCol + extracted.Size(1))] = extracted;
            return result;
        }
    }

    public class TensorLabel
    {
        private Tensor<int> _label;

        public Tensor<int> Label
        {
            get { return new(_label,true); }
        }

        private List<int> _labelKinds;

        public TensorLabel(Tensor<bool> tensor, bool backgroundValue = false)
        {
            if (tensor.Shape.Length != 2)
            {
                throw new ArgumentException("Input tensor must be 2-dimensional");
            }

            int rows = tensor.Shape[0];
            int cols = tensor.Shape[1];
            _label = Tensor<int>.Empty(rows,cols);

            int currentLabel = 1;
            var equivalenceTable = new Dictionary<int, int>();

            // First pass: assign temporary labels and record equivalences
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (tensor[i, j].item != backgroundValue)
                    {
                        var neighbors = GetNeighbors(i, j, rows, cols)
                            .Select(p => _label[p.Item1, p.Item2].item)
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
                    if (_label[i, j].item != 0)
                    {
                        _label[i, j] = Find(equivalenceTable, _label[i, j].item);
                    }
                }
            }

            // Collect unique labels (excluding background)
            _labelKinds = _label.Unique.Where(l => l != 0).OrderBy(l => l).ToList();
        }
        public List<int> RouteLabels { get { return _labelKinds; } }
        public int GetAreaSize(int label) => (_label==label).Cast<int>().Sum();
        public int GetAreaSize(Indice[] labelPosition) => GetAreaSize(_label[labelPosition].item);

        public void ApplyEachLabel<T>(Action<List<Indice[]>> func) where T : IComparable<T>
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

        public enum AreaMaskSelectMode
        {
            MAX,
            MIN,
            RANDOM
        }

        public (Tensor<bool>, int) GetAreaMask(AreaMaskSelectMode areaMaskSelectMode)
        {
            int selectedLabel = 0;
            if (areaMaskSelectMode == AreaMaskSelectMode.RANDOM)
            {
                selectedLabel = SelectRandom(_labelKinds);
            }
            else
            {
                var areaSizes = _labelKinds.Select(label => (_label == label).Cast<float>().Sum());
                if(areaMaskSelectMode == AreaMaskSelectMode.MAX)
                {
                    var maxAreaSize = areaSizes.Max();
                    selectedLabel = _labelKinds[SelectRandomIndex(areaSizes, maxAreaSize)];
                }
                else if(areaMaskSelectMode == AreaMaskSelectMode.MIN)
                {
                    var minAreaSize = areaSizes.Min();
                    selectedLabel = _labelKinds[SelectRandomIndex(areaSizes, minAreaSize)];
                }
            }
            return (_label == selectedLabel, selectedLabel);
        }

        public void UpdateLabel(Indice[] indices, int setLabel)
        {
            if (setLabel != 0 && !_labelKinds.Contains(setLabel))
            {
                _labelKinds.Append(setLabel);
            }
            _label[indices] = setLabel;
        }
        public void UpdateLabel(int delLabel, int setLabel)
        {
            if (setLabel != 0 && !_labelKinds.Contains(setLabel))
            {
                _labelKinds.Append(setLabel);
            }
            _labelKinds.Remove(delLabel);
            _label[_label == delLabel] = setLabel;
        }
    }

    public class MazeCreater
    {
        public static bool FindRandomPosition<T>(Tensor<T> field, T searchValue, Tensor<bool> mask, out Indice[] indice) where T : IComparable<T>
        {
            var selectMask = mask & (field == searchValue);
            if (selectMask.Any())
            {
                var selectList = selectMask.ArgWhere();
                indice = SelectRandom(selectList);
                return true;
            }
            indice = field.GetIndices(0);
            return false;
        }

        public static bool SetWall(Indice[] pos, Tensor<float> field, int minSize, TensorLabel tensorLabel, out Tensor<float> newField, out TensorLabel newTensorLabel)
        {
            if (field[pos].item == 1f)
            {
                newField = field;
                newTensorLabel = tensorLabel;
                return true;
            }
            if (tensorLabel.GetAreaSize(pos) - 1 < minSize)
            {
                newField = field;
                newTensorLabel = tensorLabel;
                return false;
            }
            var ESE = ESEResult.Get(pos, field);
            var predField = new Tensor<float>(field);
            predField[pos] = 1f;
            var wallLabels = (field == 1f).ArgWhere();
            if (wallLabels.Count == 0)
            {
                newField = predField;
                newTensorLabel = new TensorLabel(predField == 0f);
                return true;
            }
            if (MazeArrayHelper.IsPotentialSplitter(ESE.Extract(field, 1f))) 
            {
                var predTensorLabel = new TensorLabel(predField == 0f);
                foreach(var routelabel in predTensorLabel.RouteLabels)
                {
                    if (predTensorLabel.GetAreaSize(routelabel) < minSize)
                    {
                        newField = field;
                        newTensorLabel = tensorLabel;
                        return false;
                    }
                }
            }
            tensorLabel.UpdateLabel(pos, 0);
            newField = predField;
            newTensorLabel = tensorLabel;
            return true;
        }

        public static bool DeleteWall(Indice[] pos, Tensor<float> field, int minSize, TensorLabel tensorLabel, out Tensor<float> newField, out TensorLabel newTensorLabe)
        {
            if (field[pos].item == 0f)
            {
                newField = field;
                newTensorLabe = tensorLabel;
                return true;
            }
            var ESE = ESEResult.Get(pos, field);
            var extractedField = ESE.Extract(field, 1f);
            var mask = MazeArrayHelper.NeighborBoolMask & (extractedField == 0f);
            if (!mask.Any())
            {
                newField = field;
                newTensorLabe = tensorLabel;
                return false;
            }
            var uniqLabels = ESE.Extract(tensorLabel.Label, 0)[mask].Unique.Where(l => l != 0);
            var replaceLabel = uniqLabels.Min();
            foreach(var label in uniqLabels)
            {
                if (label != replaceLabel)
                {
                    tensorLabel.UpdateLabel(label,replaceLabel);
                }
            }
            tensorLabel.UpdateLabel(pos, replaceLabel);
            newTensorLabe = tensorLabel;
            newField = new(field);
            newField[pos] = 0f;
            return true;
        }
        
    }

    public class MazeArrayHelper
    {
        private static Tensor<float> _rotMasks;
        private static bool _rotMasksCreated;
        private static Tensor<int> _rotLabel;
        private static Tensor<bool> _surroundMask;
        private static List<Indice[]> _surrondMaskIndices;
        private static Tensor<float> _neighborMask;
        private static Tensor<bool> _neighborBoolMask;
        private static Tensor<float> _deltaMask;
        

        public static Tensor<float> RotMasks
        {
            get
            {
                if (!_rotMasksCreated)
                {
                    var s1 = Tensor<float>.FromArray(new[,]{
                        { 1f, 0f, 1f },
                        { 0f, 0f, 1f },
                        { 1f, 1f, 1f }
                    },true);

                    var s2 = Tensor<float>.FromArray(new[,]
                    {
                        { 0f, 1f, 0f },
                        { 0f, 0f, 0f },
                        { 1f ,1f, 1f }
                    }, true);

                    List<Tensor<float>> _rotMasks = new List<Tensor<float>> { s1, s2 };

                    for (int i = 0; i < 3; i++)
                    {
                        s1 = s1.Rotate90();
                        s2 = s2.Rotate90();
                        _rotMasks.Add(s1);
                        _rotMasks.Add(s2);
                    }
                    MazeArrayHelper._rotMasks = Tensor<float>.Stack(_rotMasks, 0);
                    _rotMasksCreated = true;
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
                },true);
            }
        }

        public static Tensor<bool> SurroundMask
        {
            get
            {
                return _surroundMask ??= Tensor<bool>.FromArray(new[,]
                {
                    { true, true,true },
                    { true, false, true },
                    { true, true, true }
                }, true);
            }
        }

        public static List<Indice[]> SurroundMaskIndice
        {
            get
            {
                return _surrondMaskIndices ??= SurroundMask.ArgWhere();
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
                }, true);
            }
        }
        public static Tensor<bool> NeighborBoolMask
        {
            get
            {
                return _neighborBoolMask ??= Tensor<bool>.FromArray(new[,]
                {
                    { false, true, false },
                    { true, false, true },
                    { false, true, false }
                },true);
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
                }, true);
            }
        }

        public static bool IsPotentialSplitter(Tensor<float> extractedField)
        {
            if (extractedField[1, 1].item == 1f) return false;
            var fieldMask = (extractedField == 1f) & SurroundMask;
            if (fieldMask.All()) return false;
            return (fieldMask & (RotMasks[RotLabel[fieldMask]].Sum(new[] { 0 }) > 0)).Any();
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
            tensorLabel.ApplyEachLabel<float>((List<Indice[]> indices) =>
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
            var maxPeaks = difficultyScore.GetPeak2D(mask, PeakMode.Maximum);
            var minPeaks = difficultyScore.GetPeak2D(mask, PeakMode.Minimum);
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
                    MaxSelector = selector & (difficultyScore == difficultyScore[selector].Max());
                    normalizedPeak[MaxSelector] = 2;
                }
                if (MinSelector.Any())
                {
                    normalizedPeak[MinSelector] = -((-difficultyScore[MinSelector]).MinMaxNormalize() + 1f);
                }
                else
                {
                    MinSelector = selector & (difficultyScore == difficultyScore[selector].Min());
                    normalizedPeak[MinSelector] = -2;
                }
            }

            tensorLabel.ApplyEachLabel<float>(Norm);
            return (maxPeaks, minPeaks, normalizedPeak);
        }
    }

    internal class SLSequential
    {
        private static System.Random _random;
        public static System.Random Random => _random ??= new System.Random();
        public static T SelectRandom<T>(List<T> pool) => pool[Random.Next(pool.Count)];
        public static T SelectRandom<T>(T[] pool) => pool[Random.Next(pool.Length)];
        public static int SelectRandomIndex<T>(IEnumerable<T> pool, T value) where T : IComparable<T>
        {
            var selectList = pool.Select((v,i) => (v,i)).Where((v) => v.v.Equals(value));
            return SelectRandom(selectList.Select((v) => v.i).ToList());
        }
    }
}