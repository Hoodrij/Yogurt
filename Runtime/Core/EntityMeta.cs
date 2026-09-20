using System.Runtime.InteropServices;

namespace Yogurt
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct EntityMeta : IUnmanaged<EntityMeta>
    {
        internal bool IsAlive;
        internal int Id;
        internal int Age;
        internal Mask ComponentsMask;
        internal Mask PendingComponentsMask;

        internal UnsafeSpan<GroupId> Groups;

        internal UnsafeSpan<Entity> Childs;
        internal Entity Parent;
        internal int ParentIndex; // this entity's slot in Parent's Childs; enables O(1) unparent

        public void Initialize()
        {
            IsAlive = false;
            Id = 0;
            Age = 0;

            Groups = default;
            Childs = default;
            Clear();
        }

        public void Dispose()
        {
            Clear();
            Groups.Dispose();
            Childs.Dispose();
        }

        public void Clear()
        {
            Parent = default;
            ParentIndex = 0;
            ComponentsMask.Clear();
            PendingComponentsMask.Clear();
            Groups.Clear();
            Childs.Clear();
        }

        public bool Equals(EntityMeta other)
        {
            return IsAlive == other.IsAlive
                   && Age == other.Age
                   && Id == other.Id;
        }
    }
}