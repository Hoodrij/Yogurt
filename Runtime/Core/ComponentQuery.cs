namespace Yogurt
{
    internal static class ComponentQuery<TComponent> where TComponent : IComponent
    {
        internal static readonly QueryNode Node = QueryNode.Root.With(ComponentID<TComponent>.Value);
    }
}
