using System;
using System.Collections.Generic;
using Random = System.Random;

namespace SL.Lib
{
    public partial class Tensor<T> where T : IComparable<T>
    {
        /// <summary>
        /// Returns the indices where the condition tensor is true.
        /// </summary>
        /// <typeparam name="TCond">The type of the condition tensor.</typeparam>
        /// <param name="condition">The condition tensor.</param>
        /// <returns>A list of indices where the condition is true.</returns>
        public List<Indice[]> ArgWhere<TCond>(Tensor<TCond> condition) where TCond : IComparable<TCond>
        {
            if (!MatchShape(condition.Shape))
            {
                throw new ArgumentException("Condition tensor must have the same shape as this tensor.");
            }
            List<Indice[]> indices = new List<Indice[]>();
            for (int i = 0; i < _data.Length; i++)
            {
                if (Convert.ToBoolean(condition._data[i]))
                {
                    indices.Add(condition.GetIndices(i));
                }
            }
            return indices;
        }

        /// <summary>
        /// Returns the indices where the tensor elements are true.
        /// </summary>
        /// <returns>A list of indices where the elements are true.</returns>
        public List<Indice[]> ArgWhere()
        {
            List<Indice[]> indices = new List<Indice[]>();
            for (int i = 0; i < _data.Length; i++)
            {
                if (Convert.ToBoolean(_data[i]))
                {
                    indices.Add(GetIndices(i));
                }
            }
            return indices;
        }

        /// <summary>
        /// Finds all indices of maximum values in the tensor.
        /// </summary>
        /// <returns>A list of indices with maximum values.</returns>
        public List<Indice[]> ArgsMax()
        {
            List<Indice[]> maxIndices = new List<Indice[]>();
            T maxValue = _data[0];
            for (int i = 0; i < _data.Length; i++)
            {
                int compResult = _data[i].CompareTo(maxValue);
                if (compResult > 0)
                {
                    maxIndices.Clear();
                    maxIndices.Add(GetIndices(i));
                    maxValue = _data[i];
                }
                else if (compResult == 0)
                {
                    maxIndices.Add(GetIndices(i));
                }
            }
            return maxIndices;
        }

        /// <summary>
        /// Finds a single index of the maximum value in the tensor.
        /// </summary>
        /// <param name="selectMode">The mode for selecting the index when multiple maximum values exist.</param>
        /// <returns>The index of the maximum value.</returns>
        public Indice[] ArgMax(ArgSelector.ArgSelect selectMode = ArgSelector.ArgSelect.FIRST)
        {
            var argsMax = ArgsMax();
            return ArgSelector.Select(argsMax, selectMode);
        }

        /// <summary>
        /// Finds all indices of minimum values in the tensor.
        /// </summary>
        /// <returns>A list of indices with minimum values.</returns>
        public List<Indice[]> ArgsMin()
        {
            List<Indice[]> minIndices = new List<Indice[]>();
            T minValue = _data[0];
            for (int i = 0; i < _data.Length; i++)
            {
                int compResult = _data[i].CompareTo(minValue);
                if (compResult < 0)
                {
                    minIndices.Clear();
                    minIndices.Add(GetIndices(i));
                    minValue = _data[i];
                }
                else if (compResult == 0)
                {
                    minIndices.Add(GetIndices(i));
                }
            }
            return minIndices;
        }

        /// <summary>
        /// Finds a single index of the minimum value in the tensor.
        /// </summary>
        /// <param name="selectMode">The mode for selecting the index when multiple minimum values exist.</param>
        /// <returns>The index of the minimum value.</returns>
        public Indice[] ArgMin(ArgSelector.ArgSelect selectMode = ArgSelector.ArgSelect.FIRST)
        {
            var argsMin = ArgsMin();
            return ArgSelector.Select(argsMin, selectMode);
        }
    }

    /// <summary>
    /// Provides methods for selecting arguments based on different criteria.
    /// </summary>
    public static class ArgSelector
    {
        private static Random _random;

        /// <summary>
        /// Gets or initializes the random number generator.
        /// </summary>
        public static Random Random
        {
            get
            {
                return (_random ??= new Random());
            }
        }

        /// <summary>
        /// Defines the selection mode for arguments.
        /// </summary>
        public enum ArgSelect
        {
            FIRST,
            LAST,
            RANDOM
        }

        /// <summary>
        /// Selects a value from the list based on the specified selection mode.
        /// </summary>
        /// <typeparam name="TValue">The type of values in the list.</typeparam>
        /// <param name="list">The list of values to select from.</param>
        /// <param name="argSelect">The selection mode.</param>
        /// <returns>The selected value.</returns>
        public static TValue Select<TValue>(List<TValue> list, ArgSelect argSelect)
        {
            if (list == null || list.Count == 0)
            {
                throw new ArgumentException("List cannot be null or empty.");
            }

            return list[argSelect switch
            {
                ArgSelect.FIRST => 0,
                ArgSelect.LAST => list.Count - 1,
                ArgSelect.RANDOM => Random.Next(0, list.Count),
                _ => throw new ArgumentException("Invalid ArgSelect value.")
            }];
        }
    }

}
