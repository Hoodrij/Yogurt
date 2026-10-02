using System;
using System.Diagnostics;

namespace Yogurt
{
    // Words are trimmed to the highest non-zero word, so equal sets compare equal at any mask width.
    [DebuggerDisplay("{Name}")]
    internal readonly struct Composition : IEquatable<Composition>
    {
        internal static readonly Composition Empty = new(Array.Empty<ulong>(), Array.Empty<ulong>());

        internal readonly ulong[] Included;
        internal readonly ulong[] Excluded;
        private readonly int hash;

        private Composition(ulong[] included, ulong[] excluded)
        {
            Included = included;
            Excluded = excluded;

            unchecked
            {
                const ulong prime = 0x9E3779B97F4A7C15UL;
                ulong h = (ulong)included.Length;
                foreach (ulong word in included)
                {
                    h = (h ^ word) * prime;
                }

                foreach (ulong word in excluded)
                {
                    h = (h ^ word) * prime;
                }
                hash = (int)(h ^ (h >> 32));
            }
        }

        internal Composition With(ushort componentId) => new(Mask.With(Included, componentId), Excluded);
        internal Composition Without(ushort componentId) => new(Included, Mask.With(Excluded, componentId));

        public override int GetHashCode() => hash;

        public bool Equals(Composition other)
        {
            return hash == other.hash
                && Included.AsSpan().SequenceEqual(other.Included)
                && Excluded.AsSpan().SequenceEqual(other.Excluded);
        }

        public override bool Equals(object obj)
        {
            return obj is Composition other && Equals(other);
        }

        public override string ToString() => Name;

        private unsafe string Name
        {
            get
            {
                fixed (ulong* included = Included)
                fixed (ulong* excluded = Excluded)
                {
                    string with = Mask.Names(included, Included.Length);
                    string without = Mask.Names(excluded, Excluded.Length, "!");
                    return without.Length == 0 ? with : with.Length == 0 ? without : with + ", " + without;
                }
            }
        }
    }
}
