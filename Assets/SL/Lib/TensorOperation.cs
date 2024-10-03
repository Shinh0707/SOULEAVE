using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SL.Lib
{
    /*
     * Basic Operations
     * it provides tensor operators
     */
    public partial class Tensor<T> where T : IComparable<T>
    {
        // Element-wise operations
        public static Tensor<T> operator +(Tensor<T> a, Tensor<T> b) => ElementwiseOperation(a, b, NumericOperations.Add);
        public static Tensor<T> operator -(Tensor<T> a, Tensor<T> b) => ElementwiseOperation(a, b, NumericOperations.Subtract);
        public static Tensor<T> operator *(Tensor<T> a, Tensor<T> b) => ElementwiseOperation(a, b, NumericOperations.Multiply);
        public static Tensor<T> operator /(Tensor<T> a, Tensor<T> b) => ElementwiseOperation(a, b, NumericOperations.Divide);

        // Scalar operations
        public static Tensor<T> operator +(Tensor<T> a, T scalar) => ElementwiseOperation(a, scalar, NumericOperations.Add);
        public static Tensor<T> operator -(Tensor<T> a, T scalar) => ElementwiseOperation(a, scalar, NumericOperations.Subtract);
        public static Tensor<T> operator *(Tensor<T> a, T scalar) => ElementwiseOperation(a, scalar, NumericOperations.Multiply);
        public static Tensor<T> operator /(Tensor<T> a, T scalar) => ElementwiseOperation(a, scalar, NumericOperations.Divide);

        // Reverse scalar operations
        public static Tensor<T> operator +(T scalar, Tensor<T> a) => ElementwiseOperation(a, scalar, NumericOperations.Add);
        public static Tensor<T> operator -(T scalar, Tensor<T> a) => ElementwiseOperation(a, scalar, (x, y) => NumericOperations.Subtract(y, x));
        public static Tensor<T> operator *(T scalar, Tensor<T> a) => ElementwiseOperation(a, scalar, NumericOperations.Multiply);
        public static Tensor<T> operator /(T scalar, Tensor<T> a) => ElementwiseOperation(a, scalar, (x, y) => NumericOperations.Divide(y, x));

        // Unary minus operator
        public static Tensor<T> operator -(Tensor<T> tensor) => tensor.Map(NumericOperations.Negate);

        // Logical NOT (unary operator)
        public static Tensor<T> operator !(Tensor<T> tensor) => tensor.Map(NumericOperations.Not);

        /// <summary>
        /// Checks if any element in the tensor is true.
        /// </summary>
        /// <returns>True if any element is true, false otherwise.</returns>
        public bool Any()
        {
            foreach (var d in _data)
            {
                if (CastValue<bool>(d)) return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if all elements in the tensor are true.
        /// </summary>
        /// <returns>True if all elements are true, false otherwise.</returns>
        public bool All()
        {
            foreach (var d in _data)
            {
                if (!CastValue<bool>(d)) return false;
            }
            return true;
        }

        // Logical operators
        public static Tensor<bool> operator ==(Tensor<T> left, Tensor<T> right) => ElementwiseOperation(left, right, (a, b) => a.CompareTo(b) == 0);
        public static Tensor<bool> operator ==(Tensor<T> left, T right) => ElementwiseOperation(left, right, (a, b) => a.CompareTo(b) == 0);
        public static Tensor<bool> operator !=(Tensor<T> left, Tensor<T> right) => ElementwiseOperation(left, right, (a, b) => a.CompareTo(b) != 0);
        public static Tensor<bool> operator !=(Tensor<T> left, T right) => ElementwiseOperation(left, right, (a, b) => a.CompareTo(b) != 0);
        public static Tensor<bool> operator >(Tensor<T> left, Tensor<T> right) => ElementwiseOperation(left, right, (a, b) => a.CompareTo(b) > 0);
        public static Tensor<bool> operator >(Tensor<T> left, T right) => ElementwiseOperation(left, right, (a, b) => a.CompareTo(b) > 0);
        public static Tensor<bool> operator <(Tensor<T> left, Tensor<T> right) => ElementwiseOperation(left, right, (a, b) => a.CompareTo(b) < 0);
        public static Tensor<bool> operator <(Tensor<T> left, T right) => ElementwiseOperation(left, right, (a, b) => a.CompareTo(b) < 0);
        public static Tensor<bool> operator >=(Tensor<T> left, Tensor<T> right) => ElementwiseOperation(left, right, (a, b) => a.CompareTo(b) >= 0);
        public static Tensor<bool> operator >=(Tensor<T> left, T right) => ElementwiseOperation(left, right, (a, b) => a.CompareTo(b) >= 0);
        public static Tensor<bool> operator <=(Tensor<T> left, Tensor<T> right) => ElementwiseOperation(left, right, (a, b) => a.CompareTo(b) <= 0);
        public static Tensor<bool> operator <=(Tensor<T> left, T right) => ElementwiseOperation(left, right, (a, b) => a.CompareTo(b) <= 0);

        // Bitwise operators
        public static Tensor<T> operator &(Tensor<T> left, Tensor<T> right) => ElementwiseOperation(left, right, NumericOperations.BitwiseAnd);
        public static Tensor<T> operator |(Tensor<T> left, Tensor<T> right) => ElementwiseOperation(left, right, NumericOperations.BitwiseOr);
        public static Tensor<T> operator ^(Tensor<T> left, Tensor<T> right) => ElementwiseOperation(left, right, NumericOperations.BitwiseXor);

        /// <summary>
        /// Casts the current Tensor to a new Tensor with a different data type.
        /// </summary>
        /// <typeparam name="U">The target data type</typeparam>
        /// <returns>A new Tensor with the casted values</returns>
        public Tensor<U> Cast<U>() where U : IComparable<U>
        {
            // If T and U are the same type, return a copy
            if (typeof(T) == typeof(U))
            {
                return new Tensor<U>((U[])(object)_data.Clone(), Shape);
            }

            // Perform the cast
            U[] newData = new U[_data.Length];
            for (int i = 0; i < _data.Length; i++)
            {
                newData[i] = CastValue<U>(_data[i]);
            }

            return new Tensor<U>(newData, Shape);
        }

        private static U CastValue<U>(T value) where U : IComparable<U>
        {
            try
            {
                if (typeof(U) == typeof(bool))
                {
                    return (U)(object)Convert.ToBoolean(value);
                }
                return (U)Convert.ChangeType(value, typeof(U));
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException($"Unable to cast from {typeof(T).Name} to {typeof(U).Name}");
            }
        }

        private static Tensor<T> ElementwiseOperation(Tensor<T> a, Tensor<T> b, Func<T, T, T> operation)
        {
            //Debug.Log($"operation:{operation.Method.Name}\na={a}\nb={b}");
            Tensor<T> result = Broadcast(a, b, operation);
            //Debug.Log($"result={result}");
            return result;
        }

        private static Tensor<T> ElementwiseOperation(Tensor<T> a, T scalar, Func<T, T, T> operation)
        {
            Tensor<T> result = Empty(a.Shape);

            for (int i = 0; i < a._data.Length; i++)
            {
                result._data[i] = operation(a._data[i], scalar);
            }
            return result;
        }

        private static Tensor<bool> ElementwiseOperation(Tensor<T> a, Tensor<T> b, Func<T, T, bool> operation) => Broadcast(a, b, operation);

        private static Tensor<bool> ElementwiseOperation(Tensor<T> a, T scalar, Func<T, T, bool> operation)
        {
            Tensor<bool> result = Tensor<bool>.Empty(a.Shape);

            for (int i = 0; i < a._data.Length; i++)
            {
                result._data[i] = operation(a._data[i], scalar);
            }
            return result;
        }

        public Tensor<TResult> Map<TResult>(Func<T, TResult> func) where TResult : IComparable<TResult>
        {
            TResult[] newData = new TResult[_data.Length];
            for (int i = 0; i < _data.Length; i++)
            {
                newData[i] = func(_data[i]);
            }
            return new Tensor<TResult>(newData, Shape);
        }

        public Tensor<TResult> Map<TResult>(Func<T, int, TResult> func) where TResult : IComparable<TResult>
        {
            TResult[] newData = new TResult[_data.Length];
            for (int i = 0; i < _data.Length; i++)
            {
                newData[i] = func(_data[i], i);
            }
            return new Tensor<TResult>(newData, Shape);
        }
    }

    public interface INumeric<T>
    {
        public abstract T Zero();
        public abstract T One();
        public abstract T Random(float rate);
    }

    public static class NumericOperations
    {
        public static T Add<T>(T a, T b)
        {
            return PerformOperation(a, b, (float x, float y) => x + y,
                                          (double x, double y) => x + y,
                                          (int x, int y) => x + y,
                                          (long x, long y) => x + y,
                                          (decimal x, decimal y) => x + y,
                                          "addition");
        }

        public static T Multiply<T>(T a, T b)
        {
            return PerformOperation(a, b, (float x, float y) => x * y,
                                          (double x, double y) => x * y,
                                          (int x, int y) => x * y,
                                          (long x, long y) => x * y,
                                          (decimal x, decimal y) => x * y,
                                          "multiplication");
        }

        public static T Subtract<T>(T a, T b)
        {
            return PerformOperation(a, b, (float x, float y) => x - y,
                                          (double x, double y) => x - y,
                                          (int x, int y) => x - y,
                                          (long x, long y) => x - y,
                                          (decimal x, decimal y) => x - y,
                                          "subtraction");
        }

        public static T Divide<T>(T a, T b)
        {
            return PerformOperation(a, b, (float x, float y) => x / y,
                                          (double x, double y) => x / y,
                                          (int x, int y) => x / y,
                                          (long x, long y) => x / y,
                                          (decimal x, decimal y) => x / y,
                                          "division");
        }

        public static T Negate<T>(T value)
        {
            return PerformUnaryOperation(value,
                (float x) => -x,
                (double x) => -x,
                (int x) => -x,
                (long x) => -x,
                (decimal x) => -x,
                "negation");
        }

        public static T Not<T>(T value)
        {
            if (typeof(T) == typeof(bool))
                return (T)(object)!(bool)(object)value;
            return (T)(object)!Convert.ToBoolean(value);
        }

        public static T BitwiseAnd<T>(T a, T b)
        {
            return PerformBitwiseOperation(a, b,
                (bool x, bool y) => x && y,
                (int x, int y) => x & y,
                (long x, long y) => x & y,
                "Bitwise AND");
        }

        public static T BitwiseOr<T>(T a, T b)
        {
            return PerformBitwiseOperation(a, b,
                (bool x, bool y) => x || y,
                (int x, int y) => x | y,
                (long x, long y) => x | y,
                "Bitwise OR");
        }

        public static T BitwiseXor<T>(T a, T b)
        {
            return PerformBitwiseOperation(a, b,
                (bool x, bool y) => x ^ y,
                (int x, int y) => x ^ y,
                (long x, long y) => x ^ y,
                "Bitwise XOR");
        }

        public static T One<T>()
        {
            if (typeof(T) == typeof(int)) return (T)(object)1;
            if (typeof(T) == typeof(float)) return (T)(object)1f;
            if (typeof(T) == typeof(double)) return (T)(object)1.0;
            if (typeof(T) == typeof(decimal)) return (T)(object)1m;
            if (typeof(T) == typeof(bool)) return (T)(object)true;
            if (typeof(INumeric<T>).IsAssignableFrom(typeof(T)))
            {
                // INumeric<T> Çé¿ëïÇµÇƒÇ¢ÇÈå^ÇÃèÍçá
                return ((INumeric<T>)Activator.CreateInstance(typeof(T))).One();
            }
            throw new NotSupportedException($"Type {typeof(T)} is not supported for One operation.");
        }

        public static T Zero<T>()
        {
            if (typeof(T) == typeof(int)) return (T)(object)0;
            if (typeof(T) == typeof(float)) return (T)(object)0f;
            if (typeof(T) == typeof(double)) return (T)(object)0.0;
            if (typeof(T) == typeof(decimal)) return (T)(object)0m;
            if (typeof(T) == typeof(bool)) return (T)(object)false;
            if (typeof(INumeric<T>).IsAssignableFrom(typeof(T)))
            {
                // INumeric<T> Çé¿ëïÇµÇƒÇ¢ÇÈå^ÇÃèÍçá
                return ((INumeric<T>)Activator.CreateInstance(typeof(T))).Zero();
            }
            throw new NotSupportedException($"Type {typeof(T)} is not supported for Zero operation.");
        }
        public static T Random<T>(float rate)
        {
            if (rate < 0 || rate > 1)
                throw new ArgumentOutOfRangeException(nameof(rate), "Rate must be between 0 and 1.");

            var random = SLRandom.Random;

            if (typeof(T) == typeof(int))
                return (T)(object)random.Next((int)(int.MaxValue * rate));

            if (typeof(T) == typeof(float))
                return (T)(object)((float)random.NextDouble() * rate);

            if (typeof(T) == typeof(double))
                return (T)(object)(random.NextDouble() * rate);

            if (typeof(T) == typeof(decimal))
                return (T)(object)((decimal)random.NextDouble() * (decimal)rate);

            if (typeof(T) == typeof(bool))
                return (T)(object)(random.NextDouble() < rate);

            if (typeof(INumeric<T>).IsAssignableFrom(typeof(T)))
            {
                // INumeric<T> Çé¿ëïÇµÇƒÇ¢ÇÈå^ÇÃèÍçá
                return ((INumeric<T>)Activator.CreateInstance(typeof(T))).Random(rate);
            }

            throw new NotSupportedException($"Type {typeof(T)} is not supported for Random operation.");
        }

        private static T PerformOperation<T>(T a, T b,
            Func<float, float, float> floatOp,
            Func<double, double, double> doubleOp,
            Func<int, int, int> intOp,
            Func<long, long, long> longOp,
            Func<decimal, decimal, decimal> decimalOp,
            string operationName)
        {
            if (typeof(T) == typeof(float))
                return (T)(object)floatOp((float)(object)a, (float)(object)b);
            if (typeof(T) == typeof(double))
                return (T)(object)doubleOp((double)(object)a, (double)(object)b);
            if (typeof(T) == typeof(int))
                return (T)(object)intOp((int)(object)a, (int)(object)b);
            if (typeof(T) == typeof(long))
                return (T)(object)longOp((long)(object)a, (long)(object)b);
            if (typeof(T) == typeof(decimal))
                return (T)(object)decimalOp((decimal)(object)a, (decimal)(object)b);

            try
            {
                return (T)Convert.ChangeType(
                    doubleOp(Convert.ToDouble(a), Convert.ToDouble(b)),
                    typeof(T));
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Type {typeof(T).Name} is not supported for {operationName}.", ex);
            }
        }

        private static T PerformUnaryOperation<T>(T value,
            Func<float, float> floatOp,
            Func<double, double> doubleOp,
            Func<int, int> intOp,
            Func<long, long> longOp,
            Func<decimal, decimal> decimalOp,
            string operationName)
        {
            if (typeof(T) == typeof(float))
                return (T)(object)floatOp((float)(object)value);
            if (typeof(T) == typeof(double))
                return (T)(object)doubleOp((double)(object)value);
            if (typeof(T) == typeof(int))
                return (T)(object)intOp((int)(object)value);
            if (typeof(T) == typeof(long))
                return (T)(object)longOp((long)(object)value);
            if (typeof(T) == typeof(decimal))
                return (T)(object)decimalOp((decimal)(object)value);

            try
            {
                return (T)Convert.ChangeType(doubleOp(Convert.ToDouble(value)), typeof(T));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"{operationName} is not supported for type {typeof(T).Name}.", ex);
            }
        }

        private static T PerformBitwiseOperation<T>(T a, T b,
            Func<bool, bool, bool> boolOp,
            Func<int, int, int> intOp,
            Func<long, long, long> longOp,
            string operationName)
        {
            if (typeof(T) == typeof(bool))
                return (T)(object)boolOp((bool)(object)a, (bool)(object)b);
            if (typeof(T) == typeof(int))
                return (T)(object)intOp((int)(object)a, (int)(object)b);
            if (typeof(T) == typeof(long))
                return (T)(object)longOp((long)(object)a, (long)(object)b);

            throw new NotSupportedException($"{operationName} is not supported for type {typeof(T)}");
        }
    }

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
    /*
     * Shape Change Operations
     * Ex. Stack, Concat(Cat), Pad
     */
    public partial class Tensor<T> where T : IComparable<T>
    {
        public static Tensor<T> Stack(IEnumerable<Tensor<T>> tensors, int dim)
        {
            var tensorList = tensors.ToList();
            if (tensorList.Count == 0)
                throw new ArgumentException("The list of tensors cannot be empty.", nameof(tensors));

            var firstTensor = tensorList[0];
            if (dim < 0 || dim > firstTensor.ndim)
                throw new ArgumentOutOfRangeException(nameof(dim), "Dimension is out of range.");

            //Debug.Log($"Stacking {tensorList.Count} tensors along dimension {dim}");

            // Validate shapes and calculate new shape
            int[] newShape = firstTensor.Shape.Insert(0, dim);
            for (int i = 0; i < tensorList.Count; i++)
            {
                if (!tensorList[i].MatchShape(firstTensor))
                    throw new ArgumentException($"Tensor at index {i} has a different shape.");
                newShape[dim] += 1;
            }

            //Debug.Log($"New shape: [{string.Join(", ", newShape)}]");

            // Calculate new strides
            int[] newStrides = new int[newShape.Length];
            int stride = 1;
            for (int i = newShape.Length - 1; i >= 0; i--)
            {
                newStrides[i] = stride;
                stride *= newShape[i];
            }

            //Debug.Log($"New strides: [{string.Join(", ", newStrides)}]");

            // Create result tensor
            T[] newData = new T[stride];
            var result = new Tensor<T>(newData, newShape, newStrides, 0, false, false);

            // Perform stacking
            int[] indices = new int[newShape.Length];
            int[] srcIndices = new int[firstTensor.ndim];
            for (int i = 0; i < newData.Length; i++)
            {
                result.GetIndices(i, indices);
                int tensorIndex = indices[dim];
                Array.Copy(indices, srcIndices, dim);
                Array.Copy(indices, dim + 1, srcIndices, dim, srcIndices.Length - dim);

                //Debug.Log($"Processing element {i}: Result indices: [{string.Join(", ", indices)}], Source indices: [{string.Join(", ", srcIndices)}]");

                int srcOffset = 0;
                for (int j = 0; j < srcIndices.Length; j++)
                {
                    srcOffset += srcIndices[j] * tensorList[tensorIndex].Strides[j];
                }

                //Debug.Log($"Source offset: {srcOffset}, Tensor index: {tensorIndex}");

                newData[i] = tensorList[tensorIndex]._data[srcOffset];
            }

            result._data = newData;
            //Debug.Log("Stacking completed");
            return result;
        }

        /// <summary>
        /// Concatenates a list of tensors along the specified dimension.
        /// </summary>
        /// <param name="tensors">The list of tensors to concatenate.</param>
        /// <param name="dim">The dimension along which to concatenate the tensors.</param>
        /// <returns>A new tensor containing the concatenated tensors.</returns>
        public static Tensor<T> Concat(List<Tensor<T>> tensors, int dim)
        {
            var tensorList = tensors.ToList();
            if (tensorList.Count == 0)
                throw new ArgumentException("The list of tensors cannot be empty.", nameof(tensors));

            var firstTensor = tensorList[0];
            if (dim < 0 || dim >= firstTensor.ndim)
                throw new ArgumentOutOfRangeException(nameof(dim), "Dimension is out of range.");

            // Validate shapes and calculate new shape
            int[] newShape = (int[])firstTensor.Shape.Clone();
            for (int i = 1; i < tensorList.Count; i++)
            {
                if (tensorList[i].ndim != firstTensor.ndim)
                    throw new ArgumentException($"Tensor at index {i} has a different number of dimensions.");

                for (int j = 0; j < newShape.Length; j++)
                {
                    if (j == dim)
                        newShape[j] += tensorList[i].Shape[j];
                    else if (newShape[j] != tensorList[i].Shape[j])
                        throw new ArgumentException($"Tensor at index {i} has incompatible shape.");
                }
            }

            // Calculate new strides
            int[] newStrides = new int[newShape.Length];
            int stride = 1;
            for (int i = newShape.Length - 1; i >= 0; i--)
            {
                newStrides[i] = stride;
                stride *= newShape[i];
            }

            // Create result tensor
            T[] newData = new T[stride];
            var result = new Tensor<T>(newData, newShape, newStrides, 0, false, false);

            // Perform concatenation
            int[] indices = new int[newShape.Length];
            int[] srcIndices = new int[newShape.Length];
            int offset = 0;
            for (int t = 0; t < tensorList.Count; t++)
            {
                var tensor = tensorList[t];
                for (int i = 0; i < tensor.Size(); i++)
                {
                    tensor.GetIndices(i, srcIndices);
                    Array.Copy(srcIndices, indices, newShape.Length);
                    indices[dim] += offset;

                    int destOffset = 0;
                    for (int j = 0; j < indices.Length; j++)
                    {
                        destOffset += indices[j] * newStrides[j];
                    }

                    newData[destOffset] = tensor._data[tensor.Offset + i];
                }
                offset += tensor.Shape[dim];
            }

            return result;
        }

        /// <summary>
        /// Pads a tensor with a specified padding mode.
        /// </summary>
        /// <param name="paddings">A list of tuples specifying the padding for each dimension. Each tuple contains (padding before, padding after).</param>
        /// <param name="padMode">The padding mode to use.</param>
        /// <returns>A new padded tensor.</returns>
        public Tensor<T> Pad(List<(int, int)> paddings, PadMode<T> padMode)
        {
            if (paddings == null || paddings.Count != ndim)
                throw new ArgumentException("Paddings must be specified for each dimension.", nameof(paddings));

            int[] newShape = new int[ndim];
            int[] paddingStart = new int[ndim];
            for (int i = 0; i < ndim; i++)
            {
                newShape[i] = Shape[i] + paddings[i].Item1 + paddings[i].Item2;
                paddingStart[i] = paddings[i].Item1;
            }

            T[] newData = new T[newShape.Aggregate(1, (a, b) => a * b)];
            var result = new Tensor<T>(newData, newShape);

            // Optimize for contiguous memory access
            int fastestChangingDim = ndim - 1;
            int originalFastestDimSize = Shape[fastestChangingDim];
            int newFastestDimSize = newShape[fastestChangingDim];
            int prePadding = paddings[fastestChangingDim].Item1;
            int postPadding = paddings[fastestChangingDim].Item2;

            // Create a buffer for the fastest changing dimension
            T[] lineBuffer = new T[newFastestDimSize];

            // Iterate over all other dimensions
            int[] indices = new int[ndim];
            int[] newIndices = new int[newShape.Length];
            for (int i = 0; i < Size() / originalFastestDimSize; i++)
            {
                // Calculate indices for other dimensions
                int temp = i;
                for (int dim = ndim - 2; dim >= 0; dim--)
                {
                    indices[dim] = temp % Shape[dim];
                    temp /= Shape[dim];
                    newIndices[dim] = indices[dim] + paddingStart[dim];
                }

                // Handle padding for the current line
                for (int j = 0; j < prePadding; j++)
                {
                    newIndices[fastestChangingDim] = j;
                    lineBuffer[j] = padMode.GetPaddedValue(this,newIndices);
                }

                // Copy original data
                Array.Copy(_data, i * originalFastestDimSize, lineBuffer, prePadding, originalFastestDimSize);

                // Handle post-padding
                for (int j = 0; j < postPadding; j++)
                {
                    newIndices[fastestChangingDim] = prePadding + originalFastestDimSize + j;
                    lineBuffer[prePadding + originalFastestDimSize + j] = padMode.GetPaddedValue(this, newIndices);
                }

                // Copy the entire line to the result tensor
                int destIndex = 0;
                for (int dim = 0; dim < newShape.Length - 1; dim++)
                {
                    destIndex = destIndex * newShape[dim] + newIndices[dim];
                }
                destIndex *= newFastestDimSize;
                Array.Copy(lineBuffer, 0, newData, destIndex, newFastestDimSize);
            }

            return result;
        }

        /// <summary>
        /// Pads a tensor with a specified padding mode.
        /// </summary>
        /// <param name="padding">A specifying the padding for each dimension.</param>
        /// <param name="padMode">The padding mode to use.</param>
        /// <returns>A new padded tensor.</returns>
        public Tensor<T> Pad(int padding, PadMode<T> padMode) =>
            Pad(Shape.Select(s => (padding, padding)).ToList(), padMode);

        /// <summary>
        /// Pads a tensor with a specified padding mode.
        /// </summary>
        /// <param name="paddings">A specifying the padding for each dimension.</param>
        /// <param name="padMode">The padding mode to use.</param>
        /// <returns>A new padded tensor.</returns>
        public Tensor<T> Pad((int leftPadding, int rightPadding) paddings, PadMode<T> padMode) =>
            Pad(Shape.Select(s => (paddings.leftPadding, paddings.rightPadding)).ToList(), padMode);
    }

    /*
     * Statistic Operations
     * Operations argument have 'dim' and 'keepdims'. These  arguments are optional.
     * 
     */
    public partial class Tensor<T> where T : IComparable<T>
    {
        /// <summary>
        /// Clips the values in the tensor to be between the specified minimum and maximum values.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>A new tensor with clipped values.</returns>
        public Tensor<T> Clip(T min, T max)
        {
            return Map(x => x.CompareTo(min) < 0 ? min : (x.CompareTo(max) > 0 ? max : x));
        }

        /// <summary>
        /// Applies min-max normalization to the tensor.
        /// </summary>
        /// <returns>A new tensor with normalized values in the range [0, 1].</returns>
        public Tensor<float> MinMaxNormalize()
        {
            T min = Min();
            T max = Max();
            return Map(x => (Convert.ToSingle(x) - Convert.ToSingle(min)) / (Convert.ToSingle(max) - Convert.ToSingle(min)));
        }

        /// <summary>
        /// Applies max normalization to the tensor.
        /// </summary>
        /// <returns>A new tensor with normalized values in the range [0, 1].</returns>
        public Tensor<float> MaxNormalize()
        {
            T max = Max();
            return Map(x => Convert.ToSingle(x) / Convert.ToSingle(max));
        }

        /// <summary>
        /// Applies RN2C normalization to the tensor.
        /// </summary>
        /// <returns>A new tensor with RN2C normalized values.</returns>
        public Tensor<float> RN2C()
        {
            float mean = Mean();
            // Step 1: Centering
            var centered = Map(x => Convert.ToSingle(x) - mean);
            // Step 2: Min-Max normalization
            var normalized = centered.MinMaxNormalize();
            // Step 3: Scale to [-1, 1] range
            var scaled = normalized.Map(x => 2 * x - 1);
            // Step 4: Re-centering
            float scaledMean = scaled.Mean();
            return scaled.Map(x => x - scaledMean);
        }
        /// <summary>
        /// Computes the minimum value along the specified dimensions.
        /// </summary>
        /// <param name="dims">The dimensions along which to compute the minimum. If null, computes the global minimum.</param>
        /// <param name="keepDims">Whether to keep the dimensions with size 1.</param>
        /// <returns>A new tensor containing the minimum values.</returns>
        public Tensor<T> Min(int[] dims = null, bool keepDims = false)
        {
            return ReduceOperation(dims, keepDims, (a, b) => a.CompareTo(b) < 0 ? a : b);
        }

        /// <summary>
        /// Computes the minimum value.
        /// </summary>
        /// <returns>The minimum value.</returns>
        public T Min() => Min(null, false).item;

        /// <summary>
        /// Computes the maximum value along the specified dimensions.
        /// </summary>
        /// <param name="dims">The dimensions along which to compute the maximum. If null, computes the global maximum.</param>
        /// <param name="keepDims">Whether to keep the dimensions with size 1.</param>
        /// <returns>A new tensor containing the maximum values.</returns>
        public Tensor<T> Max(int[] dims = null, bool keepDims = false)
        {
            return ReduceOperation(dims, keepDims, (a, b) => a.CompareTo(b) > 0 ? a : b);
        }

        /// <summary>
        /// Computes the maximum value.
        /// </summary>
        /// <returns>The maximum value.</returns>
        public T Max() => Max(null, false).item;

        /// <summary>
        /// Computes the sum along the specified dimensions.
        /// </summary>
        /// <param name="dims">The dimensions along which to compute the sum. If null, computes the global sum.</param>
        /// <param name="keepDims">Whether to keep the dimensions with size 1.</param>
        /// <returns>A new tensor containing the sum values.</returns>
        public Tensor<T> Sum(int[] dims = null, bool keepDims = false)
        {
            return ReduceOperation(dims, keepDims, NumericOperations.Add);
        }
        /// <summary>
        /// Computes the sum along the specified dimension.
        /// </summary>
        /// <param name="dim">The dimension along which to compute the sum.</param>
        /// <param name="keepDims">Whether to keep the dimensions with size 1.</param>
        /// <returns>A new tensor containing the sum values.</returns>
        public Tensor<T> Sum(int dim, bool keepDims = false) => Sum(new[] { dim }, keepDims);
        /// <summary>
        /// Computes the sum.
        /// </summary>
        /// <returns>The sum value.</returns>
        public T Sum() => Sum(null, false).item;

        /// <summary>
        /// Computes the mean along the specified dimensions.
        /// </summary>
        /// <param name="dims">The dimensions along which to compute the mean. If null, computes the global mean.</param>
        /// <param name="keepDims">Whether to keep the dimensions with size 1.</param>
        /// <returns>A new tensor containing the mean values.</returns>
        public Tensor<float> Mean(int[] dims = null, bool keepDims = false)
        {
            Tensor<T> sum = Sum(dims, keepDims);
            int count = dims == null ? Size() : dims.Aggregate(1, (acc, dim) => acc * Shape[dim]);
            return sum.Map(x => Convert.ToSingle(x) / count);
        }

        /// <summary>
        /// Computes the mean.
        /// </summary>
        /// <returns>The mean value.</returns>
        public float Mean() => Convert.ToSingle(Sum()) / Size();

        public Tensor<T> Diff(int dim = 0)
        {
            if (dim < 0 || dim >= ndim)
                throw new ArgumentOutOfRangeException(nameof(dim), "Dimension is out of range.");

            int[] newShape = (int[])Shape.Clone();
            newShape[dim]--;

            Tensor<T> result = Empty(newShape);

            int[] indices = new int[ndim];
            int[] nextIndices = new int[ndim];

            for (int i = 0; i < result.Size(); i++)
            {
                result.GetIndices(i, indices);
                Array.Copy(indices, nextIndices, indices.Length);
                nextIndices[dim]++;

                int currentIndex = GetFlatIndex(indices);
                int nextIndex = GetFlatIndex(nextIndices);

                result._data[i] = NumericOperations.Subtract(_data[nextIndex], _data[currentIndex]);
            }

            return result;
        }

        private Tensor<T> ReduceOperation(int[] dims, bool keepDims, Func<T, T, T> operation)
        {
            if (dims == null || dims.Length == 0)
            {
                T scalarResult = _data[0];
                for (int i = 1; i < _data.Length; i++)
                {
                    scalarResult = operation(scalarResult, _data[i]);
                }
                return keepDims
                    ? new Tensor<T>(new T[] { scalarResult }, Enumerable.Repeat(1, ndim).ToArray())
                    : new Tensor<T>(new T[] { scalarResult }, new int[] { 1 });
            }

            int[] newShape = keepDims ? Shape.ToArray() : Shape.Where((_, i) => !dims.Contains(i)).ToArray();
            if (keepDims)
            {
                foreach (int dim in dims)
                {
                    newShape[dim] = 1;
                }
            }

            Tensor<T> result = Empty(newShape);
            T[] resultData = result._data;

            int[] indices = new int[ndim];
            int[] resultIndices = new int[newShape.Length];

            for (int i = 0; i < Size(); i++)
            {
                GetIndices(i, indices);
                GetReducedIndices(indices, dims, keepDims, resultIndices);
                int resultIndex = result.GetFlatIndex(resultIndices);

                resultData[resultIndex] = operation(resultData[resultIndex], _data[i]);
            }

            return result;
        }

        private void GetReducedIndices(int[] indices, int[] dims, bool keepDims, int[] resultIndices)
        {
            int resultDim = 0;
            for (int i = 0; i < indices.Length; i++)
            {
                if (!dims.Contains(i))
                {
                    resultIndices[resultDim++] = indices[i];
                }
                else if (keepDims)
                {
                    resultIndices[resultDim++] = 0;
                }
            }
        }
    }
    /// <summary>
    /// Defines the mode for convolution operations.
    /// </summary>
    public enum ConvolveMode
    {
        VALID,
        SAME,
        FULL
    }
    /// <summary>
    /// Defines the mode for peak detection.
    /// </summary>
    public enum PeakMode
    {
        Maximum,
        Minimum
    }
    /*
    * 2D Operations
    * Operations argument have 'rowdim' and 'coldim'( or tuple (int rowdim, int coldim)). These  arguments are optional.
    */
    public partial class Tensor<T> where T : IComparable<T>
    {
        public bool Is2D => ndim == 2;

        public Tensor<T> Convolve<TKernel>(Tensor<TKernel> kernel, ConvolveMode convolveMode, (int rowDim, int colDim) dims)
        where TKernel : IComparable<TKernel>
        {
            if (kernel.ndim != 2)
                throw new ArgumentException("Kernel must be 2D.");

            int rowPad = 0, colPad = 0;
            switch (convolveMode)
            {
                case ConvolveMode.SAME:
                    rowPad = kernel.Shape[0] / 2;
                    colPad = kernel.Shape[1] / 2;
                    break;
                case ConvolveMode.FULL:
                    rowPad = kernel.Shape[0] - 1;
                    colPad = kernel.Shape[1] - 1;
                    break;
            }

            var paddingList = Enumerable.Repeat((0, 0), ndim).ToList();
            paddingList[dims.rowDim] = (rowPad, rowPad);
            paddingList[dims.colDim] = (colPad, colPad);

            Tensor<T> padded = this.Pad(paddingList, new ConstantPadMode<T>(default(T)));

            int[] newShape = Shape.ToArray();
            newShape[dims.rowDim] = padded.Shape[dims.rowDim] - kernel.Shape[0] + 1;
            newShape[dims.colDim] = padded.Shape[dims.colDim] - kernel.Shape[1] + 1;

            Tensor<T> result = Empty(newShape);

            int[] paddedIndices = new int[ndim];
            int[] resultIndices = new int[ndim];

            for (int i = 0; i < result.Size(); i++)
            {
                result.GetIndices(i, resultIndices);
                Array.Copy(resultIndices, paddedIndices, ndim);

                T sum = default(T);
                for (int ki = 0; ki < kernel.Shape[0]; ki++)
                {
                    for (int kj = 0; kj < kernel.Shape[1]; kj++)
                    {
                        paddedIndices[dims.rowDim] = resultIndices[dims.rowDim] + ki;
                        paddedIndices[dims.colDim] = resultIndices[dims.colDim] + kj;

                        T paddedValue = padded._data[padded.GetFlatIndex(paddedIndices)];
                        T kernelValue = (T)Convert.ChangeType(kernel._data[kernel.GetFlatIndex(new[] { ki, kj })], typeof(T));

                        sum = NumericOperations.Add(sum, NumericOperations.Multiply(paddedValue, kernelValue));
                    }
                }
                result._data[i] = sum;
            }

            return result;
        }

        public Tensor<T> Convolve<TKernel>(Tensor<TKernel> kernel, ConvolveMode convolveMode = ConvolveMode.VALID) where TKernel : IComparable<TKernel>
            => Convolve(kernel, convolveMode, (0, 1));

        public Tensor<bool> GetPeak2D(Tensor<bool> mask, PeakMode mode, (int rowDim, int colDim) dims)
        {
            if (!mask.Shape.SequenceEqual(Shape))
                throw new ArgumentException("Mask shape must match tensor shape.");

            Tensor<bool> result = Tensor<bool>.Empty(Shape);

            int[] indices = new int[ndim];
            int[] neighborIndices = new int[ndim];

            for (int i = 0; i < Size(); i++)
            {
                GetIndices(i, indices);

                if (!mask._data[i]) continue;

                bool isPeak = true;
                for (int di = -1; di <= 1 && isPeak; di++)
                {
                    for (int dj = -1; dj <= 1 && isPeak; dj++)
                    {
                        if (di == 0 && dj == 0) continue;

                        Array.Copy(indices, neighborIndices, indices.Length);
                        neighborIndices[dims.rowDim] += di;
                        neighborIndices[dims.colDim] += dj;

                        if (neighborIndices[dims.rowDim] < 0 || neighborIndices[dims.rowDim] >= Shape[dims.rowDim] ||
                            neighborIndices[dims.colDim] < 0 || neighborIndices[dims.colDim] >= Shape[dims.colDim])
                            continue;

                        int neighborIndex = GetFlatIndex(neighborIndices);
                        int comparison = _data[i].CompareTo(_data[neighborIndex]);
                        if ((mode == PeakMode.Maximum && comparison <= 0) ||
                            (mode == PeakMode.Minimum && comparison >= 0))
                        {
                            isPeak = false;
                        }
                    }
                }

                result._data[i] = isPeak;
            }

            return result;
        }

        public Tensor<bool> GetPeak2D(Tensor<bool> mask, PeakMode mode = PeakMode.Maximum) => GetPeak2D(mask, mode, (0, 1));

        public Tensor<T> Rotate90(int rotation, (int rowDim, int colDim) dims)
        {

            rotation = ((rotation % 4) + 4) % 4; // Normalize rotation to be between 0 and 3

            if (rotation == 0)
            {
                return new Tensor<T>(this);
            }

            int[] newShape = Shape.ToArray();
            if (rotation % 2 != 0)
            {
                newShape[dims.rowDim] = Shape[dims.colDim];
                newShape[dims.colDim] = Shape[dims.rowDim];
            }

            Tensor<T> result = Empty(newShape);

            int[] indices = new int[Shape.Length];
            int[] newIndices = new int[Shape.Length];

            for (int i = 0; i < Size(); i++)
            {
                GetIndices(i, indices);
                Array.Copy(indices, newIndices, indices.Length);

                switch (rotation)
                {
                    case 1: // 90 degrees clockwise
                        newIndices[dims.rowDim] = indices[dims.colDim];
                        newIndices[dims.colDim] = Shape[dims.rowDim] - 1 - indices[dims.rowDim];
                        break;
                    case 2: // 180 degrees
                        newIndices[dims.rowDim] = Shape[dims.rowDim] - 1 - indices[dims.rowDim];
                        newIndices[dims.colDim] = Shape[dims.colDim] - 1 - indices[dims.colDim];
                        break;
                    case 3: // 90 degrees counterclockwise
                        newIndices[dims.rowDim] = Shape[dims.colDim] - 1 - indices[dims.colDim];
                        newIndices[dims.colDim] = indices[dims.rowDim];
                        break;
                }

                int originalFlatIndex = i;
                int newFlatIndex = result.GetFlatIndex(newIndices);

                if (newFlatIndex < 0 || newFlatIndex >= result.Size())
                {
                    Debug.LogError($"Error: New flat index {newFlatIndex} is out of range. Result tensor size: {result.Size()}");
                }
                else
                {
                    result._data[newFlatIndex] = _data[originalFlatIndex];
                }
            }
            return result;
        }

        public Tensor<T> Rotate90(int rotation = 1) => Rotate90(rotation, (0, 1));

        public Tensor<T> Roll(int rowOffset, int colOffset, (int rowDim, int colDim) dims)
        {
            Tensor<T> result = Empty(Shape);

            int[] indices = new int[ndim];
            int[] newIndices = new int[ndim];

            for (int i = 0; i < Size(); i++)
            {
                GetIndices(i, indices);
                Array.Copy(indices, newIndices, indices.Length);

                newIndices[dims.rowDim] = (indices[dims.rowDim] + rowOffset + Shape[dims.rowDim]) % Shape[dims.rowDim];
                newIndices[dims.colDim] = (indices[dims.colDim] + colOffset + Shape[dims.colDim]) % Shape[dims.colDim];

                result._data[result.GetFlatIndex(newIndices)] = _data[i];
            }

            return result;
        }

        public Tensor<T> Roll(int rowOffset, int colOffset) => Roll(rowOffset, colOffset, (0, 1));

        public Tensor<T> Slide(int rowOffset, int colOffset, PadMode<T> padMode, (int rowDim, int colDim) dims)
        {
            Tensor<T> result = Empty(Shape);

            int[] indices = new int[ndim];
            int[] srcIndices = new int[ndim];

            for (int i = 0; i < Size(); i++)
            {
                GetIndices(i, indices);
                Array.Copy(indices, srcIndices, indices.Length);

                srcIndices[dims.rowDim] -= rowOffset;
                srcIndices[dims.colDim] -= colOffset;

                if (srcIndices[dims.rowDim] >= 0 && srcIndices[dims.rowDim] < Shape[dims.rowDim] &&
                    srcIndices[dims.colDim] >= 0 && srcIndices[dims.colDim] < Shape[dims.colDim])
                {
                    result._data[i] = _data[GetFlatIndex(srcIndices)];
                }
                else
                {
                    result._data[i] = padMode.GetPaddedValue(this, srcIndices);
                }
            }

            return result;
        }

        public Tensor<T> Slide(int rowOffset, int colOffset, PadMode<T> padMode) => Slide(rowOffset, colOffset, padMode, (0, 1));
    }
}
