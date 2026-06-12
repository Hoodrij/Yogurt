using System;
using System.Collections.Generic;
using System.Reflection;

namespace Yogurt
{
    internal abstract class Storage
    {
        private static Storage[] All;

        public Stack<Group> Groups = new();

        public abstract IComponent GetBoxed(Entity entity);
        public abstract void ClearEntity(Entity entity);
        protected abstract void Reset();

        public static void Initialize()
        {
            if (All != null)
            {
                ResetAll();
                return;
            }

            All = new Storage[Consts.MAX_COMPONENTS];

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsGenericType || !typeof(IComponent).IsAssignableFrom(type))
                        continue;

                    Type genericStorage = typeof(Storage<>).MakeGenericType(type);
                    Storage storage = (Storage)Activator.CreateInstance(genericStorage);
                    All[ComponentID.Of(type)] = storage;
                }
            }
        }

        public static void ResetAll()
        {
            if (All == null)
                return;

            foreach (Storage storage in All)
            {
                storage?.Reset();
            }
        }

        public static Storage Of(ComponentID componentId) => All[componentId];
    }

    internal class Storage<T> : Storage where T : IComponent
    {
        private static Storage<T> instance;
        public static Storage<T> Instance => instance ??= (Storage<T>)Of(ComponentID<T>.Value);

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