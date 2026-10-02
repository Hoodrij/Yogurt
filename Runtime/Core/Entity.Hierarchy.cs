namespace Yogurt
{
    public unsafe partial struct Entity
    {
        public Entity SetParent(Entity parentEntity)
        {
            this.DebugParentToSelf(parentEntity);

            EntityMeta* meta = Meta;

            if (meta->Parent.Exist)
            {
                UnParent(meta);
            }

            EntityMeta* parentMeta = parentEntity.Meta;
            meta->Parent = parentEntity;
            meta->ParentIndex = parentMeta->Childs.Count;
            parentMeta->Childs.Add(this);
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

            ref UnsafeSpan<Entity> siblings = ref meta->Parent.Meta->Childs;
            int index = meta->ParentIndex;
            int last = siblings.Count - 1;

            if (index != last)
            {
                siblings[last]->Meta->ParentIndex = index;
            }

            siblings.RemoveAtSwapBack(index);
            meta->Parent = Null;
        }
    }
}
