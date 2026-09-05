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