using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Yogurt
{
    public static partial class QueryEx
    {
        public static int Count(this QueryOfEntity query, Func<Entity, bool> predicate)
        {
            EntitiesEnumerator enumerator = query.GetEnumerator();
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
            return query.GetEnumerator().Count;
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