using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Yogurt.Unity
{
    public class EntityLink : MonoBehaviour, IComponent
    {
        public Entity Entity { get; private set; }

        public static implicit operator Entity(EntityLink link)
        {
            return link.Entity;
        }

        internal void Set(Entity entity)
        {
            Entity = entity;
            entity.Set(this);
            DespawnWhenDead(entity).Forget();
        }

        private async UniTaskVoid DespawnWhenDead(Entity entity)
        {
            await entity.Life();

            // A pooled object can be linked to a new entity before the old entity dies.
            if (this != null && Entity == entity)
            {
                Entity = Entity.Null;
                gameObject.Despawn();
            }
        }
    }
}
