using System;
using System.Collections.Generic;

namespace Yogurt
{
    public static class StorageFactory
    {
        public static void Create<T>() where T : IComponent
        {
            Storage.Create<T>(ComponentID<T>.Value);
        }
    }
        
    internal abstract class Storage
    {
        private static readonly Storage[] all = new Storage[Consts.MAX_COMPONENTS];

        public Stack<Group> Groups = new();

        public abstract IComponent GetBoxed(Entity entity);
        public abstract void ClearEntity(Entity entity);
        protected abstract void Reset();

        public static void Initialize()
        {
            ResetAll();
        }

        public static void Create<T>(ComponentID componentId) where T : IComponent
        {
            all[componentId] ??= new Storage<T>();
        }

        public static void ResetAll()
        {
            foreach (Storage storage in all)
            {
                storage?.Reset();
            }
        }

        public static Storage Of(ComponentID componentId) => all[componentId];
    }

    internal class Storage<T> : Storage where T : IComponent
    {
        public static Storage<T> Instance => (Storage<T>)Of(ComponentID<T>.Value);

        private T[] components = new T[Consts.INITIAL_ENTITIES_COUNT];
        public override IComponent GetBoxed(Entity entity) => components[entity];

        public override void ClearEntity(Entity entity)
        {
            if (entity < components.Length)
            {
                components[entity] = default;
            }
        }

        protected override void Reset()
        {
            Groups.Clear();
            Array.Clear(components, 0, components.Length);
        }

        public void Set(T component, Entity entity)
        {
            AssureSize();
            components[entity] = component;
            return;

            void AssureSize()
            {
                if (entity < components.Length)
                    return;
                int newSize = components.Length;
                while (newSize <= entity)
                {
                    newSize *= 2;
                }

                Array.Resize(ref components, newSize);
            }
        }

        public ref T Get(Entity entity)
        {
            return ref components[entity];
        }
    }
}