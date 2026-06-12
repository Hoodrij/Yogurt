namespace Yogurt
{
    internal static class GroupCache<TComponent> where TComponent : IComponent
    {
        public static QueryOfEntity Get()
        {
            QueryOfEntity query = default;
            query.Included.Set(ComponentID<TComponent>.Value);
            query.CachedGroup = TryGet();
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

        public static Group TryGet()
        {
            if (WorldFacade.World == null)
                return null;

            if (version == World.Version)
                return group;

            if (Group.Cache.TryGetValue(composition, out Group found))
            {
                group = found;
                version = World.Version;
                return group;
            }

            return null;
        }
    }
}