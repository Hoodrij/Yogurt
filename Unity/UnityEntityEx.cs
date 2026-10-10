using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Yogurt.Unity
{
    public static class UnityEntityEx
    {
        public static Entity Link(this Entity entity, GameObject gameObject)
        {
            if (!gameObject.TryGetComponent(out EntityLink link))
            {
                link = gameObject.AddComponent<EntityLink>();
            }

            link.Set(entity);
            return entity;
        }

        public static Entity GetEntity(this Component component)
        {
            EntityLink link = component.GetComponentInParent<EntityLink>();
            return link != null ? link.Entity : Entity.Null;
        }

        public static Entity PopulateFrom(this Entity entity, IBlueprint blueprint)
        {
            blueprint.Populate(entity);
            return entity;
        }

        public static void Run(this Entity entity, Action action)
        {
            Loop().Forget();
            return;

            async UniTaskVoid Loop()
            {
                while (entity.Exist)
                {
                    action();
                    await Wait.Update();
                }
            }
        }

        public static void Run(this Entity entity, Func<UniTask> action)
        {
            Loop().Forget();
            return;

            async UniTaskVoid Loop()
            {
                while (entity.Exist)
                {
                    await action();
                    await Wait.Update();
                }
            }
        }

        public static void Run<TAspect>(this TAspect aspect, Action action) where TAspect : struct, IAspect
        {
            aspect.Entity.Run(action);
        }

        public static void Run<TAspect>(this TAspect aspect, Func<UniTask> action) where TAspect : struct, IAspect
        {
            aspect.Entity.Run(action);
        }
    }
}
