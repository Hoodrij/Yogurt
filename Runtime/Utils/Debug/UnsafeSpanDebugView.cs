using System.Collections.Generic;

namespace Yogurt
{
    internal class UnsafeSpanDebugView<T> where T : unmanaged, IUnmanaged<T>
    {
        public int Count => span.Count;
        public List<T> Items => GetItems();

        private UnsafeSpan<T> span;
            
        public UnsafeSpanDebugView(UnsafeSpan<T> span)
        {
            this.span = span;
        }

        private unsafe List<T> GetItems()
        {
            List<T> result = new();
            for (int i = 0; i < span.Count; i++)
            {
                T* t = span.Get(i);
                result.Add(*t);
            }

            return result;
        }
    }
}