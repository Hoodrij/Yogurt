namespace Yogurt
{
    // ReSharper disable once UnusedTypeParameter
    // Actually used by Generator
    public static class AspectCache<TAspect> where TAspect : struct, IAspect
    {
        internal static QueryNode Node { get; private set; } = QueryNode.Root;

        public static void Register(QueryOfEntity query)
        {
            Node = query.Node ?? QueryNode.Root;
        }
    }
}
