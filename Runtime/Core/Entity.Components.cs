namespace Yogurt
{
    public unsafe partial struct Entity
    {
        internal EntityMeta* Meta => WorldFacade.GetMeta(this);
        
        public Entity Add<T>(T component) where T : IComponent
        {
            this.DebugAlreadyHave<T>();
            
            Set(component);
            return this;
        }

        public Entity Set<T>(T component) where T : IComponent
        {
            this.DebugCheckAlive();
            
            ComponentID componentID = ComponentID.Of<T>();
            Meta->ComponentsMask.Set(componentID);
            Storage.Of<T>().Set(component, this);
            WorldFacade.Enqueue(PostProcessor.Action.ComponentsChanged, this, componentID);

            return this;
        }

        public ref T Get<T>() where T : IComponent
        {
            this.DebugCheckAlive();
            this.DebugNoComponent<T>();

            return ref Storage.Of<T>().Get(this);
        }
        
        public bool TryGet<T>(out T t) where T : IComponent
        {
            bool has = Has<T>();
            t = default;
            if (has)
            {
                t = Storage.Of<T>().Get(this);
            }

            return has;
        }

        public bool Has<T>() where T : IComponent
        {
            this.DebugCheckAlive();

            return Meta->ComponentsMask.Has(ComponentID.Of<T>());
        }

        public void Remove<T>() where T : IComponent
        {
            this.DebugNoComponent<T>();

            ComponentID componentID = ComponentID.Of<T>();
            Meta->ComponentsMask.UnSet(componentID);

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