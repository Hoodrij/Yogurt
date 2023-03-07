namespace Yogurt
{
    public unsafe partial struct Entity
    {
        public Entity SetParent(Entity parentEntity)
        {
            this.DebugParentToSelf(parentEntity);
            
            Meta->Parent = parentEntity;
            parentEntity.Meta->Childs.Add(this);
            return this;
        }

        public Entity UnParent()
        {
            UnParent(Meta);
            return this;
        }

        private void UnParent(EntityMeta* meta)
        {
            if (meta->Parent == Null) return;
            
            meta->Parent.Meta->Childs.Remove(this);
            meta->Parent = Null;
        }
    }
}