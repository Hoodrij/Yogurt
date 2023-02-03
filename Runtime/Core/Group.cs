using System;
using System.Collections.Generic;
using System.Linq;

namespace Yogurt
{
    internal class Group : IEquatable<Group>, IComparable<Group>
    {
        internal static Dictionary<HashCode, Group> Cache = new();

        private HashSet<Entity> entities = new(Consts.INITIAL_ENTITIES_COUNT);
        private readonly Composition composition;

        public static Group GetGroup(Composition composition)
        {
            if (Cache.TryGetValue(composition.Hash, out Group group))
            {
                return group;
            }

            return new Group(composition);
        }

        private Group(Composition composition)
        {
            this.composition = composition;
            Cache.Add(composition.Hash, this);

            foreach (ComponentID componentID in composition.GetIds())
            {
                Storage.All[componentID].Groups.Push(this);
            }
            
            foreach (Entity entity in WorldBridge.GetEntities())
            {
                unsafe
                {
                    ProcessEntity(entity, entity.Meta);
                }
            }
        }
        
        public void Dispose()
        {
            entities.Clear();
        }
        
        public Entity Single()
        {
            WorldBridge.UpdateWorld();
            if (entities.Count > 0)
            {
                return entities.First();
            }

            return Entity.Null;
        }

        internal unsafe void ProcessEntity(in Entity entity, EntityMeta* meta)
        {
            if (composition.Fits(meta))
            {
                if (TryAdd(entity))
                {
                    meta->Groups.Add(this);
                }
            }
            else
            {
                if (TryRemove(entity))
                {
                    meta->Groups.Remove(this);
                }
            }
        }

        private bool TryAdd(Entity entity)
        {
            return entities.Add(entity);
        }

        internal bool TryRemove(Entity entity)
        {
            return entities.Remove(entity);
        }

        public bool Equals(Group other)
        {
            return GetHashCode() == other?.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && Equals((Group) obj);
        }

        public override int GetHashCode()
        {
            return composition.Hash;
        }

        public IEnumerator<Entity> GetEnumerator()
        {
            WorldBridge.UpdateWorld();
            return entities.GetEnumerator();
        }

        public int CompareTo(Group other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return GetHashCode().CompareTo(other.GetHashCode());
        }
    }
    
    internal struct GroupId : IUnmanaged<GroupId>
    {
        public int Id;
            
        public void Initialize() { }
        public void Dispose() { }

        public bool Equals(GroupId other)
        {
            return Id == other.Id;
        }
        
        public static implicit operator GroupId(Group group)
        {
            return new GroupId { Id = group.GetHashCode() };
        }
    }
}