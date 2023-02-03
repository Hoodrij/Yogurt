using System.Collections.Generic;

namespace Yogurt
{
    public unsafe partial struct Entity : IUnmanaged<Entity>
    {
        public void SetParent(Entity parentEntity)
        {
            this.DebugParentToSelf(parentEntity);
            
            Meta->Parent = parentEntity;
            parentEntity.Meta->Childs.Add(this);
        }

        public void UnParent()
        {
            UnParent(Meta);
        }

        internal void UnParent(EntityMeta* meta)
        {
            if (meta->Parent == Null) return;
            
            meta->Parent.Meta->Childs.Remove(this);
            meta->Parent = Null;
        }

        public void Initialize()
        {
            this = default;
        }
        public void Dispose() { }
    }
}