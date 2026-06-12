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

            ComponentID componentID = ComponentID.Of<T>();
            Meta->ComponentsMask.Set(componentID);
            Storage.Of<T>(componentID).Set(component, this);
            WorldFacade.Enqueue(PostProcessor.Action.ComponentsChanged, this, componentID);

            return this;
        }

        public ref T Get<T>() where T : IComponent
        {
            this.DebugCheckAlive();
            this.DebugNoComponent<T>();
            if (!IsAliveFast)
                throw new System.InvalidOperationException($"Get<{typeof(T).Name}>() called on a dead or Null entity (ID {ID}, Age {Age}).");

            return ref Storage.Of<T>(ComponentID.Of<T>()).Get(this);
        }

        public bool TryGet<T>(out T t) where T : IComponent
        {
            bool has = Has<T>();
            t = default;
            if (has)
            {
                t = Storage.Of<T>(ComponentID.Of<T>()).Get(this);
            }

            return has;
        }

        public bool Has<T>() where T : IComponent
        {
            this.DebugCheckAlive();
            if (!IsAliveFast)
                return false;

            return Meta->ComponentsMask.Has(ComponentID.Of<T>());
        }

        public void Remove<T>() where T : IComponent
        {
            this.DebugNoComponent<T>();
            if (!IsAliveFast)
                return;

            ComponentID componentID = ComponentID.Of<T>();
            Meta->ComponentsMask.UnSet(componentID);
            Storage.Of<T>(componentID).Clear(this);

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