using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Yogurt
{
    [DebuggerDisplay("{Name}")]
    internal readonly struct ComponentID
    {
        private static Dictionary<Type, ComponentID> ComponentsIds = new();

        private readonly byte ID;

        private ComponentID(byte id)
        {
            ID = id;
        }
        
        public static ComponentID Of(Type type)
        {
            if (ComponentsIds.TryGetValue(type, out ComponentID componentId))
            {
                return componentId;
            }

            componentId = new ComponentID((byte)ComponentsIds.Values.Count);
            ComponentsIds.Add(type, componentId);
            return componentId;
        }
        
        public static ComponentID Of<T>() where T : IComponent => Of(typeof(T));
        
        public static implicit operator byte(ComponentID id)
        {
            return id.ID;
        }

        public static implicit operator ComponentID(byte id)
        {
            return new ComponentID(id);
        }
        
        internal string Name
        {
            get
            {
                foreach ((Type key, ComponentID value) in ComponentsIds)
                {
                    if (value == ID) return key.Name;
                }

                return "None";
            }
        }
    }
}