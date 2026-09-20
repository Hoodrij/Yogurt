using System;
using System.Diagnostics;

namespace Yogurt
{
    [DebuggerDisplay("{Name}")]
    internal unsafe struct Mask : IComparable<Mask>, IEquatable<Mask>
    {
        private fixed ulong bits[Consts.MASK_ULONGS];

        // De Bruijn multiply: branch-free trailing-zero count for a non-zero word.
        private const ulong DeBruijn = 0x03F79D71B4CA8B09UL;
        private static readonly byte[] trailingZeros = BuildTrailingZeros();

        public readonly bool IsEmpty
        {
            get
            {
                ulong any = 0;
                for (int i = 0; i < Consts.MASK_ULONGS; i++)
                {
                    any |= bits[i];
                }

                return any == 0;
            }
        }

        // Shift counts on ulong are masked to 6 bits by C#, so `1UL << componentId` selects the bit inside the word.
        public void Set(ushort componentId) => bits[componentId >> 6] |= 1UL << componentId;

        public void UnSet(ushort componentId) => bits[componentId >> 6] &= ~(1UL << componentId);

        public readonly bool Has(ushort componentId) => (bits[componentId >> 6] & (1UL << componentId)) != 0;

        public readonly bool HasAny(in Mask other)
        {
            ulong common = 0;
            for (int i = 0; i < Consts.MASK_ULONGS; i++)
            {
                common |= bits[i] & other.bits[i];
            }

            return common != 0;
        }

        public readonly bool HasAll(in Mask other)
        {
            // A bit that is set in `other` but not here leaves a 1 in `missing`.
            ulong missing = 0;
            for (int i = 0; i < Consts.MASK_ULONGS; i++)
            {
                missing |= ~bits[i] & other.bits[i];
            }

            return missing == 0;
        }

        public void Clear()
        {
            for (int i = 0; i < Consts.MASK_ULONGS; i++)
            {
                bits[i] = 0;
            }
        }

        public readonly Mask Or(in Mask other)
        {
            Mask result = default;
            for (int i = 0; i < Consts.MASK_ULONGS; i++)
            {
                result.bits[i] = bits[i] | other.bits[i];
            }

            return result;
        }

        // Consume one set bit without materializing an ID buffer.
        public bool TryPopFirst(out ComponentID componentId)
        {
            for (int i = 0; i < Consts.MASK_ULONGS; i++)
            {
                ulong word = bits[i];
                if (word == 0)
                    continue;

                bits[i] = word & (word - 1);
                componentId = (ushort)((i << 6) + TrailingZeroCount(word));
                return true;
            }

            componentId = default;
            return false;
        }

        public readonly int GetIDs(Span<ComponentID> buffer)
        {
            int count = 0;
            for (int ulongIndex = 0; ulongIndex < Consts.MASK_ULONGS; ulongIndex++)
            {
                ulong current = bits[ulongIndex];
                while (current != 0)
                {
                    buffer[count++] = (ushort)((ulongIndex << 6) + TrailingZeroCount(current));
                    current &= current - 1; // Clear the lowest set bit
                }
            }
            return count;
        }

        /// <summary>Trailing zero count of a NON-ZERO value.</summary>
        private static int TrailingZeroCount(ulong value)
        {
            // value ^ (value - 1) isolates the lowest set bit plus all trailing zeros below it.
            return trailingZeros[((value ^ (value - 1)) * DeBruijn) >> 58];
        }

        private static byte[] BuildTrailingZeros()
        {
            byte[] table = new byte[64];
            for (int i = 0; i < 64; i++)
            {
                ulong bit = 1UL << i;
                table[((bit ^ (bit - 1)) * DeBruijn) >> 58] = (byte)i;
            }
            return table;
        }

        public override string ToString()
        {
            return Name;
        }

        public readonly int CompareTo(Mask other)
        {
            for (int i = Consts.MASK_ULONGS - 1; i >= 0; i--)
            {
                if (bits[i] != other.bits[i])
                    return bits[i].CompareTo(other.bits[i]);
            }

            return 0;
        }

        public readonly bool Equals(Mask other)
        {
            ulong difference = 0;
            for (int i = 0; i < Consts.MASK_ULONGS; i++)
            {
                difference |= bits[i] ^ other.bits[i];
            }

            return difference == 0;
        }

        public override bool Equals(object obj)
        {
            return obj is Mask other && Equals(other);
        }

        public readonly override int GetHashCode()
        {
            unchecked
            {
                // Multiply-xor fold of the words; distributes sparse bit patterns well.
                const ulong prime = 0x9E3779B97F4A7C15UL;
                ulong hash = 0;
                for (int i = 0; i < Consts.MASK_ULONGS; i++)
                {
                    hash = (hash ^ bits[i]) * prime;
                }

                return (int)(hash ^ (hash >> 32));
            }
        }

        private readonly string Name
        {
            get
            {
                Span<ComponentID> buffer = stackalloc ComponentID[Consts.MAX_COMPONENTS];
                int count = GetIDs(buffer);

                if (count == 0)
                    return string.Empty;

                string[] names = new string[count];
                for (int i = 0; i < count; i++)
                {
                    names[i] = ((ComponentID)buffer[i]).Name;
                }

                return string.Join(", ", names);
            }
        }
    }
}
