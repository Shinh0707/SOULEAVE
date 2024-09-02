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
            int[] broadcastShape = BroadcastShapes(a.Shape, b.Shape);
            Tensor<T> result = Empty(broadcastShape);

            for (int i = 0; i < result._data.Length; i++)
            {
                var indices = result.GetIndices(i);
                T valueA = a.GetBroadcastValue(indices);
                T valueB = b.GetBroadcastValue(indices);
                result._data[i] = operation(valueA, valueB);
            }

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

        private static Tensor<bool> ElementwiseOperation(Tensor<T> a, Tensor<T> b, Func<T, T, bool> operation)
        {
            int[] broadcastShape = BroadcastShapes(a.Shape, b.Shape);
            Tensor<bool> result = Tensor<bool>.Empty(broadcastShape);

            for (int i = 0; i < result._data.Length; i++)
            {
                var indices = result.GetIndices(i);
                T valueA = a.GetBroadcastValue(indices);
                T valueB = b.GetBroadcastValue(indices);
                result._data[i] = operation(valueA, valueB);
            }
            return result;
        }

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
        private T GetBroadcastValue(Indice[] indices)
        {
            if (indices.Length != Shape.Length)
            {
                throw new ArgumentException("Number of indices must match the number of dimensions in the tensor.");
            }

            int[] adjustedIndices = new int[Shape.Length];
            for (int i = 0; i < Shape.Length; i++)
            {
                if (Shape[i] == 1)
                {
                    adjustedIndices[i] = 0;
                }
                else
                {
                    int[] dimIndices = indices[i].GetIndices(Shape[i]);
                    if (dimIndices.Length == 0)
                    {
                        throw new ArgumentException($"Invalid index for dimension {i}");
                    }
                    adjustedIndices[i] = dimIndices[0] % Shape[i];  // Use modulo to handle wrap-around
                }
            }

            return this[adjustedIndices].item;
        }

        private static int[] BroadcastShapes(int[] shapeA, int[] shapeB)
        {
            int rank = Math.Max(shapeA.Length, shapeB.Length);
            int[] result = new int[rank];

            for (int i = 1; i <= rank; i++)
            {
                int dimA = i <= shapeA.Length ? shapeA[^i] : 1;
                int dimB = i <= shapeB.Length ? shapeB[^i] : 1;

                if (dimA == dimB || dimA == 1 || dimB == 1)
                {
                    result[^i] = Math.Max(dimA, dimB);
                }
                else
                {
                    throw new ArgumentException($"Shapes are not compatible for broadcasting at dimension {rank - i}. DimA: {dimA}, DimB: {dimB}");
                }
            }

            return result;
        }
    }
    public static class NumericOperations
    {
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

            // ÇªÇÃëºÇÃêîílå^Ç‚ì¡éÍÇ»å^ÇÃèÍçá
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

            // ÇªÇÃëºÇÃêîílå^Ç‚ì¡éÍÇ»å^ÇÃèÍçá
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

            // ÇªÇÃëºÇÃêîílå^Ç‚ì¡éÍÇ»å^ÇÃèÍçá
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

            // ÇªÇÃëºÇÃêîílå^Ç‚ì¡éÍÇ»å^ÇÃèÍçá
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

        public static T Negate<T>(T value)
        {
            if (typeof(T) == typeof(int))
                return (T)(object)(-(int)(object)value);
            if (typeof(T) == typeof(long))
                return (T)(object)(-(long)(object)value);
            if (typeof(T) == typeof(float))
                return (T)(object)(-(float)(object)value);
            if (typeof(T) == typeof(double))
                return (T)(object)(-(double)(object)value);
            if (typeof(T) == typeof(decimal))
                return (T)(object)(-(decimal)(object)value);

            // ÇªÇÃëºÇÃêîílå^Ç‚ì¡éÍÇ»å^ÇÃèÍçá
            try
            {
                return (T)Convert.ChangeType(-Convert.ToDouble(value), typeof(T));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Negation is not supported for type {typeof(T).Name}.", ex);
            }
        }

        public static T Not<T>(T value)
        {
            if (typeof(T) == typeof(bool))
                return (T)(object)!(bool)(object)value;
            return (T)(object)!Convert.ToBoolean(value);
        }

        // Helper methods for logical operations
        public static T BitwiseAnd<T>(T a, T b)
        {
            if (typeof(T) == typeof(bool))
                return (T)(object)((bool)(object)a & (bool)(object)b);
            if (typeof(T) == typeof(int))
                return (T)(object)((int)(object)a & (int)(object)b);
            if (typeof(T) == typeof(long))
                return (T)(object)((long)(object)a & (long)(object)b);
            throw new NotSupportedException($"Bitwise AND is not supported for type {typeof(T)}");
        }

        public static T BitwiseOr<T>(T a, T b)
        {
            if (typeof(T) == typeof(bool))
                return (T)(object)((bool)(object)a | (bool)(object)b);
            if (typeof(T) == typeof(int))
                return (T)(object)((int)(object)a | (int)(object)b);
            if (typeof(T) == typeof(long))
                return (T)(object)((long)(object)a | (long)(object)b);
            throw new NotSupportedException($"Bitwise OR is not supported for type {typeof(T)}");
        }

        public static T BitwiseXor<T>(T a, T b)
        {
            if (typeof(T) == typeof(bool))
                return (T)(object)((bool)(object)a ^ (bool)(object)b);
            if (typeof(T) == typeof(int))
                return (T)(object)((int)(object)a ^ (int)(object)b);
            if (typeof(T) == typeof(long))
                return (T)(object)((long)(object)a ^ (long)(object)b);
            throw new NotSupportedException($"Bitwise XOR is not supported for type {typeof(T)}");
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
        /// <summary>
        /// Stacks a list of tensors along the specified dimension.
        /// </summary>
        /// <param name="tensors">The list of tensors to stack.</param>
        /// <param name="dim">The dimension along which to stack the tensors.</param>
        /// <returns>A new tensor containing the stacked tensors.</returns>
        public static Tensor<T> Stack(IEnumerable<Tensor<T>> tensors, int dim)
        {
            if (tensors == null || !tensors.Any())
                throw new ArgumentException("The list of tensors cannot be null or empty.", nameof(tensors));

            var firstTensor = tensors.First();
            if (dim < 0 || dim > firstTensor.Shape.Length)
                throw new ArgumentOutOfRangeException(nameof(dim), "Dimension is out of range.");

            // Validate that all tensors have the same shape
            if (!tensors.All(t => t.Shape.SequenceEqual(firstTensor.Shape)))
                throw new ArgumentException("All tensors must have the same shape.");

            int[] newShape = new int[firstTensor.Shape.Length + 1];
            Array.Copy(firstTensor.Shape, 0, newShape, 0, dim);
            newShape[dim] = tensors.Count();
            Array.Copy(firstTensor.Shape, dim, newShape, dim + 1, firstTensor.Shape.Length - dim);

            Tensor<T> result = Empty(newShape);

            var tensorsList = tensors.ToList();
            for (int i = 0; i < result.Size(); i++)
            {
                var indices = result.GetApparentPosition(i);
                int tensorIndex = indices[dim];
                indices = indices.Take(dim).Concat(indices.Skip(dim + 1)).ToArray();
                result._data[i] = tensorsList[tensorIndex][indices].item;
            }

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
            if (tensors == null || tensors.Count == 0)
                throw new ArgumentException("The list of tensors cannot be null or empty.", nameof(tensors));

            if (dim < 0 || dim >= tensors[0].Shape.Length)
                throw new ArgumentOutOfRangeException(nameof(dim), "Dimension is out of range.");

            // Validate that all tensors have the same shape except for the concatenation dimension
            var firstShape = tensors[0].Shape;
            if (!tensors.All(t => t.Shape.Length == firstShape.Length &&
                                  t.Shape.Where((s, i) => i != dim).SequenceEqual(firstShape.Where((s, i) => i != dim))))
                throw new ArgumentException("All tensors must have the same shape except for the concatenation dimension.");

            int[] newShape = (int[])firstShape.Clone();
            newShape[dim] = tensors.Sum(t => t.Shape[dim]);

            Tensor<T> result = Empty(newShape);

            int offset = 0;
            for (int i = 0; i < tensors.Count; i++)
            {
                for (int j = 0; j < tensors[i].Size(); j++)
                {
                    var indices = tensors[i].GetApparentPosition(j);
                    indices[dim] += offset;
                    result[indices] = tensors[i]._data[j];
                }
                offset += tensors[i].Shape[dim];
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
            if (paddings == null || paddings.Count != Shape.Length)
                throw new ArgumentException("Paddings must be specified for each dimension.", nameof(paddings));

            int[] newShape = new int[Shape.Length];
            for (int i = 0; i < Shape.Length; i++)
            {
                newShape[i] = Shape[i] + paddings[i].Item1 + paddings[i].Item2;
            }

            Tensor<T> result = Empty(newShape);

            for (int i = 0; i < result.Size(); i++)
            {
                var indices = result.GetApparentPosition(i);
                bool isInOriginalTensor = true;
                int[] originalIndices = new int[indices.Length];

                for (int j = 0; j < indices.Length; j++)
                {
                    if (indices[j] < paddings[j].Item1 || indices[j] >= newShape[j] - paddings[j].Item2)
                    {
                        isInOriginalTensor = false;
                        break;
                    }
                    originalIndices[j] = indices[j] - paddings[j].Item1;
                }

                result._data[i] = isInOriginalTensor ? this[originalIndices].item : padMode.GetPaddedValue(this, indices);
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

        /// <summary>
        /// Computes the difference along the specified dimension.
        /// </summary>
        /// <param name="dim">The dimension along which to compute the difference.</param>
        /// <returns>A new tensor containing the differences.</returns>
        public Tensor<T> Diff(int dim = 0)
        {
            if (dim < 0 || dim >= Shape.Length)
                throw new ArgumentOutOfRangeException(nameof(dim), "Dimension is out of range.");

            int[] newShape = (int[])Shape.Clone();
            newShape[dim]--;

            Tensor<T> result = Empty(newShape);

            for (int i = 0; i < result.Size(); i++)
            {
                var indices = result.GetApparentPosition(i);
                var nextIndices = (int[])indices.Clone();
                nextIndices[dim]++;
                result._data[i] = NumericOperations.Subtract(this[nextIndices], this[indices]).item;
            }

            return result;
        }

        private Tensor<T> ReduceOperation(int[] dims, bool keepDims, Func<T, T, T> operation)
        {
            if (dims == null)
            {
                T scalarResult = _data.Aggregate(operation);
                return keepDims ? new Tensor<T>(new T[] { scalarResult }, Shape.Select(_ => 1).ToArray()) : FromArray(new T[] { scalarResult });
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

            for (int i = 0; i < Size(); i++)
            {
                var indices = GetApparentPosition(i);
                var resultIndices = GetReducedIndices(indices, dims, keepDims);
                int resultIndex = result.GetFlatIndex(resultIndices);

                result._data[resultIndex] = operation(result._data[resultIndex], this._data[i]);
            }

            return result;
        }

        private int[] GetReducedIndices(int[] indices, int[] dims, bool keepDims)
        {
            int[] resultIndices = new int[keepDims ? indices.Length : indices.Length - dims.Length];
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

            return resultIndices;
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
        public bool Is2D => Shape.Length == 2;

        public Tensor<T> Convolve<TKernel>(Tensor<TKernel> kernel, ConvolveMode convolveMode, (int rowDim, int colDim) dims) where TKernel : IComparable<TKernel>
        {
            if (typeof(T) != typeof(TKernel))
                return Convolve(kernel.Cast<T>(), convolveMode, dims);

            if (kernel.Shape.Length != 2)
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

            var paddingList = Enumerable.Repeat((0, 0), Shape.Length).ToList();
            paddingList[dims.rowDim] = (rowPad, rowPad);
            paddingList[dims.colDim] = (colPad, colPad);

            Tensor<T> padded = this.Pad(paddingList, new ConstantPadMode<T>(default));

            int[] newShape = Shape.ToArray();
            newShape[dims.rowDim] = padded.Shape[dims.rowDim] - kernel.Shape[0] + 1;
            newShape[dims.colDim] = padded.Shape[dims.colDim] - kernel.Shape[1] + 1;

            Tensor<T> result = Empty(newShape);

            for (int i = 0; i < result.Size(); i++)
            {
                int[] resultIndices = result.GetApparentPosition(i);
                int[] paddedIndices = (int[])resultIndices.Clone();

                T sum = NumericOperations.Zero<T>();
                for (int ki = 0; ki < kernel.Shape[0]; ki++)
                {
                    for (int kj = 0; kj < kernel.Shape[1]; kj++)
                    {
                        paddedIndices[dims.rowDim] = resultIndices[dims.rowDim] + ki;
                        paddedIndices[dims.colDim] = resultIndices[dims.colDim] + kj;
                        sum = NumericOperations.Add(sum, NumericOperations.Multiply(
                            padded[paddedIndices].item,
                            (T)(object)(kernel[ki, kj].item)
                        ));
                    }
                }
                result[resultIndices] = sum;
            }

            return result;
        }

        public Tensor<T> Convolve<TKernel>(Tensor<TKernel> kernel, ConvolveMode convolveMode = ConvolveMode.VALID) where TKernel : IComparable<TKernel>
            => Convolve(kernel, convolveMode, (0, 1));

        public Tensor<bool> GetPeak2D(Tensor<bool> mask, PeakMode mode, (int rowDim, int colDim) dims)
        {
            if (mask.Shape.Length != Shape.Length || !mask.Shape.SequenceEqual(Shape))
                throw new ArgumentException("Mask shape must match tensor shape.");

            Tensor<bool> result = Tensor<bool>.Empty(Shape);

            for (int i = 0; i < Size(); i++)
            {
                int[] indices = GetApparentPosition(i);

                if (!mask[indices].item) continue;

                bool isPeak = true;
                for (int di = -1; di <= 1 && isPeak; di++)
                {
                    for (int dj = -1; dj <= 1 && isPeak; dj++)
                    {
                        if (di == 0 && dj == 0) continue;

                        int[] neighborIndices = (int[])indices.Clone();
                        neighborIndices[dims.rowDim] += di;
                        neighborIndices[dims.colDim] += dj;

                        if (neighborIndices[dims.rowDim] < 0 || neighborIndices[dims.rowDim] >= Shape[dims.rowDim] ||
                            neighborIndices[dims.colDim] < 0 || neighborIndices[dims.colDim] >= Shape[dims.colDim])
                            continue;

                        int comparison = this[indices].item.CompareTo(this[neighborIndices].item);
                        if ((mode == PeakMode.Maximum && comparison <= 0) ||
                            (mode == PeakMode.Minimum && comparison >= 0))
                        {
                            isPeak = false;
                        }
                    }
                }

                result[indices] = isPeak;
            }

            return result;
        }

        public Tensor<bool> GetPeak2D(Tensor<bool> mask, PeakMode mode = PeakMode.Maximum) => GetPeak2D(mask, mode, (0, 1));

        public Tensor<T> Rotate90(int rotation, (int rowDim, int colDim) dims)
        {
            rotation = ((rotation % 4) + 4) % 4; // Normalize rotation to be between 0 and 3

            if (rotation == 0)
                return new Tensor<T>(this);

            int[] newShape = Shape.ToArray();
            if (rotation % 2 != 0)
            {
                newShape[dims.rowDim] = Shape[dims.colDim];
                newShape[dims.colDim] = Shape[dims.rowDim];
            }

            Tensor<T> result = Empty(newShape);

            for (int i = 0; i < Size(); i++)
            {
                int[] indices = GetApparentPosition(i);
                int[] newIndices = (int[])indices.Clone();

                switch (rotation)
                {
                    case 1: // 90 degrees
                        newIndices[dims.rowDim] = indices[dims.colDim];
                        newIndices[dims.colDim] = Shape[dims.rowDim] - 1 - indices[dims.rowDim];
                        break;
                    case 2: // 180 degrees
                        newIndices[dims.rowDim] = Shape[dims.rowDim] - 1 - indices[dims.rowDim];
                        newIndices[dims.colDim] = Shape[dims.colDim] - 1 - indices[dims.colDim];
                        break;
                    case 3: // 270 degrees
                        newIndices[dims.rowDim] = Shape[dims.colDim] - 1 - indices[dims.colDim];
                        newIndices[dims.colDim] = indices[dims.rowDim];
                        break;
                }

                result[newIndices] = this[indices];
            }

            return result;
        }

        public Tensor<T> Rotate90(int rotation = 1) => Rotate90(rotation, (0, 1));

        public Tensor<T> Roll(int rowOffset, int colOffset, (int rowDim, int colDim) dims)
        {
            Tensor<T> result = Empty();

            for (int i = 0; i < Size(); i++)
            {
                int[] indices = GetApparentPosition(i);
                int[] newIndices = (int[])indices.Clone();

                newIndices[dims.rowDim] = (indices[dims.rowDim] + rowOffset + Shape[dims.rowDim]) % Shape[dims.rowDim];
                newIndices[dims.colDim] = (indices[dims.colDim] + colOffset + Shape[dims.colDim]) % Shape[dims.colDim];

                result[newIndices] = this[indices];
            }

            return result;
        }

        public Tensor<T> Roll(int rowOffset, int colOffset) => Roll(rowOffset, colOffset, (0, 1));

        public Tensor<T> Slide(int rowOffset, int colOffset, PadMode<T> padMode, (int rowDim, int colDim) dims)
        {
            Tensor<T> result = Empty();

            for (int i = 0; i < Size(); i++)
            {
                int[] indices = GetApparentPosition(i);
                int[] srcIndices = (int[])indices.Clone();

                srcIndices[dims.rowDim] -= rowOffset;
                srcIndices[dims.colDim] -= colOffset;

                if (srcIndices[dims.rowDim] >= 0 && srcIndices[dims.rowDim] < Shape[dims.rowDim] &&
                    srcIndices[dims.colDim] >= 0 && srcIndices[dims.colDim] < Shape[dims.colDim])
                {
                    result[indices] = this[srcIndices];
                }
                else
                {
                    result[indices] = padMode.GetPaddedValue(this, srcIndices);
                }
            }

            return result;
        }

        public Tensor<T> Slide(int rowOffset, int colOffset, PadMode<T> padMode) => Slide(rowOffset, colOffset, padMode, (0, 1));
    }
}
