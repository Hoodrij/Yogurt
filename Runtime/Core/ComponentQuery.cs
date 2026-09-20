namespace Yogurt
{
    internal static class ComponentQuery<TComponent> where TComponent : IComponent
    {
        private static readonly Composition composition = BuildComposition();
        private static Group group;
        private static int version = -1;

        public static QueryOfEntity Get()
        {
            QueryOfEntity query = default;
            query.Included.Set(ComponentID<TComponent>.Value);
            query.CachedGroup = TryGetGroup();
            query.CachedVersion = World.Version;
            return query;
        }

        /// <summary>Fast path for Query.Single: straight to the cached group, no query struct.</summary>
        public static Entity Single()
        {
            return GetOrCreateGroup().Single();
        }

        private static Composition BuildComposition()
        {
            Mask mask = default;
            mask.Set(ComponentID<TComponent>.Value);
            return new Composition(mask, default);
        }

        private static Group TryGetGroup()
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

        private static Group GetOrCreateGroup()
        {
            if (version == World.Version)
                return group;

            group = Groups.GetOrCreate(composition);
            version = World.Version;
            return group;
        }
    }
}
