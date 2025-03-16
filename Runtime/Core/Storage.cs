using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Yogurt
{
    internal abstract class Storage
    {
        protected static Storage[] All;

        public Stack<Group> Groups = new();
        public abstract IComponent[] ComponentsArray { get; }

        public static void Initialize()
        {
            All = new Storage[Consts.INITIAL_COMPONENTS_COUNT];
            
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (!type.GetInterfaces().Contains(typeof(IComponent)) || type.IsGenericType) 
                        continue;
                    Type genericStorage = typeof(Storage<>).MakeGenericType(type);
                    Activator.CreateInstance(genericStorage);
                }
            }
        }

        public abstract void Set(IComponent component, Entity entity);

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
        
        public Storage()
        {
            ComponentID id = ComponentID.Of<T>();
            All[id] = this;
        }

        public override void Set(IComponent component, Entity entity)
        {
            if (entity >= components.Length)
            {
                Array.Resize(ref components, entity + entity);
            }
            
            components[entity] = (T) component;
        }

        public ref T Get(Entity entity)
        {
            return ref components[entity];
        }
    }
}