using System;
using System.Collections.Generic;

namespace Yogurt
{
    public struct HashCode : IComparable<HashCode>, IEquatable<HashCode>
    {
        public readonly int value;

        private HashCode(int value)
        {
            this.value = value;
        }

        public static implicit operator int(HashCode hashCode)
        {
            return hashCode.value;
        }
        
        public static implicit operator HashCode(int value)
        {
            return new HashCode(value);
        }

        public static HashCode Of<T>(T item)
        {
            return new (GetHashCode(item));
        }

        public static HashCode OfEach<T>(IEnumerable<T> items)
        {
            return new (GetHashCode(items, 0));
        }

        public HashCode And<T>(T item)
        {
            return new (CombineHashCodes(value, GetHashCode(item)));
        }

        public HashCode AndEach<T>(IEnumerable<T> items)
        {
            return items == null ? new HashCode(value) : new HashCode(GetHashCode(items, value));
        }

        private static int CombineHashCodes(int h1, int h2)
        {
            unchecked
            {
                // Code copied from System.Tuple so it must be the best way to combine hash codes or at least a good one.
                return ((h1 << 5) + h1) ^ h2;
            }
        }

        private static int GetHashCode<T>(T item)
        {
            return item == null ? 0 : item.GetHashCode();
        }

        private static int GetHashCode<T>(IEnumerable<T> items, int startHashCode)
        {
            int temp = startHashCode;
            foreach (T item in items)
            {
                temp = CombineHashCodes(temp, GetHashCode(item));
            }

            return temp;
        }

        public int CompareTo(HashCode other)
        {
            return value.CompareTo(other.value);
        }

        public bool Equals(HashCode other)
        {
            return value.Equals(other.value);
        }

        public override int GetHashCode()
        {
            return value;
        }
    }
}