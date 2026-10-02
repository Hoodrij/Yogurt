using System;
using System.Runtime.InteropServices;

namespace Yogurt
{
    internal unsafe struct EntityMetas
    {
        public int Count { get; private set; }

        private byte* slots;
        private int stride;
        private int capacity;

        public EntityMetas(int capacity)
        {
            this.capacity = capacity < 4 ? 4 : capacity;
            stride = Stride(Mask.Words);
            slots = Allocate((long)this.capacity * stride);
            Count = 0;
        }

        // Out-of-range indices resolve to slot 0, the never-alive Entity.Null slot.
        public EntityMeta* Peek(int index)
        {
            return (EntityMeta*)(slots + (long)((uint)index < (uint)capacity ? index : 0) * stride);
        }

        public EntityMeta* Get(int index)
        {
            if (index >= capacity)
                Grow(index);

            if (index >= Count)
                Count = index + 1;

            return (EntityMeta*)(slots + (long)index * stride);
        }

        public void Relayout(int oldWords, int newWords)
        {
            int newStride = Stride(newWords);
            byte* newSlots = Allocate((long)capacity * newStride);
            long metaBytes = sizeof(EntityMeta);
            long oldMaskBytes = (long)oldWords * sizeof(ulong);

            for (int i = 0; i < capacity; i++)
            {
                byte* from = slots + (long)i * stride;
                byte* to = newSlots + (long)i * newStride;
                ulong* fromMasks = (ulong*)(from + metaBytes);
                ulong* toMasks = (ulong*)(to + metaBytes);

                Buffer.MemoryCopy(from, to, metaBytes, metaBytes);
                Buffer.MemoryCopy(fromMasks, toMasks, oldMaskBytes, oldMaskBytes);
                Buffer.MemoryCopy(fromMasks + oldWords, toMasks + newWords, oldMaskBytes, oldMaskBytes);
            }

            Marshal.FreeHGlobal((IntPtr)slots);
            slots = newSlots;
            stride = newStride;
        }

        public void Dispose()
        {
            if (slots == null)
                return;

            for (int i = 0; i < capacity; i++)
            {
                Peek(i)->Dispose();
            }

            Marshal.FreeHGlobal((IntPtr)slots);
            slots = null;
            capacity = 0;
            Count = 0;
        }

        private void Grow(int index)
        {
            int oldCapacity = capacity;
            while (index >= capacity)
            {
                capacity <<= 1;
            }

            slots = (byte*)Marshal.ReAllocHGlobal((IntPtr)slots, (IntPtr)((long)capacity * stride));
            Clear(slots + (long)oldCapacity * stride, (long)(capacity - oldCapacity) * stride);
        }

        private static int Stride(int maskWords) => sizeof(EntityMeta) + maskWords * 2 * sizeof(ulong);

        private static byte* Allocate(long bytes)
        {
            byte* memory = (byte*)Marshal.AllocHGlobal((IntPtr)bytes);
            Clear(memory, bytes);
            return memory;
        }

        private static void Clear(byte* memory, long bytes)
        {
            while (bytes > 0)
            {
                int chunk = (int)Math.Min(bytes, int.MaxValue);
                new Span<byte>(memory, chunk).Clear();
                memory += chunk;
                bytes -= chunk;
            }
        }
    }
}
