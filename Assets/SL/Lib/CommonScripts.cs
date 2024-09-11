using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SL.Lib
{
    public static class ArrayExtensions
    {
        public static void Insert<T>(this T[] src, T insertValue, int insertIndex, T[] dst)
        {
            Array.Copy(src, 0, dst, 0, insertIndex);
            dst[insertIndex] = insertValue;
            Array.Copy(src, insertIndex, dst, insertIndex + 1, src.Length - insertIndex);
        }
        public static T[] Insert<T>(this T[] src, T insertValue, int insertIndex)
        {
            var dst = new T[src.Length + 1];
            Insert(src, insertValue, insertIndex, dst);
            return dst;
        }
        public static int IndexOf<T>(this IReadOnlyList<T> list, T item)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            // List<T>の場合、既存のIndexOfメソッドを使用
            if (list is List<T> asList)
                return asList.IndexOf(item);

            // T[]の場合、Array.IndexOfを使用
            if (list is T[] asArray)
                return Array.IndexOf(asArray, item);

            // その他の場合、手動で探索
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < list.Count; i++)
            {
                if (comparer.Equals(list[i], item))
                    return i;
            }

            return -1;  // 見つからなかった場合
        }
    }

    public static class CollisionExtensions
    {
        public static LayerMask RemoveLayer(this LayerMask layerMask, int layerNumber)
        {
            // レイヤーを削除
            if (layerMask.HasLayer(layerNumber))
            {
                return layerMask & ~(1 << layerNumber);
            }
            return layerMask;
        }
        public static LayerMask RemoveLayer(this LayerMask layerMask, LayerMask layer) => layerMask.RemoveLayer(layer.value);

        public static LayerMask RemoveLayer(this LayerMask layerMask, string layerName)
        {
            int layerNumber = LayerMask.NameToLayer(layerName);
            if (layerNumber == -1)
            {
                Debug.LogError($"Layer '{layerName}' does not exist.");
                return layerMask;
            }
            return layerMask.RemoveLayer(layerNumber);
        }
        public static LayerMask AddLayer(this LayerMask layerMask, int layerNumber)
        {
            if (!layerMask.HasLayer(layerNumber))
            {
                return layerMask | (1 << layerNumber);
            }
            return layerMask;
        }
        public static LayerMask AddLayer(this LayerMask layerMask, LayerMask layer) => layerMask.AddLayer(layer.value);
        public static LayerMask AddLayer(this LayerMask layerMask, string layerName)
        {
            int layerNumber = LayerMask.NameToLayer(layerName);
            if (layerNumber == -1)
            {
                Debug.LogError($"Layer '{layerName}' does not exist.");
                return layerMask;
            }
            return layerMask.AddLayer(layerNumber);
        }
        public static bool HasLayer(this LayerMask layerMask, int layerNumber) => (layerMask.value & (1 << layerNumber)) != 0;
        public static bool HasLayer(this LayerMask layerMask, LayerMask layer) => layerMask.HasLayer(layer.value);
        public static bool HasLayer(this LayerMask layerMask, string layerName)
        {
            int layerNumber = LayerMask.NameToLayer(layerName);
            if (layerNumber == -1)
            {
                Debug.LogWarning($"Layer '{layerName}' does not exist.");
                return false;
            }
            return layerMask.HasLayer(layerNumber);
        }
        public static void AddIncludeLayer(this Collider2D collider, int layerToCheck)
        {
            collider.includeLayers = collider.includeLayers.AddLayer(layerToCheck);
        }
        public static void AddIncludeLayer(this Collider2D collider, LayerMask layerToCheck) => collider.AddIncludeLayer(layerToCheck.value);
        public static void AddIncludeLayer(this Collider2D collider, string layerToCheck)
        {
            collider.includeLayers = collider.includeLayers.AddLayer(layerToCheck);
        }
        public static void AddExcludeLayer(this Collider2D collider, int layerToCheck)
        {
            collider.excludeLayers = collider.excludeLayers.AddLayer(layerToCheck);
        }
        public static void AddExcludeLayer(this Collider2D collider, LayerMask layerToCheck) => collider.AddExcludeLayer(layerToCheck.value);
        public static void AddExcludeLayer(this Collider2D collider, string layerToCheck)
        {
            collider.excludeLayers = collider.excludeLayers.AddLayer(layerToCheck);
        }
        public static void RemoveExcludeLayer(this Collider2D collider, int layerToCheck)
        {
            collider.excludeLayers = collider.excludeLayers.RemoveLayer(layerToCheck);
        }
        public static void RemoveExcludeLayer(this Collider2D collider, LayerMask layerToCheck) => collider.RemoveExcludeLayer(layerToCheck.value);
        public static void RemoveExcludeLayer(this Collider2D collider, string layerToCheck)
        {
            collider.excludeLayers = collider.excludeLayers.RemoveLayer(layerToCheck);
        }
    }

    public static class ComponentExtension
    {
        public static T GetOrAddComponent<T>(this Component obj) where T : Component
        {
            if (obj.TryGetComponent(out T component))
            {
                return component;
            }
            return obj.AddComponent<T>();
        }
        public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
        {
            if (obj.TryGetComponent(out T component))
            {
                return component;
            }
            return obj.AddComponent<T>();
        }
    }

    public class SLRandom
    {
        private static System.Random _random;
        public static System.Random Random => _random ??= new System.Random();
        public static T SelectRandom<T>(IEnumerable<T> pool) => pool.ElementAt(Random.Next(pool.Count()));
        public static T SelectRandom<T>(T[] pool) => pool[Random.Next(pool.Length)];
        public static int SelectRandomIndex<T>(IEnumerable<T> pool, T value) where T : IComparable<T>
        {
            var selectList = pool.Select((v, i) => (v, i)).Where((v) => v.v.Equals(value));
            return SelectRandom(selectList.Select((v) => v.i).ToList());
        }
        public static T Choice<T>(T[] pool, float[] weights)
        {
            if (pool == null || weights == null || pool.Length != weights.Length || pool.Length == 0)
            {
                throw new ArgumentException("Invalid input: pool and weights must be non-null, have the same length, and contain at least one element.");
            }

            float totalWeight = weights.Sum();
            if (totalWeight <= 0)
            {
                throw new ArgumentException("Total weight must be positive.");
            }

            float randomValue = (float)Random.NextDouble() * totalWeight;

            for (int i = 0; i < pool.Length; i++)
            {
                if (randomValue < weights[i])
                {
                    return pool[i];
                }
                randomValue -= weights[i];
            }

            // This should never happen if the weights sum to totalWeight
            return pool[pool.Length - 1];
        }
        public static T Choice<T>(T[] pool) => SelectRandom(pool);
        public static T Choice<T>(IEnumerable<T> pool) => SelectRandom(pool);
        public static T[] Choices<T>(IEnumerable<T> pool, int k)
        {
            if (pool == null)
                throw new ArgumentNullException(nameof(pool));

            var poolList = new List<T>(pool);
            int n = poolList.Count;

            if (k < 0 || k > n)
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between 0 and the size of the pool.");

            T[] result = new T[k];
            for (int i = 0; i < k; i++)
            {
                int j = Random.Next(i, n);
                result[i] = poolList[j];
                poolList[j] = poolList[i];
            }

            return result;
        }
        public static T[] Sample<T>(IEnumerable<T> pool, int k)
        {
            if (pool == null)
                throw new ArgumentNullException(nameof(pool));

            var poolList = new List<T>(pool);
            int n = poolList.Count;

            if (k < 0)
                throw new ArgumentOutOfRangeException(nameof(k), "k must be non-negative.");

            if (n == 0 && k > 0)
                throw new InvalidOperationException("Cannot sample from an empty pool.");

            T[] result = new T[k];
            for (int i = 0; i < k; i++)
            {
                int j = Random.Next(0, n);
                result[i] = poolList[j];
            }

            return result;
        }
    }
    public static class ShadowCaster2DExtensions
    {
        public static void SetPath(this ShadowCaster2D shadowCaster, Vector3[] path)
        {
            FieldInfo shapeField = typeof(ShadowCaster2D).GetField("m_ShapePath",
                                                                   BindingFlags.NonPublic |
                                                                   BindingFlags.Instance);
            shapeField.SetValue(shadowCaster, path);
        }

        public static void SetPathHash(this ShadowCaster2D shadowCaster, int hash)
        {
            FieldInfo hashField = typeof(ShadowCaster2D).GetField("m_ShapePathHash",
                                                                  BindingFlags.NonPublic |
                                                                  BindingFlags.Instance);
            hashField.SetValue(shadowCaster, hash);
        }
    }


    public static class AdvancedRomanNumeralConverter
    {
        private static readonly Dictionary<int, string> romanNumerals = new Dictionary<int, string>
    {
        { 1000, "M" },
        { 900, "CM" },
        { 500, "D" },
        { 400, "CD" },
        { 100, "C" },
        { 90, "XC" },
        { 50, "L" },
        { 40, "XL" },
        { 10, "X" },
        { 9, "IX" },
        { 5, "V" },
        { 4, "IV" },
        { 1, "I" }
    };

        private const long MAX_STANDARD_VALUE = 3999;
        private const long BRACKET_THRESHOLD = 500000;
        private const long INFINITY_THRESHOLD = 1000000000000; // 1兆

        public static string ConvertToRomanNumeral(long number)
        {
            if (number < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(number), "Number must be positive.");
            }

            if (number >= INFINITY_THRESHOLD)
            {
                return "∞"; // 無限大記号
            }

            StringBuilder result = new StringBuilder();

            if (number > MAX_STANDARD_VALUE)
            {
                result.Append(ConvertLargeNumber(number));
            }
            else
            {
                result.Append(ConvertStandardNumber((int)number));
            }

            return result.ToString();
        }

        private static string ConvertStandardNumber(int number)
        {
            StringBuilder result = new StringBuilder();
            foreach (var item in romanNumerals)
            {
                while (number >= item.Key)
                {
                    result.Append(item.Value);
                    number -= item.Key;
                }
            }
            return result.ToString();
        }

        private static string ConvertLargeNumber(long number)
        {
            StringBuilder result = new StringBuilder();
            int bracketCount = 0;

            while (number >= 1000)
            {
                bracketCount++;
                number /= 1000;
            }

            string core = ConvertStandardNumber((int)number);

            if (bracketCount >= 2 || number >= BRACKET_THRESHOLD / 1000)
            {
                result.Append('(', bracketCount);
                result.Append(core);
                result.Append(')', bracketCount);
            }
            else
            {
                result.Append(core);
                result.Append('M', bracketCount);
            }

            return result.ToString();
        }

        public static string ConvertToRomanNumeralOrDefault(long number, string defaultValue = "")
        {
            if (number < 1)
            {
                return defaultValue;
            }

            return ConvertToRomanNumeral(number);
        }
    }
}