using System;

namespace Yogurt
{
    public static partial class QueryEx
    {
        public static Entity Single(this QueryOfEntity query)
        {
            EntitiesEnumerator enumerator = query.GetEnumerator();
            return enumerator.MoveNext() 
                ? enumerator.Current 
                : default;
        }
        
        public static Entity Single(this QueryOfEntity query, Predicate<Entity> predicate)
        {
            EntitiesEnumerator enumerator = query.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (predicate(enumerator.Current))
                    return enumerator.Current;
            }
            return default;
        }
        
        public static TAspect Single<TAspect>(this QueryOfAspect<TAspect> query) where TAspect : struct, IAspect
        {
            AspectsEnumerator<TAspect> enumerator = query.GetEnumerator();
            return enumerator.MoveNext() 
                ? enumerator.Current 
                : default;
        }
        
        public static TAspect Single<TAspect>(this QueryOfAspect<TAspect> query, Predicate<TAspect> predicate) where TAspect : struct, IAspect
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