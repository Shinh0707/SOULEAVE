using System;

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
    }
}