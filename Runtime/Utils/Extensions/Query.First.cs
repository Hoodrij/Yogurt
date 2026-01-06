using System;
using System.Collections.Generic;

namespace Yogurt
{
    public static partial class QueryEx
    {
        public static Entity First(this QueryOfEntity query)
        {
            using HashSet<Entity>.Enumerator enumerator = query.GetEnumerator();
            return enumerator.MoveNext() 
                ? enumerator.Current 
                : default;
        }
        
        public static Entity First(this QueryOfEntity query, Predicate<Entity> predicate)
        {
            using HashSet<Entity>.Enumerator enumerator = query.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (predicate(enumerator.Current))
                    return enumerator.Current;
            }
            return default;
        }
        
        public static TAspect First<TAspect>(this QueryOfAspect<TAspect> query) where TAspect : struct, IAspect
        {
            AspectsEnumerator<TAspect> enumerator = query.GetEnumerator();
            return enumerator.MoveNext() 
                ? enumerator.Current 
                : default;
        }
        
        public static TAspect First<TAspect>(this QueryOfAspect<TAspect> query, Predicate<TAspect> predicate) where TAspect : struct, IAspect
        {
            AspectsEnumerator<TAspect> enumerator = query.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (predicate(enumerator.Current))
                    return enumerator.Current;
            }
            return default;
        }
    }
}