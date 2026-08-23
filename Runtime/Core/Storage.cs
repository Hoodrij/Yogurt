using System;
using System.Collections.Generic;

namespace Yogurt
{
    public static class StorageFactory
    {
        public static void Create<T>() where T : IComponent
        {
            Storage.Create<T>(ComponentID<T>.Value);
        }
    }
        
    internal abstract class Storage
    {
        private static readonly Storage[] all = new Storage[Consts.MAX_COMPONENTS];

        public Stack<Group> Groups = new();

        public abstract IComponent GetBoxed(Entity entity);
        public abstract void ClearEntity(Entity entity);
        protected abstract void Reset();

        public static void Initialize()
        {
            ResetAll();
        }

        public static void Create<T>(ComponentID componentId) where T : IComponent
        {
            all[componentId] ??= new Storage<T>();
        }

        public static void ResetAll()
        {
            foreach (Storage storage in all)
            {
                storage?.Reset();
            }
        }

        public static Storage Of(ComponentID componentId) => all[componentId];
    }

    
    internal class Storage<T> : Storage where T : IComponent
    {
        internal const int PageSize = 1 << PageShift; // 4,096 entities per page
        private const int PageShift = 12;
        private const int PageMask = PageSize - 1;

        public static Storage<T> Instance => (Storage<T>)Of(ComponentID<T>.Value);

        private T[][] pages = new T[1][];

        public override IComponent GetBoxed(Entity entity)
        {
            int id = entity;
            return pages[id >> PageShift][id & PageMask];
        }

        public override void ClearEntity(Entity entity)
        {
            int id = entity;
            int pageIndex = id >> PageShift;
            if ((uint)pageIndex >= (uint)pages.Length)
                return;

            T[] page = pages[pageIndex];
            if (page != null)
            {
                page[id & PageMask] = default;
            }
        }

        protected override void Reset()
        {
            Groups.Clear();

            foreach (T[] page in pages)
            {
                if (page != null)
                {
                    Array.Clear(page, 0, page.Length);
                }
            }
        }

        public void Set(T component, Entity entity)
        {
            int id = entity;
            int pageIndex = id >> PageShift;
            if ((uint)pageIndex >= (uint)pages.Length)
            {
                EnsurePageTable(pageIndex + 1);
            }

            T[] page = pages[pageIndex] ??= new T[PageSize];
            page[id & PageMask] = component;
        }

        public ref T Get(Entity entity)
        {
            int id = entity;
            return ref pages[id >> PageShift][id & PageMask];
        }

        private void EnsurePageTable(int requiredLength)
        {
            int newLength = pages.Length;
            while (newLength < requiredLength)
            {
                newLength <<= 1;
            }

            Array.Resize(ref pages, newLength);
        }
    }
}