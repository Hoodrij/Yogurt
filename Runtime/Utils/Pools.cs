using System;
using System.Collections.Generic;

namespace Yogurt
{
    // Simple pools that replace UnityEngine.Pool outside Unity.
    internal sealed class ObjectPool<T> where T : class
    {
        private readonly Stack<T> items = new();
        private readonly Func<T> create;

        public ObjectPool(Func<T> create) => this.create = create;

        public T Get() => items.Count > 0 ? items.Pop() : create();

        public void Release(T item) => items.Push(item);
    }

    internal static class ListPool<T>
    {
        private static readonly Stack<List<T>> lists = new();

        public static void Get(out List<T> list) => list = lists.Count > 0 ? lists.Pop() : new List<T>();

        public static void Release(List<T> list)
        {
            list.Clear();
            lists.Push(list);
        }
    }
}
