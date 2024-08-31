
using System.Collections.Generic;
using System;
using System.Linq;
using static SL.Lib.TensorPadding;

namespace SL.Lib
{
    public static class ArgSelector
    {
        private static Random _random;
        public static Random Random
        {
            get
            {
                return (_random ??= new Random());
            }
        }

        public enum ArgSelect
        {
            FIRST,
            LAST,
            RANDOM
        }

        public static TValue Select<TValue>(List<TValue> list, ArgSelect argSelect)
        {
            return list[argSelect switch
            {
                ArgSelect.FIRST => 0,
                ArgSelect.LAST => list.Count - 1,
                ArgSelect.RANDOM => Random.Next(0, list.Count),
                _ => throw new Exception()
            }];
        }
    }
    public static class NumericOperations
    {
        /// <summary>
        /// 二つの値を加算します。
        /// </summary>
        /// <typeparam name="T">操作する値の型</typeparam>
        /// <param name="a">最初の値</param>
        /// <param name="b">二番目の値</param>
        /// <returns>加算結果</returns>
        public static T Add<T>(T a, T b)
        {
            if (typeof(T) == typeof(float))
                return (T)(object)((float)(object)a + (float)(object)b);
            if (typeof(T) == typeof(double))
                return (T)(object)((double)(object)a + (double)(object)b);
            if (typeof(T) == typeof(int))
                return (T)(object)((int)(object)a + (int)(object)b);
            if (typeof(T) == typeof(long))
                return (T)(object)((long)(object)a + (long)(object)b);
            if (typeof(T) == typeof(decimal))
                return (T)(object)((decimal)(object)a + (decimal)(object)b);

            // その他の数値型や特殊な型の場合
            try
            {
                return (T)Convert.ChangeType(
                    Convert.ToDouble(a) + Convert.ToDouble(b),
                    typeof(T));
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Type {typeof(T).Name} is not supported for addition.", ex);
            }
        }

        /// <summary>
        /// 二つの値を乗算します。
        /// </summary>
        /// <typeparam name="T">操作する値の型</typeparam>
        /// <param name="a">最初の値</param>
        /// <param name="b">二番目の値</param>
        /// <returns>乗算結果</returns>
        public static T Multiply<T>(T a, T b)
        {
            if (typeof(T) == typeof(float))
                return (T)(object)((float)(object)a * (float)(object)b);
            if (typeof(T) == typeof(double))
                return (T)(object)((double)(object)a * (double)(object)b);
            if (typeof(T) == typeof(int))
                return (T)(object)((int)(object)a * (int)(object)b);
            if (typeof(T) == typeof(long))
                return (T)(object)((long)(object)a * (long)(object)b);
            if (typeof(T) == typeof(decimal))
                return (T)(object)((decimal)(object)a * (decimal)(object)b);

            // その他の数値型や特殊な型の場合
            try
            {
                return (T)Convert.ChangeType(
                    Convert.ToDouble(a) * Convert.ToDouble(b),
                    typeof(T));
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Type {typeof(T).Name} is not supported for multiplication.", ex);
            }
        }

        /// <summary>
        /// 二つの値の差を計算します。
        /// </summary>
        /// <typeparam name="T">操作する値の型</typeparam>
        /// <param name="a">最初の値</param>
        /// <param name="b">二番目の値</param>
        /// <returns>減算結果</returns>
        public static T Subtract<T>(T a, T b)
        {
            if (typeof(T) == typeof(float))
                return (T)(object)((float)(object)a - (float)(object)b);
            if (typeof(T) == typeof(double))
                return (T)(object)((double)(object)a - (double)(object)b);
            if (typeof(T) == typeof(int))
                return (T)(object)((int)(object)a - (int)(object)b);
            if (typeof(T) == typeof(long))
                return (T)(object)((long)(object)a - (long)(object)b);
            if (typeof(T) == typeof(decimal))
                return (T)(object)((decimal)(object)a - (decimal)(object)b);

            // その他の数値型や特殊な型の場合
            try
            {
                return (T)Convert.ChangeType(
                    Convert.ToDouble(a) - Convert.ToDouble(b),
                    typeof(T));
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Type {typeof(T).Name} is not supported for subtraction.", ex);
            }
        }

        /// <summary>
        /// 一つの値を別の値で除算します。
        /// </summary>
        /// <typeparam name="T">操作する値の型</typeparam>
        /// <param name="a">被除数</param>
        /// <param name="b">除数</param>
        /// <returns>除算結果</returns>
        public static T Divide<T>(T a, T b)
        {
            if (typeof(T) == typeof(float))
                return (T)(object)((float)(object)a / (float)(object)b);
            if (typeof(T) == typeof(double))
                return (T)(object)((double)(object)a / (double)(object)b);
            if (typeof(T) == typeof(int))
                return (T)(object)((int)(object)a / (int)(object)b);
            if (typeof(T) == typeof(long))
                return (T)(object)((long)(object)a / (long)(object)b);
            if (typeof(T) == typeof(decimal))
                return (T)(object)((decimal)(object)a / (decimal)(object)b);

            // その他の数値型や特殊な型の場合
            try
            {
                return (T)Convert.ChangeType(
                    Convert.ToDouble(a) / Convert.ToDouble(b),
                    typeof(T));
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Type {typeof(T).Name} is not supported for division.", ex);
            }
        }
        public static T One<T>()
        {
            if (typeof(T) == typeof(int)) return (T)(object)1;
            if (typeof(T) == typeof(float)) return (T)(object)1f;
            if (typeof(T) == typeof(double)) return (T)(object)1.0;
            if (typeof(T) == typeof(decimal)) return (T)(object)1m;
            if (typeof(T) == typeof(bool)) return (T)(object)true;
            throw new NotSupportedException($"Type {typeof(T)} is not supported for One operation.");
        }
        public static T Zero<T>()
        {
            if (typeof(T) == typeof(int)) return (T)(object)0;
            if (typeof(T) == typeof(float)) return (T)(object)0f;
            if (typeof(T) == typeof(double)) return (T)(object)0.0;
            if (typeof(T) == typeof(decimal)) return (T)(object)0m;
            if (typeof(T) == typeof(bool)) return (T)(object)false;
            throw new NotSupportedException($"Type {typeof(T)} is not supported for Zero operation.");
        }
    }



    public static class TensorPadding
    {
        public abstract class PadMode<T> where T : IComparable<T>
        {
            public abstract T GetPaddedValue(Tensor<T> tensor, int[] indices);
        }

        public class ConstantPadMode<T> : PadMode<T> where T : IComparable<T>
        {
            private readonly T constantValue;

            public ConstantPadMode(T constantValue)
            {
                this.constantValue = constantValue;
            }

            public override T GetPaddedValue(Tensor<T> tensor, int[] indices)
            {
                return constantValue;
            }
        }
        // パディングを適用するメソッド
        public static Tensor<T> Pad<T>(this Tensor<T> tensor, int[] padWidths, PadMode<T> padMode) where T : IComparable<T>
        {
            if (padWidths.Length != tensor.Shape.Length)
            {
                throw new ArgumentException("Padding widths must match the number of dimensions.");
            }

            int[] newShape = new int[tensor.Shape.Length];
            for (int i = 0; i < tensor.Shape.Length; i++)
            {
                newShape[i] = tensor.Shape[i] + padWidths[i] * 2;
            }

            Tensor<T> paddedTensor = new Tensor<T>(newShape);

            // 再帰的にパディングを適用
            PadRecursive(tensor, tensor, paddedTensor, padWidths, padMode, new int[tensor.Shape.Length], new int[tensor.Shape.Length], 0);

            return paddedTensor;
        }

        public static Tensor<T> Pad<T>(this Tensor<T> tensor, int padWidth, PadMode<T> padMode) where T : IComparable<T> => tensor.Pad(tensor.Shape.Select(x => padWidth).ToArray(), padMode);

        private static void PadRecursive<T>(this Tensor<T> tensor, Tensor<T> original, Tensor<T> padded, int[] padWidths, PadMode<T> padMode, int[] originalIndices, int[] paddedIndices, int dimension) where T : IComparable<T>
        {
            if (dimension == tensor.Shape.Length)
            {
                padded[paddedIndices] = original[originalIndices];
                return;
            }

            for (int i = 0; i < tensor.Shape[dimension]; i++)
            {
                originalIndices[dimension] = i;
                paddedIndices[dimension] = i + padWidths[dimension];
                PadRecursive(tensor, original, padded, padWidths, padMode, originalIndices, paddedIndices, dimension + 1);
            }

            // 左側のパディング
            for (int i = 0; i < padWidths[dimension]; i++)
            {
                paddedIndices[dimension] = i;
                SetPaddedValue(tensor, padded, padMode, paddedIndices, originalIndices, dimension);
            }

            // 右側のパディング
            for (int i = 0; i < padWidths[dimension]; i++)
            {
                paddedIndices[dimension] = padded.Shape[dimension] - 1 - i;
                SetPaddedValue(tensor, padded, padMode, paddedIndices, originalIndices, dimension);
            }
        }

        private static void SetPaddedValue<T>(this Tensor<T> tensor, Tensor<T> padded, PadMode<T> padMode, int[] paddedIndices, int[] originalIndices, int dimension) where T : IComparable<T>
        {
            if (dimension == tensor.Shape.Length - 1)
            {
                padded[paddedIndices] = padMode.GetPaddedValue(tensor, originalIndices);
            }
            else
            {
                for (int i = 0; i < padded.Shape[dimension + 1]; i++)
                {
                    paddedIndices[dimension + 1] = i;
                    SetPaddedValue(tensor, padded, padMode, paddedIndices, originalIndices, dimension + 1);
                }
            }
        }

        // 畳み込みを適用するメソッド
        public static Tensor<T> Convolve<T>(this Tensor<T> tensor, Tensor<T> kernel, string mode = "valid") where T : IComparable<T>
        {
            if (kernel.Shape.Length != tensor.Shape.Length)
            {
                throw new ArgumentException("Kernel must have the same number of dimensions as the tensor.");
            }

            int[] outputShape = CalculateOutputShape(tensor, kernel.Shape, mode);
            Tensor<T> result = new Tensor<T>(outputShape);

            // パディングの計算
            int[] padWidths = CalculatePadWidths(tensor, kernel.Shape, mode);
            Tensor<T> paddedInput = tensor;
            if (padWidths.Any(p => p > 0))
            {
                paddedInput = Pad(tensor, padWidths, new ConstantPadMode<T>(default));
            }

            // 再帰的に畳み込みを適用
            ConvolveRecursive(tensor, paddedInput, kernel, result, new int[tensor.Shape.Length], new int[tensor.Shape.Length], 0);

            return result;
        }

        private static void ConvolveRecursive<T>(this Tensor<T> tensor, Tensor<T> input, Tensor<T> kernel, Tensor<T> result, int[] inputIndices, int[] resultIndices, int dimension) where T : IComparable<T>
        {
            if (dimension == tensor.Shape.Length)
            {
                result[resultIndices] = ComputeConvolutionAtPosition(tensor, input, kernel, inputIndices);
                return;
            }

            for (int i = 0; i < result.Shape[dimension]; i++)
            {
                resultIndices[dimension] = i;
                inputIndices[dimension] = i;
                ConvolveRecursive(tensor, input, kernel, result, inputIndices, resultIndices, dimension + 1);
            }
        }

        private static T ComputeConvolutionAtPosition<T>(this Tensor<T> tensor, Tensor<T> input, Tensor<T> kernel, int[] position) where T : IComparable<T>
        {
            T sum = default(T);
            ConvolveAtPositionRecursive(tensor, input, kernel, position, new int[tensor.Shape.Length], ref sum, 0);
            return sum;
        }

        private static void ConvolveAtPositionRecursive<T>(this Tensor<T> tensor, Tensor<T> input, Tensor<T> kernel, int[] position, int[] kernelIndices, ref T sum, int dimension) where T : IComparable<T>
        {
            if (dimension == tensor.Shape.Length)
            {
                int[] inputIndices = new int[tensor.Shape.Length];
                for (int i = 0; i < tensor.Shape.Length; i++)
                {
                    inputIndices[i] = position[i] + kernelIndices[i];
                }
                sum = NumericOperations.Add(sum, NumericOperations.Multiply(input[inputIndices], kernel[kernelIndices]));
                return;
            }

            for (int i = 0; i < kernel.Shape[dimension]; i++)
            {
                kernelIndices[dimension] = i;
                ConvolveAtPositionRecursive(tensor, input, kernel, position, kernelIndices, ref sum, dimension + 1);
            }
        }

        private static int[] CalculateOutputShape<T>(this Tensor<T> tensor, int[] kernelShape, string mode) where T : IComparable<T>
        {
            int[] outputShape = new int[tensor.Shape.Length];
            for (int i = 0; i < tensor.Shape.Length; i++)
            {
                outputShape[i] = mode.ToLower() switch
                {
                    "valid" => tensor.Shape[i] - kernelShape[i] + 1,
                    "same" => tensor.Shape[i],
                    "full" => tensor.Shape[i] + kernelShape[i] - 1,
                    _ => throw new ArgumentException("Invalid convolution mode. Use 'valid', 'same', or 'full'.")
                };
            }
            return outputShape;
        }

        private static int[] CalculatePadWidths<T>(this Tensor<T> tensor, int[] kernelShape, string mode) where T : IComparable<T>
        {
            int[] padWidths = new int[tensor.Shape.Length];
            for (int i = 0; i < tensor.Shape.Length; i++)
            {
                padWidths[i] = mode.ToLower() switch
                {
                    "valid" => 0,
                    "same" => (kernelShape[i] - 1) / 2,
                    "full" => kernelShape[i] - 1,
                    _ => throw new ArgumentException("Invalid convolution mode. Use 'valid', 'same', or 'full'.")
                };
            }
            return padWidths;
        }

    }

    public static class TensorExtension
    {
        public static List<int> ArgWhere(this Tensor<bool> condition, ArgSelector.ArgSelect selectMode = ArgSelector.ArgSelect.FIRST)
        {
            List<int> indices = new();
            for (int i = 0; i < condition.Size(); i++)
            {
                if (condition[i])
                {
                    indices.Add(i);
                }
            }
            return indices;
        }
        public static bool Any(this Tensor<bool> condition)
        {
            List<int> indices = new();
            for (int i = 0; i < condition.Size(); i++)
            {
                if (condition[i])
                {
                    return true;
                }
            }
            return false;
        }
        public static bool All(this Tensor<bool> condition)
        {
            List<int> indices = new();
            for (int i = 0; i < condition.Size(); i++)
            {
                if (!condition[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
    public static class TensorPeakDetection
    {
        public enum PeakMode
        {
            Maximum,
            Minimum
        }

        public static Tensor<bool> GetPeak2D<T>(this Tensor<T> tensor, Tensor<bool> mask, PeakMode mode = PeakMode.Maximum) where T : IComparable<T>
        {
            if (tensor.Shape.Length != 2)
            {
                throw new ArgumentException("Input tensor must be 2-dimensional");
            }

            int rows = tensor.Shape[0];
            int cols = tensor.Shape[1];

            // Apply mask if provided
            Tensor<T> A = tensor;
            if (!tensor.MatchShape(mask))
            {
                throw new ArgumentException("Mask shape must match tensor shape");
            }
            T minValue = tensor.Min;
            A = tensor.Map((value, indices) => mask[indices] ? value : minValue);

            // Pad the tensor
            Tensor<T> paddedA = PadTensor(A, mode);

            // Calculate differences
            Tensor<int> dy = Diff(paddedA, 0);
            Tensor<int> dx = Diff(paddedA, 1);

            // Calculate sign changes
            Tensor<int> P_dy = SignDiff(dy, 0);
            Tensor<int> P_dx = SignDiff(dx, 1);

            // Detect peaks
            Tensor<bool> Res;
            if (mode == PeakMode.Maximum)
            {
                Res = P_dy.Map((value, indices) => value == -2 && P_dx[indices] == -2);
            }
            else // Minimum
            {
                Res = P_dy.Map((value, indices) => value == 2 && P_dx[indices] == 2);
            }

            // Apply mask if provided
            Res = Res.Map((value, indices) => value && mask[indices]);

            return Res;
        }

        private static Tensor<T> PadTensor<T>(Tensor<T> tensor, PeakMode mode) where T : IComparable<T>
        {
            T padValue = mode == PeakMode.Maximum ? tensor.Min : tensor.Max;
            return tensor.Pad(1, new ConstantPadMode<T>(padValue));
        }

        private static Tensor<int> Diff<T>(Tensor<T> tensor, int axis) where T : IComparable<T>
        {
            int rows = tensor.Shape[0];
            int cols = tensor.Shape[1];
            Tensor<int> result = new Tensor<int>(axis == 0 ? rows - 1 : rows, axis == 1 ? cols - 1 : cols);

            for (int i = 0; i < result.Shape[0]; i++)
            {
                for (int j = 0; j < result.Shape[1]; j++)
                {
                    T current = tensor[axis == 0 ? i + 1 : i, axis == 1 ? j + 1 : j];
                    T previous = tensor[i, j];
                    result[i, j] = current.CompareTo(previous);
                }
            }

            return result;
        }

        private static Tensor<int> SignDiff(Tensor<int> tensor, int axis)
        {
            int rows = tensor.Shape[0];
            int cols = tensor.Shape[1];
            Tensor<int> result = new Tensor<int>(axis == 0 ? rows - 1 : rows, axis == 1 ? cols - 1 : cols);

            for (int i = 0; i < result.Shape[0]; i++)
            {
                for (int j = 0; j < result.Shape[1]; j++)
                {
                    int current = tensor[axis == 0 ? i + 1 : i, axis == 1 ? j + 1 : j];
                    int previous = tensor[i, j];
                    result[i, j] = Math.Sign(current) - Math.Sign(previous);
                }
            }

            return result;
        }
    }

    public partial class Tensor<T> where T : IComparable<T>
    {
        public T Sum()
        {
            return _data.Aggregate((x, y) => NumericOperations.Add(x, y));
        }

        public float Mean()
        {
            T sum = Sum();
            return Convert.ToSingle(sum) / Size();
        }

        public Tensor<float> MinMaxNormalize()
        {
            T min = Min;
            T max = Max;

            if (min.CompareTo(max) == 0)
            {
                // 全ての要素が同じ値の場合、0で埋めたテンソルを返す
                return new Tensor<float>(Enumerable.Repeat(0f, Size()).ToArray(), Shape);
            }

            return Map(x =>
            {
                float value = Convert.ToSingle(x);
                float minValue = Convert.ToSingle(min);
                float maxValue = Convert.ToSingle(max);
                return (value - minValue) / (maxValue - minValue);
            });
        }

        public Tensor<float> RN2C()
        {
            float mean = Mean();

            // Step 1: センタリング
            var centered = Map(x => Convert.ToSingle(x) - mean);

            // Step 2: Min-Max正規化
            var normalized = centered.MinMaxNormalize();

            // Step 3: [-1, 1]範囲への変換
            var scaled = normalized.Map(x => 2 * x - 1);

            // Step 4: 再センタリング
            float scaledMean = scaled.Mean();
            return scaled.Map(x => x - scaledMean);
        }
    }
}
