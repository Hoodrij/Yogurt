using System;
using System.Diagnostics;
using System.Linq;

namespace Yogurt
{
    [DebuggerDisplay("{Name}")]
    [DebuggerTypeProxy(typeof(EntityDebugView))]
    public unsafe partial struct Entity : IComparable<Entity>, IEquatable<Entity>
    {
        public static readonly Entity Null = default;
        
        public int ID;
        internal int Age;

        public bool Exist
        {
            get
            {
                if (this == Null) return false;
                EntityMeta* meta = Meta;
                return meta->IsAlive && meta->Age == Age;
            }
        }

        public static Entity Create()
        {
            return World.CreateEntity();
        }

        private Entity(int id)
        {
            ID = id;
            Age = 0;
        }

        public override int GetHashCode()
        {
            return ((ID << 5) + ID) ^ Age;
        }

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(Entity other)
        {
            return ID.CompareTo(other.ID);
        }

        public bool Equals(Entity other)
        {
            return ID == other.ID && Age == other.Age;
        }

        public override bool Equals(object obj)
        {
            return obj is Entity other && Equals(other);
        }

        public static implicit operator int(Entity entity)
        {
            return entity.ID;
        }

        public static implicit operator Entity(int id)
        {
            return new Entity(id);
        }

        public static bool operator ==(Entity entity1, Entity entity2)
        {
            return entity1.ID == entity2.ID && entity1.Age == entity2.Age;
        }

        public static bool operator !=(Entity entity1, Entity entity2)
        {
            return !(entity1 == entity2);
        }

        private string Name
        {
            get
            {
                if (this == Null)
                    return "Entity.Null";
                string components = string.Concat(this.GetComponents().Select(c => $"{c.GetType().Name} ").ToArray());
                return $"{(Meta->IsAlive ? "" : "[DEAD] ")}Entity_{ID} [{components}]";
            }
        }
    }
}