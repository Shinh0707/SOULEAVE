using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;

namespace SL.Lib
{
    public class Indice
    {
        public enum IndiceType
        {
            Single,
            Range,
            List
        }

        public IndiceType Type { get; private set; }
        public int[] Values { get; private set; }

        private Indice(IndiceType type, int[] values)
        {
            Type = type;
            Values = values;
        }

        public static implicit operator Indice(int value) => new Indice(IndiceType.Single, new[] { value });
        public static implicit operator Indice(Index index) => new Indice(IndiceType.Single, new[] { index.IsFromEnd ? -index.Value : index.Value });
        public static implicit operator Indice(Range range) => new Indice(IndiceType.Range, new[] { range.Start.Value, range.End.Value });
        public static implicit operator Indice(int[] values) => new Indice(IndiceType.List, values);
        public static implicit operator Indice(Tensor<int> values) => new Indice(IndiceType.List, values.To1DArray());
        public static implicit operator Indice(Index[] indices) => new Indice(IndiceType.List, indices.Select(i => i.IsFromEnd ? -i.Value : i.Value).ToArray());
        public static implicit operator Indice(Range[] ranges) => new Indice(IndiceType.List, ranges.SelectMany(r => new[] { r.Start.Value, r.End.Value }).ToArray());

        public int[] GetIndices(int dimSize)
        {
            switch (Type)
            {
                case IndiceType.Single:
                    int index = Values[0] < 0 ? dimSize + Values[0] : Values[0];
                    return new[] { index };
                case IndiceType.Range:
                    int start = Values[0] < 0 ? dimSize + Values[0] : Values[0];
                    int end = Values[1] < 0 ? dimSize + Values[1] : Values[1];
                    return Enumerable.Range(start, end - start).ToArray();
                case IndiceType.List:
                    return Values.Select(v => v < 0 ? dimSize + v : v).ToArray();
                default:
                    throw new InvalidOperationException("Unknown IndiceType");
            }
        }
    }
    
}