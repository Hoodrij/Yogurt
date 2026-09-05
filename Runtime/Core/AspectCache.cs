using System;

namespace Yogurt
{
    public static class AspectCache
    {
        public static QueryOfAspect<TAspect> Get<TAspect>() where TAspect : struct, IAspect
        {
            QueryOfAspect<TAspect> query = default;
            query.Included = AspectCache<TAspect>.IncludedMask;
            query.CachedGroup = AspectCache<TAspect>.TryGetGroup();
            query.CachedVersion = World.Version;
            return query;
        }

        /// <summary>Registers the component mask emitted by the aspect cache generator.</summary>
        public static void Register<TAspect>(QueryOfEntity query) where TAspect : struct, IAspect
        {
            AspectCache<TAspect>.Register(query.Included);
        }
    }

    internal static class AspectCache<TAspect> where TAspect : struct, IAspect
    {
        private static bool registered;
        private static Composition composition;
        private static Group group;
        private static int version = -1;

        public static Mask IncludedMask { get; private set; }

        public static void Register(Mask mask)
        {
            if (registered)
                return;

            IncludedMask = mask;
            composition = new Composition(mask, default);
            registered = true;
        }

        public static Group TryGetGroup()
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
