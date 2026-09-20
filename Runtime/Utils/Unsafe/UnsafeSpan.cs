using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Yogurt
{
    [DebuggerTypeProxy(typeof(UnsafeSpanDebugView<>))]
    public unsafe struct UnsafeSpan<T> where T : unmanaged, IUnmanaged<T>
    {
        public int Count { get; private set; }

        private int capacity;
        private T* items;

        public UnsafeSpan(int capacity)
        {
            this.capacity = capacity < 4 ? 4 : capacity;
            items = Allocate(this.capacity);
            Count = 0;
            InitializeRange(0, this.capacity);
        }

        public T* this[int index] => Get(index);

        /// <summary>Grows to fit <paramref name="index"/> and extends Count over it.</summary>
        public T* Get(int index)
        {
            if (items == null)
                EnsureAllocated();

            if (index >= capacity)
                Grow(index);

            if (index >= Count)
                Count = index + 1;

            return items + index;
        }

        /// <summary>Read-only lookup: out-of-range indices resolve to slot 0, an unallocated span to null.</summary>
        public T* Peek(int index)
        {
            return items == null
                ? null
                : items + ((uint)index < (uint)capacity ? index : 0);
        }

        public void Set(int index, T value)
        {
            *Get(index) = value;
        }

        public void Add(T value)
        {
            *Get(Count) = value;
        }

        /// <summary>Order-preserving removal of the last occurrence.</summary>
        public void Remove(T value)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (items[i].Equals(value))
                {
                    RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>Order-preserving removal: one memmove of the tail.</summary>
        public void RemoveAt(int index)
        {
            int tail = Count - index - 1;
            if (tail > 0)
            {
                long bytes = (long)tail * sizeof(T);
                Buffer.MemoryCopy(items + index + 1, items + index, bytes, bytes);
            }

            Count--;
        }

        /// <summary>Order-changing removal: the last element takes the freed slot.</summary>
        public void RemoveAtSwapBack(int index)
        {
            int last = Count - 1;
            if (index != last)
                items[index] = items[last];

            Count = last;
        }

        public void Clear()
        {
            Count = 0;
        }

        public void Dispose()
        {
            if (items == null)
                return;

            for (int i = 0; i < capacity; i++)
            {
                items[i].Dispose();
            }

            Marshal.FreeHGlobal((IntPtr)items);
            items = null;
            capacity = 0;
            Count = 0;
        }

        private void EnsureAllocated()
        {
            capacity = 4;
            items = Allocate(capacity);
            InitializeRange(0, capacity);
        }

        private void Grow(int index)
        {
            int oldCapacity = capacity;
            while (index >= capacity)
            {
                capacity <<= 1;
            }

            items = (T*)Marshal.ReAllocHGlobal((IntPtr)items, (IntPtr)((long)capacity * sizeof(T)));
            InitializeRange(oldCapacity, capacity);
        }

        private void InitializeRange(int from, int to)
        {
            for (int i = from; i < to; i++)
            {
                items[i].Initialize();
            }
        }

        private static T* Allocate(int count)
        {
            return (T*)Marshal.AllocHGlobal((IntPtr)((long)count * sizeof(T)));
        }
    }

    public interface IUnmanaged<T> : IDisposable, IEquatable<T> where T : unmanaged
    {
        void Initialize();
    }
}
