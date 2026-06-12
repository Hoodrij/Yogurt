using System;
using System.Diagnostics;

namespace Yogurt
{
    [DebuggerDisplay("{Name}")]
    internal unsafe struct Mask : IComparable<Mask>, IEquatable<Mask>
    {
        private fixed ulong bits[Consts.MASK_ULONGS];

        public readonly bool IsEmpty
        {
            get
            {
                for (int i = 0; i < Consts.MASK_ULONGS; i++)
                {
                    if (bits[i] != 0) 
                        return false;
                }

                return true;
            }
        }

        public void Set(ushort componentId)
        {
            int ulongIndex = componentId >> 6;
            int bitIndex = componentId & 63;
            bits[ulongIndex] |= 1UL << bitIndex;
        }

        public void UnSet(ushort componentId)
        {
            int ulongIndex = componentId >> 6;
            int bitIndex = componentId & 63;

            bits[ulongIndex] &= ~(1UL << bitIndex);
        }

        public readonly bool Has(ushort componentId)
        {
            int ulongIndex = componentId >> 6;
            int bitIndex = componentId & 63;

            return (bits[ulongIndex] & (1UL << bitIndex)) != 0;
        }

        public readonly bool HasAny(Mask other)
        {
            for (int i = 0; i < Consts.MASK_ULONGS; i++)
            {
                if ((bits[i] & other.bits[i]) != 0)
                    return true;
            }

            return false;
        }

        public readonly bool HasAll(Mask other)
        {
            for (int i = 0; i < Consts.MASK_ULONGS; i++)
            {
                if ((bits[i] & other.bits[i]) != other.bits[i])
                    return false;
            }

            return true;
        }

        public void Clear()
        {
            for (int i = 0; i < Consts.MASK_ULONGS; i++)
            {
                bits[i] = 0;
            }
        }

        public readonly Mask Or(Mask other)
        {
            Mask result = default;
            for (int i = 0; i < Consts.MASK_ULONGS; i++)
            {
                result.bits[i] = bits[i] | other.bits[i];
            }

            return result;
        }

        public readonly int GetIDs(Span<ComponentID> buffer)
        {
            int count = 0;
            for (int ulongIndex = 0; ulongIndex < Consts.MASK_ULONGS; ulongIndex++)
            {
                ulong current = bits[ulongIndex];
                while (current != 0)
                {
                    int bitIndex = TrailingZeroCount(current);
                    buffer[count++] = (ushort)(ulongIndex * 64 + bitIndex);
                    current &= current - 1; // Clear the lowest set bit
                }
            }
            return count;
        }

        private static int TrailingZeroCount(ulong value)
        {
            if (value == 0) return 64;
            int count = 0;
            if ((value & 0xFFFFFFFFUL) == 0) { value >>= 32; count += 32; }
            if ((value & 0xFFFFUL) == 0) { value >>= 16; count += 16; }
            if ((value & 0xFFUL) == 0) { value >>= 8; count += 8; }
            if ((value & 0xFUL) == 0) { value >>= 4; count += 4; }
            if ((value & 0x3UL) == 0) { value >>= 2; count += 2; }
            if ((value & 0x1UL) == 0) { count += 1; }
            return count;
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
            for (int i = 0; i < Consts.MASK_ULONGS; i++)
            {
                if (bits[i] != other.bits[i])
                    return false;
            }

            return true;
        }

        public readonly override int GetHashCode()
        {
            // FNV-1a hash for better distribution
            unchecked
            {
                const uint FNV_prime = 16777619;
                uint hash = 2166136261;

                for (int i = 0; i < Consts.MASK_ULONGS; i++)
                {
                    hash = (hash ^ (uint)bits[i]) * FNV_prime;
                    hash = (hash ^ (uint)(bits[i] >> 32)) * FNV_prime;
                }

                return (int)hash;
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