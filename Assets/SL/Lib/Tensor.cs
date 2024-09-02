using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SL.Lib
{
    public partial class Tensor<T> where T : IComparable<T>
    {
        private T[] _data;

        public IEnumerable<T> Unique => _data.Distinct();

        public T[] To1DArray() => (T[])_data.Clone();
        public int[] Shape { get; }
        public int[] Strides { get; }
        private static int[] ComputeStrides(int[] shape)
        {
            var strides = new int[shape.Length];
            int stride = 1;
            for (int i = shape.Length - 1; i >= 0; i--)
            {
                strides[i] = stride;
                stride *= shape[i];
            }
            return strides;
        }
        public int Offset { get; }
        public bool IsView { get; }
        public bool IsReadOnly { get; set; }
        private void CheckReadOnly()
        {
            if (IsReadOnly)
                throw new InvalidOperationException("Tensor is read-only.");
        }
        public T item => _data[Offset];
        public bool IsScalar => _data.Length == 1;
        public int Size() => _data.Length;

        public int Size(int dim) => Shape[dim];
        public bool MatchShape(int[] shape) => Shape.SequenceEqual(shape);
        public bool MatchShape<TOther>(Tensor<TOther> tensor) where TOther : IComparable<TOther> => Shape.SequenceEqual(tensor.Shape);

        public Tensor(T[] data, int[] shape, bool isReadOnly = false)
        {
            _data = data;
            Shape = shape;
            Strides = ComputeStrides(shape);
            Offset = 0;
            IsView = false;
            IsReadOnly = isReadOnly;
        }

        private Tensor(T[] data, int[] shape, int[] strides, int offset, bool isView, bool isReadOnly)
        {
            _data = data;
            Shape = shape;
            Strides = strides;
            Offset = offset;
            IsView = isView;
            IsReadOnly = isReadOnly;
        }

        public Tensor(Tensor<T> tensor, bool isReadOnly)
        {
            Shape = (int[])tensor.Shape.Clone();
            _data = (T[])tensor._data.Clone();
            IsReadOnly = isReadOnly;
            Strides = (int[])tensor.Strides.Clone();
            Offset = tensor.Offset;
            IsView = false;
        }

        public Tensor(Tensor<T> tensor) : this(tensor, tensor.IsReadOnly)
        {
        }

        public Tensor<T> this[List<Indice[]> indices]
        {
            get
            {
                return Stack(indices.Select(index => this[index]),0);
            }
            set
            {
                CheckReadOnly();
                if (value[0].IsScalar)
                {
                    foreach(var indice in indices)
                    {
                        this[indice] = value.item;
                    }
                }
                else
                {
                    if (value.Size() != indices.Count)
                        throw new ArgumentException("The size of the value tensor must match the number of indices.");

                    for (int i = 0; i < indices.Count; i++)
                    {
                        this[indices[i]] = value[value.GetIndices(i)].item;
                    }
                }
            }
        }

        public Tensor<T> this[Tensor<bool> mask]
        {
            get
            {
                if (!MatchShape(mask.Shape))
                    throw new ArgumentException("Mask shape must match tensor shape.");

                List<Indice[]> selectedIndices = new List<Indice[]>();
                for (int i = 0; i < _data.Length; i++)
                {
                    if (mask._data[i])
                        selectedIndices.Add(GetIndices(i));
                }

                return this[selectedIndices];
            }
            set
            {
                CheckReadOnly();
                if (!MatchShape(mask.Shape))
                    throw new ArgumentException("Mask shape must match tensor shape.");

                List<Indice[]> selectedIndices = new List<Indice[]>();
                for (int i = 0; i < _data.Length; i++)
                {
                    if (mask._data[i])
                        selectedIndices.Add(GetIndices(i));
                }


                this[selectedIndices] = value;
            }
        }

        /// <summary>
        /// Indexer that supports advanced indexing operations using Indice objects.
        /// </summary>
        /// <param name="indices">The indices to access, represented as Indice objects.</param>
        /// <returns>A tensor or a single element, depending on the indexing.</returns>
        public Tensor<T> this[params Indice[] indices]
        {
            get
            {
                if (indices == null || indices.Length == 0)
                    throw new ArgumentException("Indices cannot be null or empty.");

                if (indices.Length > Shape.Length)
                    throw new ArgumentException("Too many indices for tensor shape.");

                List<int[]> allIndices = new List<int[]>();
                List<int> newShape = new List<int>();

                for (int i = 0; i < indices.Length; i++)
                {
                    int[] dimIndices = indices[i].GetIndices(Shape[i]);
                    allIndices.Add(dimIndices);
                    if (indices[i].Type != Indice.IndiceType.Single)
                    {
                        newShape.Add(dimIndices.Length);
                    }
                }

                // If we're selecting a single element, return it directly
                if (allIndices.All(arr => arr.Length == 1) && allIndices.Count == Shape.Length)
                {
                    return this[allIndices.Select(arr => arr[0]).ToArray()];
                }

                // Otherwise, create a new tensor with the selected elements
                for (int i = indices.Length; i < Shape.Length; i++)
                {
                    allIndices.Add(Enumerable.Range(0, Shape[i]).ToArray());
                    newShape.Add(Shape[i]);
                }

                T[] newData = new T[newShape.Aggregate(1, (a, b) => a * b)];
                int[] currentIndices = new int[Shape.Length];

                GetSubTensor(allIndices, newData, currentIndices, 0, 0);

                return new Tensor<T>(newData, newShape.ToArray());
            }
            set
            {
                throw new NotImplementedException("Setting values using advanced indexing is not yet implemented.");
            }
        }

        private void GetSubTensor(List<int[]> allIndices, T[] newData, int[] currentIndices, int depth, int newDataIndex)
        {
            if (depth == allIndices.Count)
            {
                newData[newDataIndex] = this[currentIndices].item;
                return;
            }

            foreach (int index in allIndices[depth])
            {
                currentIndices[depth] = index;
                GetSubTensor(allIndices, newData, currentIndices, depth + 1, newDataIndex);
                newDataIndex += allIndices.Skip(depth + 1).Aggregate(1, (a, arr) => a * arr.Length);
            }
        }
        private void CopyData(Tensor<T> src, Tensor<T> dst, int[] shape, int[] dstOffset, int srcDim = 0, int dstDim = 0, int srcIndex = 0, int dstIndex = 0)
        {
            if (srcDim == shape.Length)
            {
                dst._data[dst.Offset + dstIndex] = src._data[src.Offset + srcIndex];
                return;
            }

            for (int i = 0; i < shape[srcDim]; i++)
            {
                CopyData(src, dst, shape, dstOffset,
                         srcDim + 1, dstDim + 1,
                         srcIndex + i * src.Strides[srcDim],
                         dstIndex + (i + dstOffset[dstDim]) * dst.Strides[dstDim]);
            }
        }

        public Tensor<T> Reshape(params int[] newShape)
        {
            if (newShape.Aggregate(1, (a, b) => a * b) != Shape.Aggregate(1, (a, b) => a * b))
                throw new ArgumentException("New shape must have the same number of elements as the current shape.");

            if (IsView)
            {
                // ビューの場合は新しいテンソルを作成
                var newData = new T[newShape.Aggregate(1, (a, b) => a * b)];
                CopyData(this, new Tensor<T>(newData, newShape), Shape, new int[Shape.Length]);
                return new Tensor<T>(newData, newShape);
            }
            else
            {
                // ビューでない場合は同じデータを共有
                return new Tensor<T>(_data, newShape, ComputeStrides(newShape), 0, false, IsReadOnly);
            }
        }

        public Indice[] GetIndices(int flatIndex)
        {
            Indice[] indices = new Indice[Shape.Length];
            for (int i = Shape.Length - 1; i >= 0; i--)
            {
                indices[i] = flatIndex % Shape[i];
                flatIndex /= Shape[i];
            }
            return indices;
        }

        // ScalarからTensorへの暗黙的な変換
        public static implicit operator Tensor<T>(T scalar)
        {
            return new Tensor<T>(new T[] { scalar }, new[] { 1 });
        }
        // Override Equals and GetHashCode
        public override bool Equals(object obj)
        {
            if (obj is Tensor<T> other)
            {
                return this.Shape.SequenceEqual(other.Shape) &&
                       Enumerable.Range(0, this.Size()).All(i => this[GetIndices(i)].Equals(other[other.GetIndices(i)]));
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                for (int i = 0; i < Size(); i++)
                {
                    hash = hash * 31 + this[GetIndices(i)].GetHashCode();
                }
                foreach (var dim in Shape)
                {
                    hash = hash * 31 + dim.GetHashCode();
                }
                return hash;
            }
        }

        public override string ToString()
        {
            return $"Tensor<{typeof(T).Name}>{(IsView ? " (View)" : "")} Shape: [{string.Join(", ", Shape)}], Strides: [{string.Join(", ", Strides)}], Offset: {Offset}";
        }
    }
}
