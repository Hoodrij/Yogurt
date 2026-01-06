using System;
using System.Diagnostics;
using System.Linq;

namespace Yogurt
{
    [DebuggerDisplay("{Name}")]
    [DebuggerTypeProxy(typeof(EntityDebugView))]
    public unsafe partial struct Entity : IComparable<Entity>, IUnmanaged<Entity>
    {
        public static readonly Entity Null = default;
        
        public int ID;
        internal int Age;

        public bool Exist
        {
            get
            {
#if UNITY_2019_1_OR_NEWER
                if (!UnityEngine.Application.isPlaying) 
                    return false;
#endif
                if (this == Null) 
                    return false;
                EntityMeta* meta = Meta;
                return meta->IsAlive && meta->Age == Age;
            }
        }
        
        internal EntityMeta* Meta => WorldFacade.GetMeta(this);

        public static Entity Create()
        {
            return World.CreateEntity();
        }

        public override int GetHashCode() => ((ID << 5) + ID) ^ Age;
        public override string ToString() => Name;
        public int CompareTo(Entity other) => ID.CompareTo(other.ID);
        public bool Equals(Entity other) => ID == other.ID && Age == other.Age;
        public override bool Equals(object obj) => obj is Entity other && Equals(other);
        public static implicit operator int(Entity entity) => entity.ID;
        public static bool operator ==(Entity entity1, Entity entity2) => entity1.ID == entity2.ID && entity1.Age == entity2.Age;
        public static bool operator !=(Entity entity1, Entity entity2) => !(entity1 == entity2);

        void IUnmanaged<Entity>.Initialize() => this = default;
        void IDisposable.Dispose() => Kill();

        private string Name
        {
            get
            {
                if (this == Null)
                    return $"{nameof(Entity)}.{nameof(Null)}";
                string components = string.Join(", ", this.GetComponents().Select(c => $"{c.GetType().Name}").ToArray());
                return $"{(Meta->IsAlive ? "" : "[DEAD]")} {nameof(Entity)}_{ID} [{components}]";
            }
        }
    }
}