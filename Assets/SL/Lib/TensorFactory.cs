using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SL.Lib
{
    /*
     * Factory Methods
     */
    public partial class Tensor<T> where T : IComparable<T>
    {
        /// <summary>
        /// Creates an empty Tensor with the specified shape.
        /// </summary>
        /// <param name="shape">The shape of the Tensor.</param>
        /// <returns>A new empty Tensor with the specified shape.</returns>
        public static Tensor<T> Empty(params int[] shape)
        {
            if (shape == null || shape.Length == 0)
                throw new ArgumentException("Shape must be non-null and non-empty.", nameof(shape));

            int size = shape.Aggregate(1, (a, b) => a * b);
            return new Tensor<T>(new T[size], shape);
        }
        /// <summary>
        /// Creates an empty Tensor.
        /// </summary>
        /// <returns>A new empty Tensor with the same shape.</returns>
        public Tensor<T> Empty()
        {
            int size = Shape.Aggregate(1, (a, b) => a * b);
            return new Tensor<T>(new T[size], Shape, IsReadOnly);
        }

        /// <summary>
        /// Creates a Tensor from an Array.
        /// </summary>
        /// <param name="array">The source array.</param>
        /// <param name="isReadOnly">Indicates whether the resulting Tensor should be read-only.</param>
        /// <returns>A new Tensor created from the given array.</returns>
        public static Tensor<T> FromArray(Array array, bool isReadOnly = false)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            int[] shape = Enumerable.Range(0, array.Rank)
                                    .Select(i => array.GetLength(i))
                                    .ToArray();

            T[] flattenedData = new T[array.Length];
            int index = 0;
            foreach (T item in array)
            {
                flattenedData[index++] = item;
            }

            return new Tensor<T>(flattenedData, shape, isReadOnly);
        }

        /// <summary>
        /// Creates a Tensor filled with zeros.
        /// </summary>
        /// <param name="shape">The shape of the Tensor.</param>
        /// <returns>A new Tensor filled with zeros.</returns>
        public static Tensor<T> Zeros(params int[] shape)
        {
            if (shape == null || shape.Length == 0)
                throw new ArgumentException("Shape must be non-null and non-empty.", nameof(shape));

            int size = shape.Aggregate(1, (a, b) => a * b);
            T[] data = new T[size];
            Array.Fill(data, NumericOperations.Zero<T>());

            return new Tensor<T>(data, shape);
        }

        /// <summary>
        /// Creates a Tensor filled with zeros, matching the shape of another Tensor.
        /// </summary>
        /// <typeparam name="TSource">The type of the source Tensor.</typeparam>
        /// <param name="tensor">The source Tensor whose shape to match.</param>
        /// <returns>A new Tensor filled with zeros, matching the shape of the source Tensor.</returns>
        public static Tensor<T> Zeros<TSource>(Tensor<TSource> tensor) where TSource : IComparable<TSource>
        {
            return Zeros(tensor.Shape);
        }

        /// <summary>
        /// Creates a Tensor filled with ones.
        /// </summary>
        /// <param name="shape">The shape of the Tensor.</param>
        /// <returns>A new Tensor filled with ones.</returns>
        public static Tensor<T> Ones(params int[] shape)
        {
            if (shape == null || shape.Length == 0)
                throw new ArgumentException("Shape must be non-null and non-empty.", nameof(shape));

            int size = shape.Aggregate(1, (a, b) => a * b);
            T[] data = new T[size];
            Array.Fill(data, NumericOperations.One<T>());

            return new Tensor<T>(data, shape);
        }

        /// <summary>
        /// Creates a Tensor filled with ones, matching the shape of another Tensor.
        /// </summary>
        /// <typeparam name="TSource">The type of the source Tensor.</typeparam>
        /// <param name="tensor">The source Tensor whose shape to match.</param>
        /// <returns>A new Tensor filled with ones, matching the shape of the source Tensor.</returns>
        public static Tensor<T> Ones<TSource>(Tensor<TSource> tensor) where TSource : IComparable<TSource>
        {
            return Ones(tensor.Shape);
        }

        /// <summary>
        /// Creates a Tensor filled with random values.
        /// </summary>
        /// <param name="shape">The shape of the Tensor.</param>
        /// <returns>A new Tensor filled with random values.</returns>
        public static Tensor<T> Randoms(int[] shape, float rate)
        {
            if (shape == null || shape.Length == 0)
                throw new ArgumentException("Shape must be non-null and non-empty.", nameof(shape));

            int size = shape.Aggregate(1, (a, b) => a * b);
            T[] data = new T[size];

            for (int i = 0; i < size; i++)
            {
                data[i] = NumericOperations.Random<T>(rate);
            }

            return new Tensor<T>(data, shape);
        }

        /// <summary>
        /// Creates a Tensor filled with random values, matching the shape of another Tensor.
        /// </summary>
        /// <typeparam name="TSource">The type of the source Tensor.</typeparam>
        /// <param name="tensor">The source Tensor whose shape to match.</param>
        /// <returns>A new Tensor filled with random values, matching the shape of the source Tensor.</returns>
        public static Tensor<T> Randoms<TSource>(Tensor<TSource> tensor, float rate) where TSource : IComparable<TSource>
        {
            return Randoms(tensor.Shape, rate);
        }

        /// <summary>
        /// Creates a Tensor filled with a specific value.
        /// </summary>
        /// <param name="value">The value to fill the Tensor with.</param>
        /// <param name="shape">The shape of the Tensor.</param>
        /// <returns>A new Tensor filled with the specified value.</returns>
        public static Tensor<T> Full(T value, params int[] shape)
        {
            if (shape == null || shape.Length == 0)
                throw new ArgumentException("Shape must be non-null and non-empty.", nameof(shape));

            int size = shape.Aggregate(1, (a, b) => a * b);
            T[] data = new T[size];
            Array.Fill(data, value);

            return new Tensor<T>(data, shape);
        }
        /// <summary>
        /// Creates a Tensor filled with a specific value.
        /// </summary>
        /// <param name="value">The value to fill the Tensor with.</param>
        /// <param name="tensor">The shape of the Tensor.</param>
        /// <returns>A new Tensor filled with the specified value.</returns>
        public static Tensor<T> Full<TSource>(T value, Tensor<TSource> tensor) where TSource : IComparable<TSource> => Full(value, tensor.Shape);

        // <summary>
        /// Broadcasts two tensors to a compatible shape.
        /// </summary>
        /// <param name="A">The first tensor to broadcast.</param>
        /// <param name="B">The second tensor to broadcast.</param>
        /// <returns>A tuple containing the two broadcast tensors.</returns>
        public static (Tensor<T>, Tensor<T>) Broadcast(Tensor<T> A, Tensor<T> B)
        {
            int[] broadcastShape = BroadcastShapes(A.Shape, B.Shape);
            Tensor<T> broadcastA = BroadcastToShape(A, broadcastShape);
            Tensor<T> broadcastB = BroadcastToShape(B, broadcastShape);
            return (broadcastA, broadcastB);
        }

        /// <summary>
        /// Broadcasts two tensors and applies a specified operation element-wise.
        /// </summary>
        /// <typeparam name="TResult">The type of the result tensor.</typeparam>
        /// <param name="A">The first tensor to broadcast.</param>
        /// <param name="B">The second tensor to broadcast.</param>
        /// <param name="valueOperation">The operation to apply element-wise.</param>
        /// <returns>A new tensor containing the result of the operation.</returns>
        public static Tensor<TResult> Broadcast<TResult>(Tensor<T> A, Tensor<T> B, Func<T, T, TResult> valueOperation)
            where TResult : IComparable<TResult>
        {
            int[] broadcastShape = BroadcastShapes(A.Shape, B.Shape);
            Tensor<TResult> result = Tensor<TResult>.Empty(broadcastShape);
            int[] indices = new int[broadcastShape.Length];
            for (int i = 0; i < result.Size(); i++)
            {
                result.GetIndices(i,indices);
                T valueA = A.GetBroadcastValue(indices, broadcastShape);
                T valueB = B.GetBroadcastValue(indices, broadcastShape);
                //Debug.Log($"Progress: {valueOperation.Method.Name}(A[{valueA}],B[{valueB}])");
                result[indices] = valueOperation(valueA, valueB);
            }

            return result;
        }

        private static Tensor<T> BroadcastToShape(Tensor<T> tensor, int[] targetShape)
        {
            Tensor<T> result = Empty(targetShape);
            int[] indices = new int[targetShape.Length];
            for (int i = 0; i < result.Size(); i++)
            {
                result.GetIndices(i,indices);
                result._data[result.GetFlatIndex(indices)] = tensor.GetBroadcastValue(indices, targetShape);
            }

            return result;
        }
        public static int[] BroadcastShapes(int[] shapeA, int[] shapeB)
        {
            if (shapeA == null || shapeB == null)
            {
                throw new ArgumentNullException(shapeA == null ? nameof(shapeA) : nameof(shapeB), "Shape arrays cannot be null.");
            }

            int rank = Math.Max(shapeA.Length, shapeB.Length);
            int[] result = new int[rank];

            for (int i = 1; i <= rank; i++)
            {
                int dimA = i <= shapeA.Length ? shapeA[^i] : 1;
                int dimB = i <= shapeB.Length ? shapeB[^i] : 1;

                if (dimA < 0 || dimB < 0)
                {
                    throw new ArgumentException($"Invalid dimension size. All dimensions must be non-negative. Found: dimA = {dimA}, dimB = {dimB} at position {rank - i}");
                }

                if (dimA == dimB)
                {
                    result[^i] = dimA;
                }
                else if (dimA == 1)
                {
                    result[^i] = dimB;
                }
                else if (dimB == 1)
                {
                    result[^i] = dimA;
                }
                else
                {
                    throw new ArgumentException(
                        $"Shapes are not compatible for broadcasting at dimension {rank - i}. " +
                        $"Shape A: {FormatPartialShape(shapeA, i)}, " +
                        $"Shape B: {FormatPartialShape(shapeB, i)}, " +
                        $"Conflict: {dimA} vs {dimB}");
                }
            }

            return result;
        }

        private static string FormatPartialShape(int[] shape, int lastN)
        {
            var relevantPart = shape.Length >= lastN ? shape.Skip(shape.Length - lastN) : shape;
            return $"[{string.Join(", ", relevantPart)}]" + (shape.Length < lastN ? $" + {lastN - shape.Length} leading 1's" : "");
        }
        /// <summary>
        /// Gets the broadcast value for the given indices and target shape.
        /// </summary>
        /// <param name="indices">The indices in the target shape.</param>
        /// <param name="targetShape">The shape to broadcast to.</param>
        /// <returns>The value at the broadcast position.</returns>
        public T GetBroadcastValue(int[] indices, int[] targetShape)
        {
            if (indices.Length != targetShape.Length)
            {
                throw new ArgumentException("Number of indices must match the number of dimensions in the target shape.");
            }

            int[] adjustedIndices = new int[Shape.Length];
            int targetDim = 0;

            for (int thisDim = 0; thisDim < Shape.Length; thisDim++)
            {
                if (targetDim >= targetShape.Length)
                {
                    throw new ArgumentException("Target shape is not compatible for broadcasting.");
                }

                if (Shape[thisDim] == targetShape[targetDim])
                {
                    adjustedIndices[thisDim] = indices[targetDim] % Shape[thisDim];
                }
                else if (Shape[thisDim] == 1)
                {
                    adjustedIndices[thisDim] = 0;
                }
                else
                {
                    throw new ArgumentException($"Incompatible shapes for broadcasting: {string.Join(",", Shape)} and {string.Join(",", targetShape)}");
                }

                targetDim++;
            }

            return _data[GetFlatIndex(adjustedIndices)];
        }
    }
}
