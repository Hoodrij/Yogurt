using System;
using System.Collections.Generic;
using System.Diagnostics;
using Yogurt.Utils;

namespace Yogurt
{
    [DebuggerDisplay("{Name}")]
    internal readonly struct ComponentID : IEquatable<ComponentID>
    {
        private static readonly Dictionary<Type, ComponentID> componentsIds = new(Consts.INITIAL_COMPONENTS_COUNT, comparer: TypeEqualityComparer.Instance);

        private readonly ushort ID;

        private ComponentID(ushort id)
        {
            ID = id;
        }

        // Registering a new type can widen entity masks, which moves entity metadata: resolve IDs before taking an EntityMeta pointer.
        public static ComponentID Of(Type type)
        {
            if (componentsIds.TryGetValue(type, out ComponentID componentId))
            {
                return componentId;
            }

            int newId = componentsIds.Count;
            if (newId > ushort.MaxValue)
            {
                throw new InvalidOperationException($"Maximum component types exceeded ({ushort.MaxValue + 1}).");
            }

            componentId = new ComponentID((ushort)newId);
            componentsIds.Add(type, componentId);
            Storage.EnsureCapacity(newId + 1);
            Mask.EnsureCapacity(newId + 1);
            return componentId;
        }

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

    internal static class ComponentID<T> where T : IComponent
    {
        public static readonly ComponentID Value = ComponentID.Of(typeof(T));
    }
}
