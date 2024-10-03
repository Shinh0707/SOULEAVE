using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SL.Lib
{
    [Serializable]
    public partial class Tensor<T> where T : IComparable<T>
    {
        [SerializeReference]
        private T[] _data;
        [SerializeField]
        private int[] _shape;
        [SerializeField]
        private int[] _strides;
        [SerializeField]
        private int _offset;
        [SerializeField]
        private bool _isView;
        [SerializeField]
        private bool _isReadOnly;
        public int[] Shape => _shape;
        public int[] Strides => _strides;
        public int Offset => _offset;
        public bool IsView => _isView;
        public bool IsReadOnly
        {
            get => _isReadOnly;
            set => _isReadOnly = value;
        }
        public IEnumerable<T> Unique => _data.Distinct();

        public T[] To1DArray() => (T[])_data.Clone();

        public int ndim => Shape.Length;
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
        private void CheckReadOnly()
        {
            if (IsReadOnly)
                throw new InvalidOperationException("Tensor is read-only.");
        }
        /// <summary>
        /// Gets the single item of the tensor if it's a scalar.
        /// Outputs a warning if the tensor is not a scalar.
        /// </summary>
        public T item
        {
            get
            {
                if (!IsScalar)
                {
                    Debug.LogWarning("Accessing 'item' on a non-scalar tensor. This may lead to unexpected behavior.");
                }
                return _data[Offset];
            }
        }
        public bool IsScalar => _data.Length == 1;
        public int Size()
        {
            int size = 1;
            for (int i = 0; i < ndim; i++)
            {
                size *= Shape[i];
            }
            return size;
        }

        public int Size(int dim) => Shape[dim];
        public bool MatchShape(int[] shape) => Shape.SequenceEqual(shape);
        public bool MatchShape<TOther>(Tensor<TOther> tensor) where TOther : IComparable<TOther> => Shape.SequenceEqual(tensor.Shape);

        public Tensor(T[] data, int[] shape, bool isReadOnly = false)
        {
            _data = data;
            _shape = shape;
            _strides = ComputeStrides(shape);
            _offset = 0;
            _isView = false;
            IsReadOnly = isReadOnly;
        }

        private Tensor(T[] data, int[] shape, int[] strides, int offset, bool isView, bool isReadOnly)
        {
            _data = data;
            _shape = shape;
            _strides = strides;
            _offset = offset;
            _isView = isView;
            IsReadOnly = isReadOnly;
        }

        public Tensor(Tensor<T> tensor, bool isReadOnly)
        {
            _shape = (int[])tensor.Shape.Clone();
            _data = (T[])tensor._data.Clone();
            IsReadOnly = isReadOnly;
            _strides = (int[])tensor.Strides.Clone();
            _offset = tensor.Offset;
            _isView = false;
        }

        public Tensor(Tensor<T> tensor) : this(tensor, tensor.IsReadOnly)
        {
        }

        public T this[params int[] indices]
        {
            get
            {
                return _data[GetFlatIndex(indices)];
            }
            set
            {
                _data[GetFlatIndex(indices)] = value;
            }
        }

        public Tensor<T> this[List<Indice[]> indices]
        {
            get
            {
                return Stack(indices.Select(index => this[index]), 0);
            }
            set
            {
                CheckReadOnly();
                if (value.IsScalar)
                {
                    foreach (var indice in indices)
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
        public Tensor<T> this[List<int[]> indices]
        {
            get
            {
                return FromArray(indices.Select(index => this[index]).ToArray());
            }
            set
            {
                CheckReadOnly();
                if (value.IsScalar)
                {
                    foreach (var indice in indices)
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

                return FromArray(mask._data.Select((v,i) => (v,i)).Where(d => d.v).Select(d => _data[d.i]).ToArray());
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
                        selectedIndices.Add(mask.GetIndices(i));
                }

                this[selectedIndices] = value;
            }
        }

        public Tensor<T> this[params Indice[] indices]
        {
            get
            {
                List<int[]> allIndices = new List<int[]>();
                int[] newShape = new int[ndim];
                int totalSize = 1;

                for (int i = 0; i < ndim; i++)
                {
                    if (i < indices.Length && indices[i] != null)
                    {
                        int[] dimIndices = indices[i].GetIndices(Shape[i]);
                        allIndices.Add(dimIndices);
                        newShape[i] = dimIndices.Length;
                    }
                    else
                    {
                        allIndices.Add(Enumerable.Range(0, Shape[i]).ToArray());
                        newShape[i] = Shape[i];
                    }
                    totalSize *= newShape[i];
                }

                // 最適化: 単一要素の場合
                if (totalSize == 1)
                {
                    int[] singleIndex = allIndices.Select(arr => arr[0]).ToArray();
                    return new Tensor<T>(new T[] { _data[GetFlatIndex(singleIndex)] }, new int[] { 1 });
                }

                T[] newData = new T[totalSize];
                int[] currentIndices = new int[ndim];

                CopyData(allIndices, newData, currentIndices, 0, 0);

                return new Tensor<T>(newData, newShape);
            }
            set
            {
                List<int[]> allIndices = new List<int[]>();
                int[] assignShape = new int[ndim];
                int totalSize = 1;

                for (int i = 0; i < ndim; i++)
                {
                    if (i < indices.Length && indices[i] != null)
                    {
                        int[] dimIndices = indices[i].GetIndices(Shape[i]);
                        allIndices.Add(dimIndices);
                        assignShape[i] = dimIndices.Length;
                    }
                    else
                    {
                        allIndices.Add(Enumerable.Range(0, Shape[i]).ToArray());
                        assignShape[i] = Shape[i];
                    }
                    totalSize *= assignShape[i];
                }

                // 特別処理: IsScalar が true のテンソルが set される場合
                if (value.IsScalar)
                {
                    T scalarValue = value._data[0];
                    SetScalarData(allIndices, scalarValue, new int[ndim], 0);
                    return;
                }

                if (!assignShape.SequenceEqual(value.Shape))
                {
                    throw new ArgumentException("Assigned value shape does not match the indexed shape.");
                }

                // 最適化: 単一要素の割り当ての場合
                if (totalSize == 1)
                {
                    int[] singleIndex = allIndices.Select(arr => arr[0]).ToArray();
                    _data[GetFlatIndex(singleIndex)] = value._data[0];
                    return;
                }

                int[] currentIndices = new int[ndim];
                SetData(allIndices, value._data, currentIndices, 0, 0);
            }
        }

        private void CopyData(List<int[]> allIndices, T[] newData, int[] currentIndices, int dimension, int newDataIndex)
        {
            if (dimension == ndim)
            {
                int flatIndex = GetFlatIndex(currentIndices);
                newData[newDataIndex] = _data[flatIndex];
                return;
            }

            for (int i = 0; i < allIndices[dimension].Length; i++)
            {
                currentIndices[dimension] = allIndices[dimension][i];
                CopyData(allIndices, newData, currentIndices, dimension + 1, newDataIndex);
                newDataIndex += allIndices.Skip(dimension + 1).Aggregate(1, (a, arr) => a * arr.Length);
            }
        }

        private void SetData(List<int[]> allIndices, T[] valueData, int[] currentIndices, int dimension, int valueDataIndex)
        {
            if (dimension == ndim)
            {
                int flatIndex = GetFlatIndex(currentIndices);
                _data[flatIndex] = valueData[valueDataIndex];
                return;
            }

            for (int i = 0; i < allIndices[dimension].Length; i++)
            {
                currentIndices[dimension] = allIndices[dimension][i];
                SetData(allIndices, valueData, currentIndices, dimension + 1, valueDataIndex);
                valueDataIndex += allIndices.Skip(dimension + 1).Aggregate(1, (a, arr) => a * arr.Length);
            }
        }

        private void SetScalarData(List<int[]> allIndices, T scalarValue, int[] currentIndices, int dimension)
        {
            if (dimension == ndim)
            {
                int flatIndex = GetFlatIndex(currentIndices);
                _data[flatIndex] = scalarValue;
                return;
            }

            for (int i = 0; i < allIndices[dimension].Length; i++)
            {
                currentIndices[dimension] = allIndices[dimension][i];
                SetScalarData(allIndices, scalarValue, currentIndices, dimension + 1);
            }
        }

        public int GetFlatIndex(int[] indices)
        {
            int flatIndex = 0;
            for (int i = 0; i < indices.Length; i++)
            {
                flatIndex += indices[i] * Strides[i];
            }
            return flatIndex;
        }

        public Indice[] GetIndices(int flatIndex)
        {
            Indice[] indices = new Indice[ndim];
            for (int i = 0; i < ndim; i++)
            {
                indices[i] = flatIndex / Strides[i];
                flatIndex %= Strides[i];
            }
            return indices;
        }
        public void GetIndices(int flatIndex, int[] indices)
        {
            if (flatIndex < 0 || flatIndex >= Size())
            {
                throw new ArgumentOutOfRangeException(nameof(flatIndex), "Flat index is out of range.");
            }

            for (int i = 0; i < ndim; i++)
            {
                indices[i] = flatIndex / Strides[i];
                flatIndex %= Strides[i];
            }
        }
        public int[][] GetRealIndices(Indice[] indices)
        {
            int[][] realIndices = new int[ndim][];
            for(int dim = 0; dim < ndim; dim++)
            {
                realIndices[dim] = indices[dim].GetIndices(Size(dim));
            }
            return realIndices;
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
                return this.MatchShape(other.Shape) &&
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
            StringBuilder sb = new StringBuilder();

            // テンソルの基本情報を表示
            sb.AppendLine($"Tensor<{typeof(T).Name}>");
            sb.AppendLine($"Shape: [{string.Join(", ", Shape)}]");
            sb.AppendLine($"Strides: [{string.Join(", ", Strides)}]");
            sb.AppendLine($"Data:");

            // データを再帰的に表示
            AppendTensorData(sb, new int[ndim], 0);

            return sb.ToString();
        }

        private void AppendTensorData(StringBuilder sb, int[] currentIndices, int depth)
        {
            if (depth == ndim)
            {
                // 最下層に到達したら、値を追加
                int flatIndex = 0;
                for (int i = 0; i < ndim; i++)
                {
                    flatIndex += currentIndices[i] * Strides[i];
                }
                sb.Append(_data[flatIndex]);
                return;
            }

            sb.Append(new string(' ', depth * 2) + "[");

            for (int i = 0; i < Shape[depth]; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                    if (depth == ndim - 2)
                    {
                        sb.AppendLine();
                        sb.Append(new string(' ', depth * 2 + 1));
                    }
                }

                currentIndices[depth] = i;
                AppendTensorData(sb, currentIndices, depth + 1);
            }

            sb.Append("]");
            if (depth < ndim - 1)
            {
                sb.AppendLine();
            }
        }
    }
}
