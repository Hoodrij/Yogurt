using System;

namespace Yogurt
{
    public static partial class QueryEx
    {
        public static bool Any(this QueryOfEntity query, Func<Entity, bool> predicate)
        {
            EntitiesEnumerator enumerator = query.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (predicate(enumerator.Current))
                    return true;
            }
            return false;
        }
        
        public static bool Any(this QueryOfEntity query)
        {
            return query.GetEnumerator().MoveNext();
        }
        
        public static bool Any<TAspect>(this QueryOfAspect<TAspect> query, Func<TAspect, bool> predicate) where TAspect : struct, IAspect<TAspect>
        {
            AspectsEnumerator<TAspect> enumerator = query.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (predicate(enumerator.Current))
                    return true;
            }
            return false;
        }
        
        public static bool Any<TAspect>(this QueryOfAspect<TAspect> query) where TAspect : struct, IAspect<TAspect>
        {
            return query.GetEnumerator().MoveNext();
        }
    }
}