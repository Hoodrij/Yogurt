using System.Runtime.InteropServices;

namespace Yogurt
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct EntityMeta : IUnmanaged<EntityMeta>
    {
        internal bool IsAlive;
        internal int Id;
        internal int Age;
        internal Mask ComponentsMask;

        internal UnsafeSpan<GroupId> Groups;
        
        internal UnsafeSpan<Entity> Childs;
        internal Entity Parent;

        public void Initialize()
        {
            Clear();
            Groups = new(4);
            Childs = new(4);
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