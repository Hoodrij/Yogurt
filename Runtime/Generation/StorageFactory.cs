namespace Yogurt
{
    public static class StorageFactory
    {
        public static void Create<T>() where T : IComponent
        {
            Storage.Create<T>(ComponentID<T>.Value);
        }
    }
}
