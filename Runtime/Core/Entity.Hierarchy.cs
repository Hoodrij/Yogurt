namespace Yogurt
{
    public unsafe partial struct Entity
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

        private void UnParent(EntityMeta* meta)
        {
            if (meta->Parent == Null) return;
            
            meta->Parent.Meta->Childs.Remove(this);
            meta->Parent = Null;
        }
    }
}