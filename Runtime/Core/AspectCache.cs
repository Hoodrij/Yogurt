using System;
using System.Reflection;

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

        internal static Mask GenerateMask(Type aspectType)
        {
            Mask mask = default;
            foreach (PropertyInfo field in aspectType.GetProperties())
            {
                Type propertyType = field.PropertyType;
                if (propertyType.IsByRef)
                {
                    propertyType = propertyType.GetElementType();
                }
                if (propertyType.GetInterface(nameof(IComponent)) != null)
                {
                    mask.Set(ComponentID.Of(propertyType));
                }

                if (propertyType.GetInterface(nameof(IAspect)) != null)
                {
                    Mask nestedMask = GenerateMask(propertyType);
                    mask = mask.Or(nestedMask);
                }
            }

            return mask;
        }
    }

    internal static class AspectCache<TAspect> where TAspect : struct, IAspect
    {
        public static readonly Mask IncludedMask = AspectCache.GenerateMask(typeof(TAspect));

        private static readonly Composition composition = new(IncludedMask, default);
        private static Group group;
        private static int version = -1;

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
