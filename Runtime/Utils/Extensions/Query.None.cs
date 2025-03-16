using System;

namespace Yogurt
{
    public static partial class QueryEx
    {
        public static bool None(this QueryOfEntity query, Func<Entity, bool> predicate)
        {
            return !query.Any(predicate);
        }

        public static bool None(this QueryOfEntity query)
        {
            return !query.Any();
        }

        public static bool None<TAspect>(this QueryOfAspect<TAspect> query, Func<TAspect, bool> predicate) where TAspect : struct, IAspect
        {
            return !query.Any(predicate);
        }

        public static bool None<TAspect>(this QueryOfAspect<TAspect> query) where TAspect : struct, IAspect
        {
            return !query.Any();
        }
    }
}