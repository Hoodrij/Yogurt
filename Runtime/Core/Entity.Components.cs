namespace Yogurt
{
    // Component IDs are resolved before the meta pointer is taken: registering a new type can move the metadata.
    public unsafe partial struct Entity
    {
        public Entity Add<T>(T component) where T : IComponent
        {
            this.DebugAlreadyHave<T>();

            Set(component);
            return this;
        }

        public Entity Set<T>(T component) where T : IComponent
        {
            this.DebugCheckAlive();

            Storage<T> storage = Storage<T>.Instance;
            EntityMeta* meta = Meta;
            if (!IsAlive(meta))
                return this;

            storage.Set(component, this);

            ComponentID componentID = storage.ID;
            ulong* components = EntityMeta.Components(meta);
            if (!Mask.Has(components, componentID))
            {
                Mask.Set(components, componentID);
                meta->ComponentCount++;
                WorldFacade.EnqueueComponentChange(this, meta, componentID);
            }

            return this;
        }

        public ref T Get<T>() where T : IComponent
        {
            this.DebugCheckAlive();
            this.DebugNoComponent<T>();

            return ref Storage<T>.Instance.Get(this);
        }

        public bool TryGet<T>(out T t) where T : IComponent
        {
            bool has = Has<T>();
            t = default;
            if (has)
            {
                t = Storage<T>.Instance.Get(this);
            }

            return has;
        }

        public bool Has<T>() where T : IComponent
        {
            this.DebugCheckAlive();

            ComponentID componentID = ComponentID<T>.Value;
            EntityMeta* meta = Meta;
            if (!IsAlive(meta))
                return false;

            return Mask.Has(EntityMeta.Components(meta), componentID);
        }

        public void Remove<T>() where T : IComponent
        {
            this.DebugNoComponent<T>();

            Storage<T> storage = Storage<T>.Instance;
            EntityMeta* meta = Meta;
            if (!IsAlive(meta))
                return;

            ComponentID componentID = storage.ID;
            ulong* components = EntityMeta.Components(meta);
            bool had = Mask.Has(components, componentID);
            if (had)
            {
                Mask.UnSet(components, componentID);
                meta->ComponentCount--;
                storage.ClearEntity(this);
            }

            if (meta->ComponentCount == 0)
                Kill();
            else if (had)
                WorldFacade.EnqueueComponentChange(this, meta, componentID);
        }

        public void Kill()
        {
#if UNITY_EDITOR
            if (!UnityEngine.Application.isPlaying)
                return;
#endif
            EntityMeta* meta = Meta;
            if (!IsAlive(meta))
                return;

            WorldFacade.EnqueueKill(this);
            WorldFacade.KillLife(this);

            // Life callbacks can create entities or register types, which moves metadata.
            meta = Meta;
            meta->IsAlive = false;
            while (meta->Childs.Count > 0)
            {
                meta->Childs.Get(meta->Childs.Count - 1)->Kill();
                meta = Meta;
            }

            UnParent(meta);
        }
    }
}
