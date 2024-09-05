using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using static SL.Lib.SLRandom;

namespace SL.Lib
{
    /// <summary>
    /// Represents a 3x3 region of a tensor centered at a specified position.
    /// This class helps in efficiently extracting and manipulating a local area of a larger tensor.
    /// </summary>
    public class ESEResult
    {
        private readonly Range _rowRange;
        private readonly Range _colRange;
        private readonly (int row, int col) _center;
        private readonly int _sourceRows;
        private readonly int _sourceCols;

        /// <summary>
        /// Creates an ESEResult instance for the given position and tensor.
        /// </summary>
        /// <typeparam name="T">The type of elements in the tensor.</typeparam>
        /// <param name="pos">The center position as an array of two Indices.</param>
        /// <param name="tensor">The source 2D tensor.</param>
        /// <returns>An ESEResult instance representing the 3x3 area around the specified position.</returns>
        public static ESEResult Get<T>(Indice[] pos, Tensor<T> tensor) where T : IComparable<T>
        {
            if (pos == null || pos.Length != 2)
                throw new ArgumentException("Position must be an array of two Indice objects.", nameof(pos));

            if (tensor.ndim != 2)
                throw new ArgumentException("Tensor must be 2D.", nameof(tensor));

            int rows = tensor.Shape[0];
            int cols = tensor.Shape[1];
            var realpos = tensor.GetRealIndices(pos);
            int centerRow = realpos[0][0];
            int centerCol = realpos[1][0];

            return new ESEResult(
                (centerRow, centerCol),
                Math.Max(0, centerRow - 1)..Math.Min(rows, centerRow + 2),
                Math.Max(0, centerCol - 1)..Math.Min(cols, centerCol + 2),
                rows,
                cols
            );
        }

        private ESEResult((int, int) center, Range rowRange, Range colRange, int sourceRows, int sourceCols)
        {
            _center = center;
            _rowRange = rowRange;
            _colRange = colRange;
            _sourceRows = sourceRows;
            _sourceCols = sourceCols;
        }

        /// <summary>
        /// Extracts the 3x3 region from the source tensor, padding with the specified value if necessary.
        /// </summary>
        /// <typeparam name="T">The type of elements in the tensor.</typeparam>
        /// <param name="source">The source tensor to extract from.</param>
        /// <param name="padValue">The value to use for padding when the 3x3 region extends beyond the source tensor.</param>
        /// <returns>A 3x3 tensor containing the extracted region.</returns>
        public Tensor<T> Extract<T>(Tensor<T> source, T padValue) where T : IComparable<T>
        {
            if (source.ndim != 2 || source.Shape[0] != _sourceRows || source.Shape[1] != _sourceCols)
                throw new ArgumentException("Source tensor must match the dimensions of the original tensor.", nameof(source));

            var extracted = source[_rowRange, _colRange];
            var result = Tensor<T>.Full(padValue, 3, 3);

            int resultStartRow = 1 - (_center.row - _rowRange.Start.Value);
            int resultStartCol = 1 - (_center.col - _colRange.Start.Value);
            int extractedRows = extracted.Shape[0];
            int extractedCols = extracted.Shape[1];

            result[resultStartRow..(resultStartRow + extractedRows), resultStartCol..(resultStartCol + extractedCols)] = extracted;

            return result;
        }

        /// <summary>
        /// Gets the center position of the 3x3 region in the original tensor.
        /// </summary>
        public (int row, int col) Center => _center;

        /// <summary>
        /// Gets the range of rows covered by this 3x3 region in the original tensor.
        /// </summary>
        public Range RowRange => _rowRange;

        /// <summary>
        /// Gets the range of columns covered by this 3x3 region in the original tensor.
        /// </summary>
        public Range ColRange => _colRange;

        /// <summary>
        /// Returns a string representation of the ESEResult.
        /// </summary>
        public override string ToString()
        {
            return $"ESEResult: Center({_center.row}, {_center.col}), RowRange({_rowRange}), ColRange({_colRange})";
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
            if (tensor.ndim != 2)
            {
                throw new ArgumentException("Input tensor must be 2-dimensional");
            }

            var CCL = new ConnectedComponentLabeling();
            _label = CCL.LabelImage(tensor != backgroundValue);
            //Debug.Log(_label);
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
        public void ApplyEachLabel<T>(Action<Tensor<bool>, int> func) where T : IComparable<T>
        {
            foreach (var kind in _labelKinds)
            {
                func(_label == kind, kind);
            }
        }

        public enum AreaMaskSelectMode
        {
            MAX,
            MIN,
            RANDOM
        }

        public Tensor<bool> GetAreaMask(AreaMaskSelectMode areaMaskSelectMode)
        {
            int selectedLabel = 0;
            if (areaMaskSelectMode == AreaMaskSelectMode.RANDOM)
            {
                selectedLabel = SelectRandom(_labelKinds);
            }
            else
            {
                var areaSizes = _labelKinds.Select(label => GetAreaSize(label));
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
            return _label == selectedLabel;
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

        public static bool SetWall(Indice[] pos, Tensor<int> field, int minSize, TensorLabel tensorLabel, out Tensor<int> newField, out TensorLabel newTensorLabel)
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
            var predField = new Tensor<int>(field);
            predField[pos] = 1;
            var wallLabels = (field == 1).ArgWhere();
            if (wallLabels.Count == 0)
            {
                newField = predField;
                newTensorLabel = new TensorLabel(predField == 0);
                return true;
            }
            //Debug.Log(field);
            //Debug.Log(ESE);
            if (MazeArrayHelper.IsPotentialSplitter(ESE.Extract(field, 1))) 
            {
                var predTensorLabel = new TensorLabel(predField == 0);
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

        public static bool DeleteWall(Indice[] pos, Tensor<int> field, int minSize, TensorLabel tensorLabel, out Tensor<int> newField, out TensorLabel newTensorLabe)
        {
            if (field[pos].item == 0f)
            {
                newField = field;
                newTensorLabe = tensorLabel;
                return true;
            }
            var ESE = ESEResult.Get(pos, field);
            var extractedField = ESE.Extract(field, 1);
            var mask = MazeArrayHelper.NeighborBoolMask & (extractedField == 0);
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
            newField[pos] = 0;
            return true;
        }

        public enum MazeBaseTile
        {
            ROUTE = 0,
            WALL = 1
        }

        public static (Tensor<int>, TensorLabel) AutoSet(Tensor<int> field, MazeBaseTile tile, int minSize, TensorLabel tensorLabel)
        {
            Tensor<bool> fieldMask;
            if (tile == MazeBaseTile.ROUTE)
            {
                fieldMask = field == (int)MazeBaseTile.WALL;
            }
            else
            {
                fieldMask = tensorLabel.GetAreaMask(TensorLabel.AreaMaskSelectMode.MAX);
            }
            if (FindRandomPosition(field, 1 - (int)tile, fieldMask, out Indice[] pos))
            {
                if (tile == MazeBaseTile.ROUTE)
                {
                    DeleteWall(pos, field, minSize, tensorLabel, out Tensor<int> newField, out TensorLabel newTensorLabel);
                    return (newField, newTensorLabel);
                }
                else
                {
                    SetWall(pos, field, minSize, tensorLabel, out Tensor<int> newField, out TensorLabel newTensorLabel);
                    return (newField, newTensorLabel);
                }
            }
            return (field, tensorLabel);
        }

        public static (Tensor<int>, TensorLabel) AutoSetting(Tensor<int> field, int minSize, int numIterations, TensorLabel tensorLabel)
        {
            for(int i = 0; i < numIterations; i++)
            {
                (field, tensorLabel) = AutoSet(field, Choice(new[] { MazeBaseTile.ROUTE, MazeBaseTile.WALL }, new[] { 0.1f, 1.0f }), minSize, tensorLabel);
            }
            tensorLabel = new TensorLabel(field == 0);
            //Debug.Log($"{tensorLabel.Label}");
            return (field, tensorLabel);
        }

        public static (Tensor<int>, TensorLabel) CreatePlainMaze(int width, int height, int minSize)
        {
            var maze = Tensor<int>.Zeros(width, height);
            return AutoSetting(maze, minSize, 300, new TensorLabel(maze == 0));
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
                    //Debug.Log($"RotMasks: {MazeArrayHelper._rotMasks}");
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

        public static bool IsPotentialSplitter(Tensor<int> extractedField)
        {
            if (extractedField[1, 1] == 1) return false;
            var fieldMask = (extractedField == 1) & SurroundMask;
            if (!fieldMask.Any()) return false;
            //Debug.Log($"fieldMask={fieldMask}");
            var rotLabel = RotLabel[fieldMask];
            //Debug.Log($"rotLabel={rotLabel}");
            var rotMasks = RotMasks[rotLabel];
            //Debug.Log($"rotMasks={rotMasks}");
            return (fieldMask & (rotMasks.Sum(new[] { 0 }) > 0)).Any();
        }

        public static Tensor<float> NeighborScore(Tensor<int> field)
        {
            var padded = (1 - field).Pad(1, new ConstantPadMode<int>(0)).Cast<float>();
            var mask = field == 0;
            var R = Tensor<float>.Zeros(field);
            var conv = padded.Convolve(NeighborMask);
            Debug.Log($"Convolved: {conv}");
            R[mask] =conv[mask];
            return R;
        }

        public static Tensor<float> DeltaScore(Tensor<int> field, Tensor<float> neighborScore)
        {
            var mask = field == 0;
            var R = Tensor<float>.Zeros(field);
            var paddedNeighborScore = neighborScore.Pad(1, new ConstantPadMode<float>(0f));
            R[mask] = paddedNeighborScore.Convolve(DeltaMask)[mask];
            return R;
        }

        public static Tensor<float> Difficulty(Tensor<int> field, Tensor<float> neighborScore, Tensor<float> deltaScore, TensorLabel tensorLabel, int steps = 100, float alpha = 0.5f)
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

        public static Tensor<float> DifficultyPeaks(Tensor<int> field, Tensor<float> difficultyScore, TensorLabel tensorLabel)
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
            return normalizedPeak;
        }

        public static Tensor<float> Fluid(Tensor<float> sources, Tensor<float> stables, Tensor<bool> mask, float fmin, float fmax, int maxSteps = 1000, float _alpha=0.5f)
        {
            float alpha = Mathf.Min(_alpha, 1.0f);
            var floatMask = mask.Cast<float>();
            var floatPadMode = new ConstantPadMode<float>(0);
            (int i, int j)[] rollOffsets = (new[] { (0, 1), (0, -1), (1, 0), (-1, 0) });
            Tensor<float> countTable = Tensor<float>.Stack(rollOffsets.Select(ij => floatMask.Slide(ij.i,ij.j,floatPadMode)),0).Sum(0) * floatMask;
            var updatedChecker = ((stables != 0) & (countTable > 0)).Cast<float>();
            var mulCountTable = 1 / (countTable + 1);
            var fluidCurrent = new Tensor<float>(sources);
            var thresFill = sources.Mean() * 0.2f + sources.Min() * 0.8f;
            var filledSteps = Tensor<int>.Full(-1, sources);
            var filled = !mask;
            Tensor<float> currentFluid;
            Tensor<float> neighborSum;
            Tensor<bool> newFilled;
            for(int step = 0; step < maxSteps; step++)
            {
                newFilled = (filledSteps == -1) & (fluidCurrent > thresFill);
                filledSteps[newFilled] = step;
                filled |= newFilled;
                if (filled.All()) break;
                currentFluid = fluidCurrent * stables;
                neighborSum = Tensor<float>.Stack(rollOffsets.Select(ij => (currentFluid * floatMask).Slide(ij.i, ij.j, floatPadMode)), 0).Sum(0);
                fluidCurrent += alpha * (neighborSum - currentFluid * countTable) * updatedChecker * mulCountTable;
                fluidCurrent = fluidCurrent.Clip(fmin, fmax);
            }
            filledSteps[(filledSteps == -1) & mask] = filledSteps.Max() + 1;
            return filledSteps.MaxNormalize();
        }

        public static Tensor<float> FluidDifficulty(Tensor<int> field, Tensor<float> normalizedPeak, Tensor<float> deltaScore, TensorLabel tensorLabel, int maxSteps = 1000, float sourceAmount=3.0f)
        {
            var stable = (deltaScore.MinMaxNormalize() + 1f) * 0.5f;
            var maxPeak = normalizedPeak > 0;
            var sources = Tensor<float>.Zeros(field);

            void dNorm(Tensor<bool> selector)
            {
                stable[selector] += normalizedPeak[selector] * 0.1f;
                var selectorPeak = selector & maxPeak;
                sources[selectorPeak] = normalizedPeak[selectorPeak] * sourceAmount * 0.5f;
            }
            tensorLabel.ApplyEachLabel<float>(dNorm);

            return Fluid(sources, stable, field == 0, -sourceAmount, sourceAmount, maxSteps);
        }

        public static Dictionary<int, (List<Indice[]>, List<Indice[]>)> GetStartAndGoal(Tensor<float> normalizedFilledSteps, Tensor<float> normalizedPreak, TensorLabel tensorLabel)
        {
            Dictionary<int, (List<Indice[]>, List<Indice[]>)> points = new();
            void setStartAndGoal(Tensor<bool> selector, int label)
            {
                var maxPoints = normalizedFilledSteps.MinMask(selector);
                maxPoints = normalizedPreak.MaxMask(maxPoints);
                var minPoints = normalizedFilledSteps.MaxMask(selector);
                minPoints = normalizedPreak.MinMask(minPoints);
                points[label] = (minPoints.ArgWhere(), maxPoints.ArgWhere());
            }
            tensorLabel.ApplyEachLabel<float>(setStartAndGoal);
            return points;
        }

        public static Dictionary<int, (List<Indice[]>, List<Indice[]>)> GetStartAndGoal(Tensor<int> field, TensorLabel tensorLabel)
        {
            var neighborScore = NeighborScore(field);
            var deltaScore = DeltaScore(field,neighborScore);
            var difficultyScore = Difficulty(field, neighborScore, deltaScore, tensorLabel, 5000);
            var difficultyPeaks = DifficultyPeaks(field, difficultyScore, tensorLabel);
            var fluidResult = FluidDifficulty(field, difficultyPeaks, deltaScore, tensorLabel, 5000);
            return GetStartAndGoal(fluidResult, difficultyPeaks, tensorLabel);
        }
        public static IEnumerator GetStartAndGoalAsync(Tensor<int> field, TensorLabel tensorLabel, Action<Dictionary<int, (List<Indice[]>, List<Indice[]>)>> callback)
        {
            Debug.Log("GetStartAndGoalAsync: Starting calculation");

            yield return new WaitForEndOfFrame();

            Debug.Log("GetStartAndGoalAsync: Calculating NeighborScore");
            var neighborScore = NeighborScore(field);
            Debug.Log("GetStartAndGoalAsync: NeighborScore calculation complete");

            yield return new WaitForEndOfFrame();

            Debug.Log("GetStartAndGoalAsync: Calculating DeltaScore");
            var deltaScore = DeltaScore(field, neighborScore);
            Debug.Log("GetStartAndGoalAsync: DeltaScore calculation complete");

            yield return new WaitForEndOfFrame();

            Debug.Log("GetStartAndGoalAsync: Starting DifficultyAsync calculation");
            var difficultyScore = Tensor<float>.Empty(field.Shape);
            yield return DifficultyAsync(field, neighborScore, deltaScore, tensorLabel, difficultyScore);
            Debug.Log("GetStartAndGoalAsync: DifficultyAsync calculation complete");

            Debug.Log("GetStartAndGoalAsync: Calculating DifficultyPeaks");
            var difficultyPeaks = DifficultyPeaks(field, difficultyScore, tensorLabel);
            Debug.Log("GetStartAndGoalAsync: DifficultyPeaks calculation complete");

            yield return new WaitForEndOfFrame();

            Debug.Log("GetStartAndGoalAsync: Starting FluidDifficultyAsync calculation");
            var fluidResult = Tensor<float>.Empty(field.Shape);
            yield return FluidDifficultyAsync(field, difficultyPeaks, deltaScore, tensorLabel, fluidResult);
            Debug.Log("GetStartAndGoalAsync: FluidDifficultyAsync calculation complete");

            Debug.Log("GetStartAndGoalAsync: Calculating final StartAndGoal");
            var result = GetStartAndGoal(fluidResult, difficultyPeaks, tensorLabel);
            Debug.Log("GetStartAndGoalAsync: StartAndGoal calculation complete");

            callback(result);
            Debug.Log("GetStartAndGoalAsync: Calculation finished and callback invoked");
        }
        public static IEnumerator CalculateStartAndGoalPointsAsync(Tensor<int> baseMap, TensorLabel tensorLabel, Dictionary<int, (List<Indice[]>, List<Indice[]>)> startAndGoalPoints)
        {
            bool calculationComplete = false;
            yield return GetStartAndGoalAsync(baseMap, tensorLabel, result => {
                startAndGoalPoints = result;
                calculationComplete = true;
            });

            while (!calculationComplete)
            {
                yield return null;
            }
        }
        private static IEnumerator DifficultyAsync(Tensor<int> field, Tensor<float> neighborScore, Tensor<float> deltaScore, TensorLabel tensorLabel, Tensor<float> result, int steps = 5000, float alpha = 0.5f)
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

            result = normalizedDeltaScore;
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
                var lap = result.Pad(1, new ConstantPadMode<float>(0f)).Convolve(kernel);
                result[maskedIndice] += lap[maskedIndice] - neighborCount[maskedIndice] * result[maskedIndice];
                result[notMaskedIndice] = 0f;

                if (i % 100 == 0) // 100ステップごとにフレームを譲る
                {
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        private static IEnumerator FluidDifficultyAsync(Tensor<int> field, Tensor<float> normalizedPeak, Tensor<float> deltaScore, TensorLabel tensorLabel, Tensor<float> result, int maxSteps = 5000, float sourceAmount = 3.0f)
        {
            var stable = (deltaScore.MinMaxNormalize() + 1f) * 0.5f;
            var maxPeak = normalizedPeak > 0;
            var sources = Tensor<float>.Zeros(field);

            void dNorm(Tensor<bool> selector)
            {
                stable[selector] += normalizedPeak[selector] * 0.1f;
                var selectorPeak = selector & maxPeak;
                sources[selectorPeak] = normalizedPeak[selectorPeak] * sourceAmount * 0.5f;
            }
            tensorLabel.ApplyEachLabel<float>(dNorm);

            yield return FluidAsync(sources, stable, field == 0, -sourceAmount, sourceAmount, result, maxSteps);
        }

        private static IEnumerator FluidAsync(Tensor<float> sources, Tensor<float> stables, Tensor<bool> mask, float fmin, float fmax, Tensor<float> result, int maxSteps = 1000, float _alpha = 0.5f)
        {
            float alpha = Mathf.Min(_alpha, 1.0f);
            var floatMask = mask.Cast<float>();
            var floatPadMode = new ConstantPadMode<float>(0);
            (int i, int j)[] rollOffsets = (new[] { (0, 1), (0, -1), (1, 0), (-1, 0) });
            Tensor<float> countTable = Tensor<float>.Stack(rollOffsets.Select(ij => floatMask.Slide(ij.i, ij.j, floatPadMode)), 0).Sum(0) * floatMask;
            var updatedChecker = ((stables != 0) & (countTable > 0)).Cast<float>();
            var mulCountTable = 1 / (countTable + 1);
            var fluidCurrent = new Tensor<float>(sources);
            var thresFill = sources.Mean() * 0.2f + sources.Min() * 0.8f;
            var filledSteps = Tensor<int>.Full(-1, sources);
            var filled = !mask;

            for (int step = 0; step < maxSteps; step++)
            {
                var newFilled = (filledSteps == -1) & (fluidCurrent > thresFill);
                filledSteps[newFilled] = step;
                filled |= newFilled;
                if (filled.All()) break;
                var currentFluid = fluidCurrent * stables;
                var neighborSum = Tensor<float>.Stack(rollOffsets.Select(ij => (currentFluid * floatMask).Slide(ij.i, ij.j, floatPadMode)), 0).Sum(0);
                fluidCurrent += alpha * (neighborSum - currentFluid * countTable) * updatedChecker * mulCountTable;
                fluidCurrent = fluidCurrent.Clip(fmin, fmax);

                if (step % 100 == 0) // 100ステップごとにフレームを譲る
                {
                    yield return new WaitForEndOfFrame();
                }
            }
            filledSteps[(filledSteps == -1) & mask] = filledSteps.Max() + 1;
            result = filledSteps.MaxNormalize();
        }
    }

    
}