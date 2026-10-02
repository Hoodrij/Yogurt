using System;
using System.Runtime.CompilerServices;

namespace Yogurt
{
    internal static unsafe class Mask
    {
        internal static int Words { get; private set; } = 1;

        private const ulong DeBruijn = 0x03F79D71B4CA8B09UL;
        private static readonly byte[] trailingZeros = BuildTrailingZeros();

        internal static void EnsureCapacity(int componentCount)
        {
            int required = (componentCount + 63) >> 6;
            if (required <= Words)
                return;

            int previous = Words;
            Words = required;
            WorldFacade.World?.EntitiesMetas.Relayout(previous, required);
        }

        // C# masks ulong shift counts to 6 bits, so `1UL << id` selects the bit inside the word.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool Has(ulong* words, ushort id) => (words[id >> 6] & (1UL << id)) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Set(ulong* words, ushort id) => words[id >> 6] |= 1UL << id;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void UnSet(ulong* words, ushort id) => words[id >> 6] &= ~(1UL << id);

        internal static void Clear(ulong* words, int count)
        {
            for (int i = 0; i < count; i++)
            {
                words[i] = 0;
            }
        }

        internal static ulong[] With(ulong[] words, ushort id)
        {
            ulong[] result = new ulong[Math.Max(words.Length, (id >> 6) + 1)];
            Array.Copy(words, result, words.Length);
            result[id >> 6] |= 1UL << id;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int TrailingZeroCount(ulong nonZero)
        {
            return trailingZeros[((nonZero ^ (nonZero - 1)) * DeBruijn) >> 58];
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

        internal static string Names(ulong* words, int count, string prefix = "")
        {
            string result = string.Empty;
            for (int i = 0; i < count; i++)
            {
                ulong word = words[i];
                while (word != 0)
                {
                    ComponentID id = (ushort)((i << 6) + TrailingZeroCount(word));
                    word &= word - 1;
                    result += (result.Length == 0 ? prefix : ", " + prefix) + id.Name;
                }
            }
            return result;
        }
    }
}
