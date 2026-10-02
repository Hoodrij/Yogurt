using System.Collections.Generic;

namespace Yogurt
{
    public static class EntityDebugEx
    {
        internal static unsafe List<IComponent> GetComponents(this Entity entity)
        {
            List<IComponent> result = new();
            ulong* components = EntityMeta.Components(entity.Meta);

            for (int i = 0; i < Mask.Words; i++)
            {
                ulong word = components[i];
                while (word != 0)
                {
                    ushort componentId = (ushort)((i << 6) + Mask.TrailingZeroCount(word));
                    word &= word - 1;
                    result.Add(Storage.Of(componentId).GetBoxed(entity));
                }
            }

            return result;
        }

        private static bool DebugCheckNull(this Entity entity)
        {
#if YOGURT_DEBUG
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
#if YOGURT_DEBUG
            if (DebugCheckNull(entity)) return;
            if (!entity.Exist)
            {
                UnityEngine.Debug.LogError($"{entity} does not Exist");
            }
#endif
        }

        internal static unsafe void DebugNoComponent<T>(this Entity entity) where T : IComponent
        {
#if YOGURT_DEBUG
            if (!HasComponent<T>(entity))
            {
                UnityEngine.Debug.LogError($"{entity} does not have [{typeof(T).Name}]");
            }
#endif
        }

        internal static unsafe void DebugAlreadyHave<T>(this Entity entity) where T : IComponent
        {
#if YOGURT_DEBUG
            if (HasComponent<T>(entity))
            {
                UnityEngine.Debug.LogError($"{entity} already have [{typeof(T).Name}]");
            }
#endif
        }

#if YOGURT_DEBUG
        // ID first: registering a new type can move metadata.
        private static unsafe bool HasComponent<T>(Entity entity) where T : IComponent
        {
            ushort componentId = ComponentID<T>.Value;
            return Mask.Has(EntityMeta.Components(entity.Meta), componentId);
        }
#endif
        
        internal static void DebugParentToSelf(this Entity entity, Entity parent)
        {
#if YOGURT_DEBUG
            if (entity == parent)
            {
                UnityEngine.Debug.LogError($"{entity} trying parent self");
            }
#endif
        }
    }
}