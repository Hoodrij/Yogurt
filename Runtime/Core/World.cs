﻿using System.Collections.Generic;

namespace Yogurt
{
    internal unsafe class World
    {
        internal PostProcessor PostProcessor = new();
        internal UnsafeSpan<EntityMeta> EntitiesMetas = new(Consts.INITIAL_ENTITIES_COUNT);
        internal HashSet<Entity> Entities = new(Consts.INITIAL_ENTITIES_COUNT);
        internal Queue<Entity> ReleasedEntities = new(Consts.INITIAL_ENTITIES_COUNT);
        
        // because 0 index = default = Entity.Null
        private int nextEntityID = 1;

        private World()
        {
            Storage.Initialize();

#if UNITY_2019_1_OR_NEWER
            UnityEngine.Application.quitting += Dispose;
#endif
        }

        internal static Entity CreateEntity()
        {
            World world = WorldFacade.World ??= new World();
            
            Entity entity;
            if (world.ReleasedEntities.Count > 0)
            {
                entity = world.ReleasedEntities.Dequeue();
                entity.Age += 1;
                entity.Age %= int.MaxValue;
            }
            else
            {
                entity = new()
                {
                    ID = world.nextEntityID++,
                    Age = 0
                };
            }

            EntityMeta* meta = world.EntitiesMetas.Get(entity.ID);
            meta->Id = entity.ID;
            meta->Age = entity.Age;
            meta->IsAlive = true;
            meta->ComponentsMask.Clear();

            world.Entities.Add(entity);

            return entity;
        }

        public void Dispose()
        {
            WorldFacade.World = null;
#if UNITY_2019_1_OR_NEWER
            UnityEngine.Application.quitting -= Dispose;
#endif

            Entities.Clear();
            ReleasedEntities.Clear();
            PostProcessor.Clear();

            foreach (Group group in Group.Cache.Values)
            {
                group.Dispose();
            }
            Group.Cache.Clear();
            
            EntitiesMetas.Dispose();
            AspectCache.Clear();
        }
    }
}