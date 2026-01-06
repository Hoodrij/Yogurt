using System;
using System.Collections.Generic;

namespace Yogurt
{
    public static partial class QueryEx
    {
        public static int Count(this QueryOfEntity query, Func<Entity, bool> predicate)
        {
            using HashSet<Entity>.Enumerator enumerator = query.GetEnumerator();
            int count = 0;
            while (enumerator.MoveNext())
            {
                if (predicate(enumerator.Current))
                    count++;
            }
            return count;
        }
        
        public static int Count(this QueryOfEntity query)
        {
            using HashSet<Entity>.Enumerator enumerator = query.GetEnumerator();
            int count = 0;
            while (enumerator.MoveNext())
            {
                count++;
            }
            return count;
        }

        public static int Count<TAspect>(this QueryOfAspect<TAspect> query, Func<TAspect, bool> predicate) where TAspect : struct, IAspect
        {
            AspectsEnumerator<TAspect> enumerator = query.GetEnumerator();
            int count = 0;
            while (enumerator.MoveNext())
            {
                if (predicate(enumerator.Current))
                    count++;
            }
            return count;
        }
        
        public static int Count<TAspect>(this QueryOfAspect<TAspect> query) where TAspect : struct, IAspect
        {
            return query.GetEnumerator().Count;
        }
    }
}