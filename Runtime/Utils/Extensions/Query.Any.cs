using System;
using System.Collections.Generic;

namespace Yogurt
{
    public static partial class QueryEx
    {
        public static bool Any(this QueryOfEntity query, Func<Entity, bool> predicate)
        {
            using HashSet<Entity>.Enumerator enumerator = query.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (predicate(enumerator.Current))
                    return true;
            }
            return false;
        }
        
        public static bool Any(this QueryOfEntity query)
        {
            using HashSet<Entity>.Enumerator enumerator = query.GetEnumerator();
            return enumerator.MoveNext();
        }
        
        public static bool Any<TAspect>(this QueryOfAspect<TAspect> query, Func<TAspect, bool> predicate) where TAspect : struct, IAspect
        {
            AspectsEnumerator<TAspect> enumerator = query.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (predicate(enumerator.Current))
                    return true;
            }
            return false;
        }
        
        public static bool Any<TAspect>(this QueryOfAspect<TAspect> query) where TAspect : struct, IAspect
        {
            return query.GetEnumerator().MoveNext();
        }
    }
}