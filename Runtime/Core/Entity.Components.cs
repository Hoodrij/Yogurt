namespace Yogurt
{
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

            EntityMeta* meta = Meta;
            if (!IsAlive(meta))
                return this;

            Storage<T>.Instance.Set(component, this);

            ComponentID componentID = ComponentID<T>.Value;
            if (!meta->ComponentsMask.Has(componentID))
            {
                meta->ComponentsMask.Set(componentID);
                WorldFacade.Enqueue(PostProcessor.Action.ComponentsChanged, this, componentID);
            }

            return this;
        }

        public ref T Get<T>() where T : IComponent
        {
            this.DebugCheckAlive();
            this.DebugNoComponent<T>();

            EntityMeta* meta = Meta;
            if (!IsAlive(meta))
                throw new System.InvalidOperationException($"Get<{typeof(T).Name}>() called on a dead or Null entity (ID {ID}, Age {Age}).");

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

            EntityMeta* meta = Meta;
            if (!IsAlive(meta))
                return false;

            return meta->ComponentsMask.Has(ComponentID<T>.Value);
        }

        public void Remove<T>() where T : IComponent
        {
            this.DebugNoComponent<T>();

            EntityMeta* meta = Meta;
            if (!IsAlive(meta))
                return;

            ComponentID componentID = ComponentID<T>.Value;
            meta->ComponentsMask.UnSet(componentID);
            Storage<T>.Instance.ClearEntity(this);

            if (meta->ComponentsMask.IsEmpty)
                Kill();
            else
                WorldFacade.Enqueue(PostProcessor.Action.ComponentsChanged, this, componentID);
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

            WorldFacade.Enqueue(PostProcessor.Action.Kill, this);
            WorldFacade.KillLife(this);

            meta->IsAlive = false;
            while (meta->Childs.Count > 0)
            {
                meta->Childs.Get(meta->Childs.Count - 1)->Kill();
            }

            UnParent(meta);
        }
    }
}
