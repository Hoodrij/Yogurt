using System;

namespace Yogurt
{
    public unsafe partial struct Entity
    {
        internal EntityMeta* Meta => WorldBridge.GetMeta(ID);
        
        public Entity Add<T>(T component) where T : IComponent
        {
            this.DebugAlreadyHave<T>();
            
            Set(component);
            return this;
        }

        public Entity Set<T>(T component) where T : IComponent
        {
            this.DebugCheckAlive();
            
            ComponentID componentID = ComponentID.Of(component.GetType());
            Meta->ComponentsMask.Set(componentID);
            Storage.Of(componentID).Set(component, this);
            WorldBridge.Enqueue(PostProcessor.Action.ComponentsChanged, this, componentID);

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
                WorldBridge.Enqueue(PostProcessor.Action.ComponentsChanged, this, componentID);
        }

        public void Kill()
        {
            if (!Exist)
                return;

            WorldBridge.Enqueue(PostProcessor.Action.Kill, this);
            
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