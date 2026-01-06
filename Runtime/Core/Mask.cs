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

        public readonly Mask And(Mask other)
        {
            Mask result = default;
            for (int i = 0; i < Consts.MASK_ULONGS; i++)
            {
                result.bits[i] = bits[i] & other.bits[i];
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
            while ((value & 1) == 0)
            {
                value >>= 1;
                count++;
            }
            return count;
        }

        public static Mask operator & (Mask mask, Mask other)
        {
            return mask.And(other);
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
            int hash = (int)bits[0];
            for (int i = 1; i < Consts.MASK_ULONGS; i++)
            {
                hash ^= (int)(bits[i] ^ (bits[i] >> 32));
            }
            return hash;
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