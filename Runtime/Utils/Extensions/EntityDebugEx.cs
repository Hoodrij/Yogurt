using System;
using System.Collections.Generic;

namespace Yogurt
{
    public static class EntityDebugEx
    {
        internal static unsafe List<IComponent> GetComponents(this Entity entity)
        {
            List<IComponent> result = new();
            EntityMeta* meta = entity.Meta;

            Span<ComponentID> buffer = stackalloc ComponentID[Consts.MAX_COMPONENTS];
            int count = meta->ComponentsMask.GetIDs(buffer);

            for (int i = 0; i < count; i++)
            {
                ComponentID componentId = buffer[i];
                Storage storage = Storage.Of(componentId);
                result.Add(storage.ComponentsArray[entity]);
            }

            return result;
        }

        private static bool DebugCheckNull(this Entity entity)
        {
#if UNITY_EDITOR && YOGURT_DEBUG
            if (entity == Entity.Null)
            {
                UnityEngine.Debug.LogError($"Entity is Null");
                return true;
            }
#endif
            return false;
        }

        internal static void DebugCheckAlive(this Entity entity)
        {
#if UNITY_EDITOR && YOGURT_DEBUG
            if (DebugCheckNull(entity)) return;
            if (!entity.Exist)
            {
                UnityEngine.Debug.LogError($"{entity} does not Exist");
            }
#endif
        }

        internal static unsafe void DebugNoComponent<T>(this Entity entity) where T : IComponent
        {
#if UNITY_EDITOR && YOGURT_DEBUG
            bool entityHasComponent = entity.Meta->ComponentsMask.Has(ComponentID.Of<T>());
            if (!entityHasComponent)
            {
                UnityEngine.Debug.LogError($"{entity} does not have [{typeof(T).Name}]");
            }
#endif
        }

        internal static unsafe void DebugAlreadyHave<T>(this Entity entity) where T : IComponent
        {
#if UNITY_EDITOR && YOGURT_DEBUG
            bool entityHasComponent = entity.Meta->ComponentsMask.Has(ComponentID.Of<T>());
            if (entityHasComponent)
            {
                UnityEngine.Debug.LogError($"{entity} already have [{typeof(T).Name}]");
            }
#endif
        }
        
        internal static void DebugParentToSelf(this Entity entity, Entity parent)
        {
#if UNITY_EDITOR && YOGURT_DEBUG
            if (entity == parent)
            {
                UnityEngine.Debug.LogError($"{entity} trying parent self");
            }
#endif
        }
    }
}