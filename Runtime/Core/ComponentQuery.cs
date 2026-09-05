namespace Yogurt
{
    internal static class ComponentQuery<TComponent> where TComponent : IComponent
    {
        public static QueryOfEntity Get()
        {
            QueryOfEntity query = default;
            query.Included.Set(ComponentID<TComponent>.Value);
            query.CachedGroup = GetGroup();
            query.CachedVersion = World.Version;
            return query;
        }
        
        private static readonly Composition composition = BuildComposition();
        private static Group group;
        private static int version = -1;

        private static Composition BuildComposition()
        {
            Mask mask = default;
            mask.Set(ComponentID<TComponent>.Value);
            return new Composition(mask, default);
        }

        private static Group GetGroup()
        {
            if (WorldFacade.World == null)
                return null;

            if (version == World.Version)
                return group;

            if (Groups.TryGet(composition, out Group found))
            {
                group = found;
                version = World.Version;
                return group;
            }

            return null;
        }
    }
}
