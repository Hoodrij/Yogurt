using System;
using System.Collections.Generic;

namespace Yogurt.Utils
{
    internal sealed class CompositionEqualityComparer : IEqualityComparer<Composition>
    {
        public static readonly CompositionEqualityComparer Instance = new();
        public bool Equals(Composition x, Composition y) => x.Equals(y);
        public int GetHashCode(Composition obj) => obj.GetHashCode();
    }
    
    internal sealed class EntityEqualityComparer : IEqualityComparer<Entity>
    {
        public static readonly EntityEqualityComparer Instance = new();
        public bool Equals(Entity x, Entity y) => x.Equals(y);
        public int GetHashCode(Entity obj) => obj.GetHashCode();
    }
    
    internal sealed class TypeEqualityComparer : IEqualityComparer<Type>
    {
        public static readonly TypeEqualityComparer Instance = new();
        public bool Equals(Type x, Type y) => x == y;
        public int GetHashCode(Type obj) => obj.GetHashCode();
    }
}