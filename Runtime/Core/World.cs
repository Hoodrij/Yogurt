using System.Collections.Generic;
using Yogurt.Utils;

namespace Yogurt
{
    internal unsafe class World
    {
        internal static int Version { get; private set; }

        public PostProcessor PostProcessor = new();
        public UnsafeSpan<EntityMeta> EntitiesMetas = new(Consts.INITIAL_ENTITIES_COUNT);
        public Queue<Entity> ReleasedEntities = new(Consts.INITIAL_ENTITIES_COUNT);
        public Dictionary<Entity, Life> Lifes = new(Consts.INITIAL_ENTITIES_COUNT, comparer: EntityEqualityComparer.Instance);
        
        // 0 index = default = Entity.Null
        private int nextEntityID = 1;

        private World()
        {
            Version++;
            Storage.Initialize();

#if UNITY_2019_1_OR_NEWER
            UnityEngine.Application.quitting += Dispose;
#endif
        }

        public static Entity CreateEntity()
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

            return entity;
        }

        public void Dispose()
        {
            WorldFacade.World = null;
#if UNITY_2019_1_OR_NEWER
            UnityEngine.Application.quitting -= Dispose;
#endif

            ReleasedEntities.Clear();
            PostProcessor.Clear();
            Lifes.Clear();

            foreach (Group group in Group.Cache.Values)
            {
                group.Dispose();
            }
            Group.Cache.Clear();

            Storage.ResetAll();
            EntitiesMetas.Dispose();
        }
    }
}