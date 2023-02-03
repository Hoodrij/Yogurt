using System.Collections.Generic;

namespace Yogurt
{
    public static class EntityDebugEx
    {
        internal static unsafe List<IComponent> GetComponents(this Entity entity)
        {
            List<IComponent> result = new();
            EntityMeta* meta = entity.Meta;
            foreach (ComponentID componentId in meta->ComponentsMask.GetBytes())
            {
                Storage storage = Storage.All[componentId];
                result.Add(storage.ComponentsArray[entity]);
            }

            return result;
        }

        public static TAspect ToAspect<TAspect>(this Entity entity) where TAspect : struct, Aspect<TAspect>
        {
            return new TAspect { Entity = entity };
        }
        
        public static Entity FromAspect(this Entity entity, Aspect aspect)
        {
            return aspect.Entity;
        }

        private static bool DebugCheckNull(this Entity entity)
        {
#if UNITY_EDITOR && FLOWJOB_DEBUG
            if (entity == Entity.Null)
            {
                Debug.LogError($"Entity is Null");
                return true;
            }
#endif
            return false;
        }

        internal static void DebugCheckAlive(this Entity entity)
        {
#if UNITY_EDITOR && FLOWJOB_DEBUG
            if (DebugCheckNull(entity)) return;
            if (!entity.Exist)
            {
                Debug.LogError($"{entity} does not Exist");
            }
#endif
        }

        internal static unsafe void DebugNoComponent<T>(this Entity entity) where T : IComponent
        {
#if UNITY_EDITOR && FLOWJOB_DEBUG
            bool entityHasComponent = entity.Meta->ComponentsMask.Has(ComponentID.Of<T>());
            if (!entityHasComponent)
            {
                Debug.LogError($"{entity} does not have [{typeof(T).Name}]");
            }
#endif
        }

        internal static unsafe void DebugAlreadyHave<T>(this Entity entity) where T : IComponent
        {
#if UNITY_EDITOR && FLOWJOB_DEBUG
            bool entityHasComponent = entity.Meta->ComponentsMask.Has(ComponentID.Of<T>());
            if (entityHasComponent)
            {
                Debug.LogError($"{entity} already have [{typeof(T).Name}]");
            }
#endif
        }
        
        internal static void DebugParentToSelf(this Entity entity, Entity parent)
        {
#if UNITY_EDITOR && FLOWJOB_DEBUG
            if (entity == parent)
            {
                Debug.LogError($"{entity} trying parent self");
            }
#endif
        }
    }
}