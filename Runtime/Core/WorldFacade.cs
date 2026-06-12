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

        public static AliveEntitiesEnumerator GetEntities()
        {
            UpdateWorld();
            return new AliveEntitiesEnumerator(World);
        }

        public static List<Entity> CollectAliveEntities()
        {
            List<Entity> result = new();
            foreach (Entity entity in GetEntities())
            {
                result.Add(entity);
            }

            return result;
        }

        public static unsafe EntityMeta* GetMeta(Entity entity)
        {
            return World.EntitiesMetas.Peek(entity.ID);
        }

        public static void RemoveEntity(Entity entity)
        {
            World.ReleasedEntities.Enqueue(entity);
        }

        public static Life GetLife(Entity entity)
        {
            if (!entity.Exist)
            {
                Life deadLife = new Life();
                deadLife.Kill();
                return deadLife;
            }

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