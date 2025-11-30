using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Yogurt
{
    [DebuggerDisplay("{Name}")]
    internal readonly struct ComponentID : IEquatable<ComponentID>
    {
        private static Dictionary<Type, ComponentID> componentsIds = new(Consts.MAX_COMPONENTS);

        private readonly ushort ID;

        private ComponentID(ushort id)
        {
            ID = id;
        }

        public static ComponentID Of(Type type)
        {
            if (componentsIds.TryGetValue(type, out ComponentID componentId))
            {
                return componentId;
            }

            ushort newId = (ushort)componentsIds.Values.Count;
            if (newId >= Consts.MAX_COMPONENTS)
            {
                throw new InvalidOperationException($"Maximum component types exceeded ({Consts.MAX_COMPONENTS}). Increase MASK_ULONGS in Consts.cs");
            }

            componentId = new ComponentID(newId);
            componentsIds.Add(type, componentId);
            return componentId;
        }

        public static ComponentID Of<T>() where T : IComponent => Of(typeof(T));

        public static implicit operator ushort(ComponentID id)
        {
            return id.ID;
        }

        public static implicit operator ComponentID(ushort id)
        {
            return new ComponentID(id);
        }
        
        internal string Name
        {
            get
            {
                foreach ((Type key, ComponentID value) in componentsIds)
                {
                    if (value == ID) return key.Name;
                }

                return "None";
            }
        }

        public bool Equals(ComponentID other)
        {
            return ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            return obj is ComponentID other && Equals(other);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }
}