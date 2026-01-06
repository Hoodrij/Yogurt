using System;
using System.Collections.Generic;
using System.Linq;
using Yogurt.Utils;

namespace Yogurt
{
    internal class Group
    {
        internal static Dictionary<Composition, Group> Cache = new(CompositionEqualityComparer.Instance);

        private HashSet<Entity> entities = new(Consts.INITIAL_ENTITIES_COUNT, comparer: EntityEqualityComparer.Instance);
        private readonly Composition composition;

        public static Group GetGroup(Composition composition)
        {
            return Cache.TryGetValue(composition, out Group group) 
                ? group 
                : new Group(composition);
        }

        private Group(Composition composition)
        {
            this.composition = composition;
            Cache.Add(composition, this);

            Span<ComponentID> buffer = stackalloc ComponentID[Consts.MAX_COMPONENTS];
            int count = composition.GetIds(buffer);
            for (int i = 0; i < count; i++)
            {
                ComponentID componentID = buffer[i];
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
        public AspectsEnumerator<TAspect> GetAspects<TAspect>() where TAspect : struct, IAspect
        {
            WorldFacade.UpdateWorld();
            return new AspectsEnumerator<TAspect>(entities);
        }

        public IEnumerable<Entity> AsEnumerable() => entities.AsEnumerable();
    }
}