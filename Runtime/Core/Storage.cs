using System;

namespace Yogurt
{
    internal abstract class Storage
    {
        private static Storage[] all = new Storage[Consts.INITIAL_COMPONENTS_COUNT];

        public Group[] Groups = new Group[4];
        public int GroupsCount;

        public abstract IComponent GetBoxed(Entity entity);
        public abstract void ClearEntity(Entity entity);
        protected abstract void Reset();

        public static void Initialize()
        {
            ResetAll();
        }

        public static void Create<T>(ComponentID componentId) where T : IComponent
        {
            Storage<T> storage = (Storage<T>)all[componentId];
            if (storage == null)
            {
                storage = new Storage<T>();
                all[componentId] = storage;
            }

            Storage<T>.Instance = storage;
        }

        public static void EnsureCapacity(int componentCount)
        {
            if (componentCount <= all.Length)
                return;

            int length = all.Length;
            while (length < componentCount)
            {
                length <<= 1;
            }

            Array.Resize(ref all, length);
        }

        public static void ResetAll()
        {
            foreach (Storage storage in all)
            {
                storage?.Reset();
            }
        }

        public static Storage Of(ComponentID componentId) => all[componentId];

        public void AddGroup(Group group)
        {
            if (GroupsCount == Groups.Length)
                Array.Resize(ref Groups, GroupsCount * 2);

            Groups[GroupsCount++] = group;
        }

        protected void ClearGroups()
        {
            Array.Clear(Groups, 0, GroupsCount);
            GroupsCount = 0;
        }
    }


    internal class Storage<T> : Storage where T : IComponent
    {
        // A struct wrapper: storing into Slot[] skips the array covariance check a T[] store pays for class T.
        private struct Slot
        {
            public T Value;
        }

        internal const int PageSize = 1 << PageShift;
        private const int PageShift = 12;
        private const int PageMask = PageSize - 1;

        public static Storage<T> Instance;

        public readonly ComponentID ID = ComponentID<T>.Value;

        private Slot[][] pages = new Slot[1][];

        public override IComponent GetBoxed(Entity entity)
        {
            int id = entity;
            return pages[id >> PageShift][id & PageMask].Value;
        }

        public override void ClearEntity(Entity entity)
        {
            int id = entity;
            int pageIndex = id >> PageShift;
            if ((uint)pageIndex >= (uint)pages.Length)
                return;

            Slot[] page = pages[pageIndex];
            if (page != null)
            {
                page[id & PageMask] = default;
            }
        }

        protected override void Reset()
        {
            ClearGroups();

            foreach (Slot[] page in pages)
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

            Slot[] page = pages[pageIndex] ??= new Slot[PageSize];
            page[id & PageMask].Value = component;
        }

        public ref T Get(Entity entity)
        {
            int id = entity;
            return ref pages[id >> PageShift][id & PageMask].Value;
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
