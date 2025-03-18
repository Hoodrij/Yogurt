using System;
using System.Collections.Generic;
using System.Linq;

namespace Yogurt
{
    internal class Group
    {
        internal static Dictionary<Composition, Group> Cache = new();

        private EntitiesSparseSet entities = new(Consts.INITIAL_ENTITIES_COUNT);
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
            if (entities.Contains(entity))
                return false;

            entities.Add(entity);
            return true;
        }

        internal bool TryRemove(Entity entity)
        {
            if (!entities.Contains(entity))
                return false;

            entities.Remove(entity);
            return true;
        }

        public EntitiesEnumerator GetEntities()
        {
            WorldFacade.UpdateWorld();
            return new EntitiesEnumerator(entities);
        }
        public AspectsEnumerator<TAspect> GetAspects<TAspect>() where TAspect : struct, IAspect
        {
            WorldFacade.UpdateWorld();
            return new AspectsEnumerator<TAspect>(GetEntities());
        }

        // public IEnumerable<Entity> AsEnumerable() 
            // => entities.Dense.AsEnumerable().Select(item => item.Value);
    }
}