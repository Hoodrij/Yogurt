using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Yogurt
{
    [DebuggerDisplay("{Name}")]
    internal readonly struct Composition : IEquatable<Composition>, IUnmanaged<Composition>
    {
        private readonly Mask included;
        private readonly Mask excluded;
        internal readonly int Hash;

        public Composition(Mask included, Mask excluded) : this()
        {
            this.included = included;
            this.excluded = excluded;
            Hash = HashCode.Combine(included, excluded);
        }
        
        internal unsafe bool Fits(EntityMeta* meta)
        {
            return meta->ComponentsMask.HasAll(included)
                   && !meta->ComponentsMask.HasAny(excluded);
        }
        
        public IEnumerable<ComponentID> GetIds()
        {
            foreach (byte id in included.And(excluded).GetBytes()) 
                yield return id;
        }

        public override int GetHashCode()
        {
            return Hash;
        }

        public bool Equals(Composition other)
        {
            return included.Equals(other.included)
                && excluded.Equals(other.excluded);
        }

        public void Initialize() { }
        public void Dispose() { }

        public override bool Equals(object obj)
        {
            return obj is Composition other && Equals(other);
        }

        public override string ToString()
        {
            return Name;
        }


        private string Name => included.ToString();
    }
}