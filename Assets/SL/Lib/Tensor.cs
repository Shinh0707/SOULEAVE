using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;

namespace SL.Lib
{
    public interface ITensor<T> where T : IComparable<T>
    {
        int[] Shape { get; }
        T Max { get; }
        T Min { get; }
        bool IsReadOnly { get; }
        int Size(int dimension);
    }

    public abstract class TensorBase<T> : ITensor<T> where T : IComparable<T>
    {
        protected T[] _data;
        protected int[] _shape;
        protected T _max;
        protected T _min;
        protected bool _isReadOnly;

        public int[] Shape => (int[])_shape.Clone();

        public T[] Data => (T[])_data.Clone();
        public T Max => _max;
        public T Min => _min;
        public bool IsReadOnly => _isReadOnly;

        public bool IsScalar => _data.Length == 1 && _shape.Length == 1;

        public T this[int directIndice]
        {
            get
            {
                return _data[directIndice];
            }
            set
            {
                if (_isReadOnly) return;
                _data[directIndice] = value;
                UpdateMinMax(value);
            }
        }
        public int Size(int dimension)
        {
            if (dimension < 0 || dimension >= _shape.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(dimension), $"Invalid dimension. Must be between 0 and {_shape.Length - 1}");
            }
            return _shape[dimension];
        }
        public int Size() => _shape.Length;
        protected void UpdateMinMax(T value)
        {
            if (EqualityComparer<T>.Default.Equals(_min, default) || value.CompareTo(_min) < 0)
            {
                _min = value;
            }
            if (EqualityComparer<T>.Default.Equals(_max, default) || value.CompareTo(_max) > 0)
            {
                _max = value;
            }
        }
        protected void UpdateMinMax()
        {
            if (_data.Length > 0)
            {
                _min = _data[0];
                _max = _data[0];
                foreach (var item in _data)
                {
                    UpdateMinMax(item);
                }
            }
        }

        public void MakeReadOnly()
        {
            _isReadOnly = true;
        }

        protected void CheckReadOnly()
        {
            if (_isReadOnly)
            {
                throw new InvalidOperationException("Cannot modify a read-only tensor.");
            }
        }

        public bool MatchShape(int[] shape) => Shape.SequenceEqual(shape);

        public bool MatchShape<TOther>(TensorBase<TOther> tensor) where TOther : IComparable<TOther> => Shape.SequenceEqual(tensor.Shape);

        protected int CalculateIndex(params int[] indices)
        {
            if (indices.Length != _shape.Length)
            {
                throw new ArgumentException("Number of indices must match the number of dimensions.");
            }

            int index = 0;
            int multiplier = 1;

            for (int i = indices.Length - 1; i >= 0; i--)
            {
                if (indices[i] < 0 || indices[i] >= _shape[i])
                {
                    throw new IndexOutOfRangeException($"Index {indices[i]} is out of range for dimension {i}");
                }

                index += indices[i] * multiplier;
                multiplier *= _shape[i];
            }

            return index;
        }
    }

    public partial class Tensor<T> : TensorBase<T> where T : IComparable<T>
    {
        public Tensor(params int[] shape)
        {
            _shape = (int[])shape.Clone();
            int size = shape.Aggregate(1, (a, b) => a * b);
            _data = new T[size];
        }
        public Tensor(Tensor<T> tensor, bool isReadOnly)
        {
            _shape = (int[])tensor.Shape.Clone();
            _data = (T[])tensor._data.Clone();
            _max = tensor._max;
            _min = tensor._min;
            _isReadOnly = isReadOnly;
        }
        public Tensor(Tensor<T> tensor) : this(tensor,tensor.IsReadOnly)
        {
        }
        public Tensor(Array data, params int[] shape) : this(shape)
        {
            if (data.Length != _data.Length)
            {
                throw new ArgumentException("Data length does not match the specified shape.");
            }

            Array.Copy(data, _data, data.Length);
            UpdateMinMax();
        }

        // Static factory method for creating a Tensor from an array
        public static Tensor<T> FromArray(Array array)
        {
            return new Tensor<T>(array, GetArrayShape(array));
        }

        private static int[] GetArrayShape(Array array)
        {
            int[] shape = new int[array.Rank];
            for (int i = 0; i < array.Rank; i++)
            {
                shape[i] = array.GetLength(i);
            }
            return shape;
        }

        public dynamic AsReadOnly() {
            MakeReadOnly();
            return this; 
        }

        public T this[params int[] indices]
        {
            get
            {
                int index = CalculateIndex(indices);
                return _data[index];
            }
            set
            {
                CheckReadOnly();
                int index = CalculateIndex(indices);
                _data[index] = value;
                UpdateMinMax(value);
            }
        }
        public Tensor<T> this[List<int[]> indices]
        {
            get
            {
                return new(indices.Select(indice => this[CalculateIndex(indice)]).ToArray(), indices.Count);
            }
            set
            {
                if (_isReadOnly) return;
                if (value.IsScalar)
                {
                    var v = value[0];
                    foreach (var indice in indices)
                    {
                        _data[CalculateIndex(indice)] = v;
                    }
                    UpdateMinMax(v);
                }
                else
                {
                    foreach (var pair in indices.Zip(value._data, (ind, d) => (CalculateIndex(ind), d)))
                    {
                        _data[pair.Item1] = pair.d;
                    }
                    UpdateMinMax();
                }
            }
        }

        // List<int>を受け取るインデクサー
        public Tensor<T> this[List<int> directIndices]
        {
            get
            {
                var indicesList = directIndices.ToList();
                return new Tensor<T>(indicesList.Select(index => _data[index]).ToArray(), indicesList.Count);
            }
            set
            {
                CheckReadOnly();
                var indicesList = directIndices.ToList();
                if (value.IsScalar)
                {
                    for (int i = 0; i < indicesList.Count; i++)
                    {
                        _data[indicesList[i]] = value[0];
                    }
                    UpdateMinMax(value[0]);
                }
                else
                {
                    if (value.Size() != indicesList.Count)
                        throw new ArgumentException("The size of the value tensor must match the number of indices.");

                    for (int i = 0; i < indicesList.Count; i++)
                    {
                        _data[indicesList[i]] = value[i];
                    }
                    UpdateMinMax();
                }
            }
        }

        // ブールマスクを使用するインデクサー
        public Tensor<T> this[Tensor<bool> mask]
        {
            get
            {
                if (!MatchShape(mask.Shape))
                    throw new ArgumentException("Mask shape must match tensor shape.");

                var selectedIndices = new List<int>();
                for (int i = 0; i < _data.Length; i++)
                {
                    if (mask._data[i])
                        selectedIndices.Add(i);
                }

                return this[selectedIndices];
            }
            set
            {
                CheckReadOnly();
                if (!MatchShape(mask.Shape))
                    throw new ArgumentException("Mask shape must match tensor shape.");

                var selectedIndices = new List<int>();
                for (int i = 0; i < _data.Length; i++)
                {
                    if (mask._data[i])
                        selectedIndices.Add(i);
                }

                this[selectedIndices] = value;
            }
        }

        // Rangeを使用するインデクサー
        public Tensor<T> this[params Range[] ranges]
        {
            get
            {
                if (ranges.Length != Shape.Length)
                    throw new ArgumentException("Number of ranges must match the number of dimensions.");

                int[] start = new int[Shape.Length];
                int[] length = new int[Shape.Length];
                int[] newShape = new int[Shape.Length];

                for (int i = 0; i < Shape.Length; i++)
                {
                    (start[i], length[i]) = ranges[i].GetOffsetAndLength(Shape[i]);
                    newShape[i] = length[i];
                }

                Tensor<T> result = new Tensor<T>(newShape);
                CopySubTensor(this, result, start, new int[Shape.Length], newShape, 0);

                return result;
            }
            set
            {
                CheckReadOnly();
                if (ranges.Length != Shape.Length)
                    throw new ArgumentException("Number of ranges must match the number of dimensions.");

                int[] start = new int[Shape.Length];
                int[] length = new int[Shape.Length];
                int[] newShape = new int[Shape.Length];

                for (int i = 0; i < Shape.Length; i++)
                {
                    (start[i], length[i]) = ranges[i].GetOffsetAndLength(Shape[i]);
                    newShape[i] = length[i];
                }
                if (value.IsScalar)
                {
                    CopySubTensor(value[0], this, start, newShape, 0);
                    UpdateMinMax(value[0]);
                }
                else
                {
                    if (!newShape.SequenceEqual(value.Shape))
                        throw new ArgumentException("Shape of the value tensor must match the selected range.");

                    CopySubTensor(value, this, new int[Shape.Length], start, newShape, 0);
                    UpdateMinMax();
                }
            }
        }

        private void CopySubTensor(Tensor<T> source, Tensor<T> target, int[] sourceStart, int[] targetStart, int[] shape, int dimension)
        {
            if (dimension == Shape.Length)
            {
                int sourceIndex = CalculateIndex(sourceStart);
                int targetIndex = CalculateIndex(targetStart);
                target._data[targetIndex] = source._data[sourceIndex];
                return;
            }

            for (int i = 0; i < shape[dimension]; i++)
            {
                sourceStart[dimension] = sourceStart[dimension] + i;
                targetStart[dimension] = targetStart[dimension] + i;
                CopySubTensor(source, target, sourceStart, targetStart, shape, dimension + 1);
                sourceStart[dimension] = sourceStart[dimension] - i;
                targetStart[dimension] = targetStart[dimension] - i;
            }
        }
        private void CopySubTensor(T source, Tensor<T> target, int[] targetStart, int[] shape, int dimension)
        {
            if (dimension == Shape.Length)
            {
                int targetIndex = CalculateIndex(targetStart);
                target._data[targetIndex] = source;
                return;
            }

            for (int i = 0; i < shape[dimension]; i++)
            {
                targetStart[dimension] = targetStart[dimension] + i;
                CopySubTensor(source, target, targetStart, shape, dimension + 1);
                targetStart[dimension] = targetStart[dimension] - i;
            }
        }

        public Tensor<T> Reshape(params int[] newShape)
        {
            int newSize = newShape.Aggregate(1, (a, b) => a * b);
            if (newSize != _data.Length)
            {
                throw new ArgumentException("New shape must have the same total number of elements.");
            }

            Tensor<T> reshaped = new Tensor<T>(newShape);
            Array.Copy(_data, reshaped._data, _data.Length);
            reshaped._max = _max;
            reshaped._min = _min;
            reshaped._isReadOnly = _isReadOnly;

            return reshaped;
        }

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
        // 単項マイナス演算子
        public static Tensor<T> operator -(Tensor<T> tensor)
        {
            return tensor.Map(NegateValue);
        }

        // 値を反転するためのヘルパーメソッド
        private static T NegateValue(T value)
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

            // その他の数値型や特殊な型の場合
            try
            {
                return (T)Convert.ChangeType(-Convert.ToDouble(value), typeof(T));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Negation is not supported for type {typeof(T).Name}.", ex);
            }
        }
        // Logical operators
        public static Tensor<bool> operator ==(Tensor<T> left, Tensor<T> right)
        {
            return ApplyLogicalOperator(left, right, (a, b) => a.CompareTo(b) == 0);
        }
        public static Tensor<bool> operator ==(Tensor<T> left, T right)
        {
            return ApplyLogicalOperator(left, right, (a, b) => a.CompareTo(b) == 0);
        }

        public static Tensor<bool> operator !=(Tensor<T> left, Tensor<T> right)
        {
            return ApplyLogicalOperator(left, right, (a, b) => a.CompareTo(b) != 0);
        }
        public static Tensor<bool> operator !=(Tensor<T> left, T right)
        {
            return ApplyLogicalOperator(left, right, (a, b) => a.CompareTo(b) != 0);
        }

        public static Tensor<bool> operator >(Tensor<T> left, Tensor<T> right)
        {
            return ApplyLogicalOperator(left, right, (a, b) => a.CompareTo(b) > 0);
        }
        public static Tensor<bool> operator >(Tensor<T> left, T right)
        {
            return ApplyLogicalOperator(left, right, (a, b) => a.CompareTo(b) > 0);
        }

        public static Tensor<bool> operator <(Tensor<T> left, Tensor<T> right)
        {
            return ApplyLogicalOperator(left, right, (a, b) => a.CompareTo(b) < 0);
        }
        public static Tensor<bool> operator <(Tensor<T> left, T right)
        {
            return ApplyLogicalOperator(left, right, (a, b) => a.CompareTo(b) < 0);
        }

        public static Tensor<bool> operator >=(Tensor<T> left, Tensor<T> right)
        {
            return ApplyLogicalOperator(left, right, (a, b) => a.CompareTo(b) >= 0);
        }
        public static Tensor<bool> operator >=(Tensor<T> left, T right)
        {
            return ApplyLogicalOperator(left, right, (a, b) => a.CompareTo(b) >= 0);
        }

        public static Tensor<bool> operator <=(Tensor<T> left, Tensor<T> right)
        {
            return ApplyLogicalOperator(left, right, (a, b) => a.CompareTo(b) <= 0);
        }
        public static Tensor<bool> operator <=(Tensor<T> left, T right)
        {
            return ApplyLogicalOperator(left, right, (a, b) => a.CompareTo(b) <= 0);
        }
        public static Tensor<T> operator &(Tensor<T> left, Tensor<T> right)
        {
            return ApplyLogicalOperator(left, right, (a, b) => BitwiseAnd(a, b));
        }

        public static Tensor<T> operator |(Tensor<T> left, Tensor<T> right)
        {
            return ApplyLogicalOperator(left, right, (a, b) => BitwiseOr(a, b));
        }

        public static Tensor<T> operator ^(Tensor<T> left, Tensor<T> right)
        {
            return ApplyLogicalOperator(left, right, (a, b) => BitwiseXor(a, b));
        }
        private static Tensor<T> ApplyLogicalOperator(Tensor<T> left, Tensor<T> right, Func<T, T, T> operation)
        {
            if (!left.Shape.SequenceEqual(right.Shape))
            {
                throw new ArgumentException("Tensors must have the same shape for logical operations.");
            }

            T[] result = new T[left._data.Length];
            for (int i = 0; i < left._data.Length; i++)
            {
                result[i] = operation(left._data[i], right._data[i]);
            }

            return new Tensor<T>(result, left.Shape);
        }


        // Helper methods for logical operations
        private static T BitwiseAnd(T a, T b)
        {
            if (typeof(T) == typeof(bool))
                return (T)(object)((bool)(object)a & (bool)(object)b);
            if (typeof(T) == typeof(int))
                return (T)(object)((int)(object)a & (int)(object)b);
            if (typeof(T) == typeof(long))
                return (T)(object)((long)(object)a & (long)(object)b);
            throw new NotSupportedException($"Bitwise AND is not supported for type {typeof(T)}");
        }

        private static T BitwiseOr(T a, T b)
        {
            if (typeof(T) == typeof(bool))
                return (T)(object)((bool)(object)a | (bool)(object)b);
            if (typeof(T) == typeof(int))
                return (T)(object)((int)(object)a | (int)(object)b);
            if (typeof(T) == typeof(long))
                return (T)(object)((long)(object)a | (long)(object)b);
            throw new NotSupportedException($"Bitwise OR is not supported for type {typeof(T)}");
        }

        private static T BitwiseXor(T a, T b)
        {
            if (typeof(T) == typeof(bool))
                return (T)(object)((bool)(object)a ^ (bool)(object)b);
            if (typeof(T) == typeof(int))
                return (T)(object)((int)(object)a ^ (int)(object)b);
            if (typeof(T) == typeof(long))
                return (T)(object)((long)(object)a ^ (long)(object)b);
            throw new NotSupportedException($"Bitwise XOR is not supported for type {typeof(T)}");
        }

        // Logical NOT (unary operator)
        public static Tensor<T> operator !(Tensor<T> tensor)
        {
            T[] result = new T[tensor._data.Length];
            for (int i = 0; i < tensor._data.Length; i++)
            {
                result[i] = LogicalNot(tensor._data[i]);
            }
            return new Tensor<T>(result, tensor.Shape);
        }

        private static T LogicalNot(T a)
        {
            if (typeof(T) == typeof(bool))
                return (T)(object)!(bool)(object)a;
            return (T)(object)!Convert.ToBoolean(a);
        }

        private static Tensor<bool> ApplyLogicalOperator(Tensor<T> left, Tensor<T> right, Func<T, T, bool> operation)
        {
            if (!left.Shape.SequenceEqual(right.Shape))
            {
                throw new ArgumentException("Tensors must have the same shape for logical operations.");
            }

            bool[] result = new bool[left._data.Length];
            for (int i = 0; i < left._data.Length; i++)
            {
                result[i] = operation(left._data[i], right._data[i]);
            }

            return new Tensor<bool>(result, left.Shape);
        }
        private static Tensor<bool> ApplyLogicalOperator(Tensor<T> left, T right, Func<T, T, bool> operation)
        {
            bool[] result = new bool[left._data.Length];
            for (int i = 0; i < left._data.Length; i++)
            {
                result[i] = operation(left._data[i], right);
            }

            return new Tensor<bool>(result, left.Shape);
        }

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

        // Override Equals and GetHashCode
        public override bool Equals(object obj)
        {
            if (obj is Tensor<T> other)
            {
                return this.Shape.SequenceEqual(other.Shape) && this._data.SequenceEqual(other._data);
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                foreach (var item in _data)
                {
                    hash = hash * 31 + item.GetHashCode();
                }
                foreach (var dim in Shape)
                {
                    hash = hash * 31 + dim.GetHashCode();
                }
                return hash;
            }
        }

        private static Tensor<T> ElementwiseOperation(Tensor<T> a, Tensor<T> b, Func<T, T, T> operation)
        {
            int[] broadcastShape = BroadcastShapes(a.Shape, b.Shape);
            Tensor<T> result = new Tensor<T>(broadcastShape);

            for (int i = 0; i < result._data.Length; i++)
            {
                int[] indices = result.GetIndices(i);
                T valueA = a.GetBroadcastValue(indices);
                T valueB = b.GetBroadcastValue(indices);
                result._data[i] = operation(valueA, valueB);
            }

            result.UpdateMinMax();
            return result;
        }

        private static Tensor<T> ElementwiseOperation(Tensor<T> a, T scalar, Func<T, T, T> operation)
        {
            Tensor<T> result = new Tensor<T>(a.Shape);

            for (int i = 0; i < a._data.Length; i++)
            {
                result._data[i] = operation(a._data[i], scalar);
            }

            result.UpdateMinMax();
            return result;
        }

        private T GetBroadcastValue(int[] indices)
        {
            int[] adjustedIndices = new int[_shape.Length];
            for (int i = 0; i < _shape.Length; i++)
            {
                adjustedIndices[i] = _shape[i] == 1 ? 0 : indices[indices.Length - _shape.Length + i];
            }
            return this[adjustedIndices];
        }

        private static int[] BroadcastShapes(int[] shapeA, int[] shapeB)
        {
            int rank = Math.Max(shapeA.Length, shapeB.Length);
            int[] result = new int[rank];

            for (int i = 0; i < rank; i++)
            {
                int dimA = i < shapeA.Length ? shapeA[shapeA.Length - 1 - i] : 1;
                int dimB = i < shapeB.Length ? shapeB[shapeB.Length - 1 - i] : 1;

                if (dimA == dimB || dimA == 1 || dimB == 1)
                {
                    result[rank - 1 - i] = Math.Max(dimA, dimB);
                }
                else
                {
                    throw new ArgumentException("Shapes are not compatible for broadcasting.");
                }
            }

            return result;
        }

        private int[] GetIndices(int flatIndex)
        {
            int[] indices = new int[_shape.Length];
            for (int i = _shape.Length - 1; i >= 0; i--)
            {
                indices[i] = flatIndex % _shape[i];
                flatIndex /= _shape[i];
            }
            return indices;
        }

        // Factory methods
        public static Tensor<T> Zeros(params int[] shape)
        {
            Tensor<T> tensor = new Tensor<T>(shape);
            for (int i = 0; i < tensor.Size(); i++)
            {
                tensor._data[i] = NumericOperations.Zero<T>();
            }
            tensor.UpdateMinMax();
            return tensor;
        }
        public static Tensor<T> Zeros<TSource>(Tensor<TSource> tensor) where TSource : IComparable<TSource> => Zeros(tensor.Shape);

        public static Tensor<T> Ones(params int[] shape)
        {
            Tensor<T> tensor = new Tensor<T>(shape);
            for (int i = 0; i < tensor.Size(); i++)
            {
                tensor._data[i] = NumericOperations.One<T>();
            }
            tensor.UpdateMinMax();
            return tensor;
        }
        public static Tensor<T> Ones<TSource>(Tensor<TSource> tensor) where TSource : IComparable<TSource> => Ones(tensor.Shape);

        // Static factory method for creating a Tensor filled with a specific value
        public static Tensor<T> Full(T value, params int[] shape)
        {
            int size = shape.Aggregate(1, (a, b) => a * b);
            T[] data = new T[size];
            Array.Fill(data, value);
            return new Tensor<T>(data, shape);
        }
        public List<int> ArgWhere<TCond>(Tensor<TCond> condition, ArgSelector.ArgSelect selectMode = ArgSelector.ArgSelect.FIRST) where TCond : IComparable<TCond>
        {
            if (!MatchShape(condition.Shape))
            {
                throw new ArgumentException("Condition tensor must have the same shape as this tensor.");
            }

            List<int> indices = new();
            for (int i = 0; i < _data.Length; i++)
            {
                if (Convert.ToBoolean(condition._data[i]))
                {
                    indices.Add(i);
                }
            }
            return indices;
        }
        // Helper method to apply a function to each element
        public Tensor<TResult> Map<TResult>(Func<T, TResult> func) where TResult : IComparable<TResult>
        {
            TResult[] newData = new TResult[_data.Length];
            for (int i = 0; i < _data.Length; i++)
            {
                newData[i] = func(_data[i]);
            }
            return new Tensor<TResult>(newData, Shape);
        }
        public Tensor<TResult> Map<TResult>(Func<T, int,TResult> func) where TResult : IComparable<TResult>
        {
            TResult[] newData = new TResult[_data.Length];
            for (int i = 0; i < _data.Length; i++)
            {
                newData[i] = func(_data[i],i);
            }
            return new Tensor<TResult>(newData, Shape);
        }
        // ArgsMax: Find all indices of maximum values
        public List<int> ArgsMax()
        {
            List<int> maxIndices = new();
            T maxValue = _data[0];

            for (int i = 0; i < _data.Length; i++)
            {
                int compResult = _data[i].CompareTo(maxValue);
                if (compResult > 0)
                {
                    maxIndices.Clear();
                    maxIndices.Add(i);
                    maxValue = _data[i];
                }
                else if (compResult == 0)
                {
                    maxIndices.Add(i);
                }
            }

            return maxIndices;
        }

        // ArgMax: Find a single index of maximum value
        public int ArgMax(ArgSelector.ArgSelect selectMode = ArgSelector.ArgSelect.FIRST)
        {
            List<int> argsMax = ArgsMax();
            return ArgSelector.Select(argsMax, selectMode);
        }

        // ArgsMin: Find all indices of minimum values
        public List<int> ArgsMin()
        {
            List<int> minIndices = new List<int>();
            T minValue = _data[0];

            for (int i = 0; i < _data.Length; i++)
            {
                int compResult = _data[i].CompareTo(minValue);
                if (compResult < 0)
                {
                    minIndices.Clear();
                    minIndices.Add(i);
                    minValue = _data[i];
                }
                else if (compResult == 0)
                {
                    minIndices.Add(i);
                }
            }

            return minIndices;
        }

        // ArgMin: Find a single index of minimum value
        public int ArgMin(ArgSelector.ArgSelect selectMode = ArgSelector.ArgSelect.FIRST)
        {
            List<int> argsMin = ArgsMin();
            return ArgSelector.Select(argsMin, selectMode);
        }

        // ScalarからTensorへの暗黙的な変換
        public static implicit operator Tensor<T>(T scalar)
        {
            return Tensor<T>.FromArray(new T[] { scalar });
        }
    }
}
