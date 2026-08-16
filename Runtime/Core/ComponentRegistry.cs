using System;

namespace Yogurt
{
    public static class ComponentRegistry
    {
        public static void Register<T>() where T : IComponent
        {
            GetOrRegister<T>();
        }

        internal static ComponentID GetOrRegister<T>() where T : IComponent
        {
            ComponentID componentId = ComponentID.GetOrCreate(typeof(T));
            Storage.Register<T>(componentId);
            return componentId;
        }

        internal static ComponentID GetOrRegister(Type type)
        {
            ComponentID componentId = ComponentID.GetOrCreate(type);
            if (Storage.Of(componentId) != null)
            {
                return componentId;
            }

            Type storageType = typeof(Storage<>).MakeGenericType(type);
            Storage.Register(componentId, (Storage)Activator.CreateInstance(storageType));
            return componentId;
        }
    }
}