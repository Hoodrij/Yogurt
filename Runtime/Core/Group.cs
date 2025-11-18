using System;
using System.Collections.Generic;
using System.Linq;

namespace Yogurt
{
    internal class Group
    {
        internal static Dictionary<Composition, Group> Cache = new();

        private HashSet<Entity> entities = new(Consts.INITIAL_ENTITIES_COUNT);
        private readonly Composition composition;

        public static Group GetGroup(Composition composition)
        {
            if (Cache.TryGetValue(composition, out Group group))
            {
                return group;
            }

            return new Group(composition);
        }

        private Group(Composition composition)
        {
            this.composition = composition;
            Cache.Add(composition, this);

            foreach (ComponentID componentID in composition.GetIds())
            {
                Storage.Of(componentID).Groups.Push(this);
            }
            
            foreach (Entity entity in WorldFacade.GetEntities())
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
            WorldFacade.UpdateWorld();
            if (entities.Count <= 0) 
                return Entity.Null;
            
            HashSet<Entity>.Enumerator enumerator = entities.GetEnumerator();
            enumerator.MoveNext();
            return enumerator.Current;
        }

        internal unsafe void ProcessEntity(Entity entity, EntityMeta* meta)
        {
            if (composition.Fits(meta))
            {
                if (TryAdd(entity))
                {
                    meta->Groups.Add(composition);
                }
            }
            else
            {
                if (TryRemove(entity))
                {
                    meta->Groups.Remove(composition);
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

        public EntitiesEnumerator GetEntities()
        {
            WorldFacade.UpdateWorld();
            return new EntitiesEnumerator(entities);
        }
        public AspectsEnumerator<TAspect> GetAspects<TAspect>() where TAspect : struct, IAspect<TAspect>
        {
            WorldFacade.UpdateWorld();
            return new AspectsEnumerator<TAspect>(entities);
        }

        public IEnumerable<Entity> AsEnumerable() => entities.AsEnumerable();
    }
}