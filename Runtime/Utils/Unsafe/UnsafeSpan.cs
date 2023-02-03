using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Yogurt
{
    [DebuggerTypeProxy(typeof(UnsafeSpanDebugView<>))]
    public unsafe struct UnsafeSpan<T> where T : unmanaged, IUnmanaged<T>
    {
        public int Count { get; private set; }

        private int elementSize;
        private int capacity;
        private IntPtr memoryPointer;

        public UnsafeSpan(int capacity)
        {
            this.capacity = capacity < 4 ? 4 : capacity;
            elementSize = sizeof(T);
            memoryPointer = Marshal.AllocHGlobal(this.capacity * elementSize);
            Count = 0;

            for (int i = 0; i < capacity; i++)
            {
                GetUnsafe(i)->Initialize();
            }
        }

        public T* this[int index] => Get(index);

        public T* Get(int index)
        {
            if (index >= capacity)
            {
                capacity <<= 1;
                memoryPointer = Marshal.ReAllocHGlobal(memoryPointer, (IntPtr)(capacity * elementSize));
                
                for (int i = capacity >> 1; i < capacity; i++)
                {
                    GetUnsafe(i)->Initialize();
                }
            }
            if (index + 1 > Count)
            {
                Count = index + 1;
            }

            return GetUnsafe(index);
        }
        
        private T* GetUnsafe(int index)
        {
            return (T*) (memoryPointer + index * elementSize);
        }

        public void Set(int index, T value)
        {
            T* t = Get(index);
            *t = value;
        }

        public void Add(T value)
        {
            Set(Count, value);
        }

        public void Remove(T value)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (GetUnsafe(i)->Equals(value))
                {
                    for (int j = i; j < Count; ++j)
                    {
                        T* next = GetUnsafe(j+1);
                        Set(j, *next);
                    }
        
                    Count--;
                    break;
                }
            }
        }

        public void Clear()
        {
            Count = 0;
        }

        public void Dispose()
        {
            for (int i = 0; i < capacity; i++)
            {
                GetUnsafe(i)->Dispose();
            }
            
            Marshal.FreeHGlobal(memoryPointer);
            memoryPointer = IntPtr.Zero;
            capacity = 0;
            Count = 0;
        }
        
    }

    public interface IUnmanaged<T> : IDisposable, IEquatable<T> where T : unmanaged
    {
        void Initialize();
    }
}