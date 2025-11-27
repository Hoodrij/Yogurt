using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Yogurt
{
    internal abstract class Storage
    {
        private static Storage[] All;

        public Stack<Group> Groups = new();
        public abstract IComponent[] ComponentsArray { get; }

        public static void Initialize()
        {
            All = new Storage[Consts.INITIAL_COMPONENTS_COUNT];

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsGenericType || !type.GetInterfaces().Contains(typeof(IComponent)))
                        continue;

                    Type genericStorage = typeof(Storage<>).MakeGenericType(type);
                    Storage storage = (Storage)Activator.CreateInstance(genericStorage);
                    All[ComponentID.Of(type)] = storage;
                }
            }
        }

        public static Storage Of(ComponentID componentId)
        {
            return All[componentId];
        }
        
        public static Storage<T> Of<T>() where T : IComponent
        {
            return (Storage<T>) Of(ComponentID.Of<T>());
        }
    }

    internal class Storage<T> : Storage where T : IComponent
    {
        private T[] components = new T[Consts.INITIAL_ENTITIES_COUNT];
        public override IComponent[] ComponentsArray => components as IComponent[];
        
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