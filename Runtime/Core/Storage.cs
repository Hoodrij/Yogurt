using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Yogurt
{
    public abstract class Storage
    {
        internal static Storage[] All;

        internal Stack<Group> Groups = new();
        internal abstract IComponent[] ComponentsArray { get; }

        internal static void Initialize()
        {
            All = new Storage[Consts.INITIAL_COMPONENTS_COUNT];
            
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (!type.GetInterfaces().Contains(typeof(IComponent))
                        || type.IsGenericType) continue;
                    Type genericStorage = typeof(Storage<>).MakeGenericType(type);
                    Activator.CreateInstance(genericStorage);
                }
            }
        }
    }

    public class Storage<T> : Storage where T : IComponent
    {
        internal static Storage<T> Instance;

        private T[] Components = new T[Consts.INITIAL_ENTITIES_COUNT / 2];
        internal override IComponent[] ComponentsArray => Components as IComponent[];

        public Storage()
        {
            Instance = this;
            ComponentID id = ComponentID.Of<T>();
            All[id] = this;
        }

        public void Set(T component, int index)
        {
            if (index >= Components.Length)
            {
                Array.Resize(ref Components, index + index / 2);
            }
            
            Components[index] = component;
        }

        public ref T Get(int index)
        {
            return ref Components[index];
        }
    }
}