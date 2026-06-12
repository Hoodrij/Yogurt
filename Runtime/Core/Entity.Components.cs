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
            if (!IsAliveFast)
                return this;

            ComponentID componentID = ComponentID<T>.Value;
            Storage<T>.Instance.Set(component, this);

            if (!Meta->ComponentsMask.Has(componentID))
            {
                Meta->ComponentsMask.Set(componentID);
                WorldFacade.Enqueue(PostProcessor.Action.ComponentsChanged, this, componentID);
            }

            return this;
        }

        public ref T Get<T>() where T : IComponent
        {
            this.DebugCheckAlive();
            this.DebugNoComponent<T>();
            if (!IsAliveFast)
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
            if (!IsAliveFast)
                return false;

            return Meta->ComponentsMask.Has(ComponentID<T>.Value);
        }

        public void Remove<T>() where T : IComponent
        {
            this.DebugNoComponent<T>();
            if (!IsAliveFast)
                return;

            ComponentID componentID = ComponentID<T>.Value;
            Meta->ComponentsMask.UnSet(componentID);
            Storage<T>.Instance.ClearEntity(this);

            if (Meta->ComponentsMask.IsEmpty)
                Kill();
            else
                WorldFacade.Enqueue(PostProcessor.Action.ComponentsChanged, this, componentID);
        }

        public void Kill()
        {
            if (!Exist)
                return;

            WorldFacade.Enqueue(PostProcessor.Action.Kill, this);
            WorldFacade.KillLife(this);
            
            EntityMeta* meta = Meta;
            meta->IsAlive = false;
            while (meta->Childs.Count > 0)
            {
                meta->Childs.Get(meta->Childs.Count - 1)->Kill();
            }

            UnParent(meta);
        }
    }
}