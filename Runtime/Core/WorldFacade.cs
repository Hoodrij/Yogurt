using System.Collections.Generic;

namespace Yogurt
{
    internal static class WorldFacade
    {
        public static World World;
        
        public static void Enqueue(PostProcessor.Action action, Entity entity, ComponentID componentID = default)
        {
            World.PostProcessor.Enqueue(action, entity, componentID);
        }

        public static void UpdateWorld()
        {
            World.PostProcessor.Update();
        }

        public static HashSet<Entity> GetEntities()
        {
            UpdateWorld();
            return World.Entities;
        }

        public static unsafe EntityMeta* GetMeta(Entity entity)
        {
            return World.EntitiesMetas.Get(entity);
        }

        public static void RemoveEntity(Entity entity)
        {
            World.Entities.Remove(entity);
            World.ReleasedEntities.Enqueue(entity);
        }

        public static Life GetLife(Entity entity)
        {
            Dictionary<Entity, Life> lifes = World.Lifes;
            if (lifes.TryGetValue(entity, out Life life))
            {
                return life;
            }
            
            life = new Life();
            lifes.Add(entity, life);
            return life;
        }

        public static void KillLife(Entity entity)
        {
            if (World.Lifes.Remove(entity, out Life life))
            {
                life.Kill();
            }
        }
    }
}