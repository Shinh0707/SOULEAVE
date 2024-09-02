using System;
using System.Collections.Generic;
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

    }
}
