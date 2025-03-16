using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Yogurt
{
    public static class QueryEx
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

        public static int Count(this QueryOfEntity query) 
            => query.GetEnumerator().Count;
        public static bool Any(this QueryOfEntity query) 
            => query.GetEnumerator().MoveNext();
        public static bool None(this QueryOfEntity query, Func<Entity, bool> predicate) 
            => !query.Any(predicate);
        public static bool None(this QueryOfEntity query) 
            => !query.Any();
        
        
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

        public static int Count<TAspect>(this QueryOfAspect<TAspect> query) where TAspect : struct, IAspect 
            => query.GetEnumerator().Count;
        public static bool Any<TAspect>(this QueryOfAspect<TAspect> query) where TAspect : struct, IAspect 
            => query.GetEnumerator().MoveNext();
        public static bool None<TAspect>(this QueryOfAspect<TAspect> query, Func<TAspect, bool> predicate) where TAspect : struct, IAspect 
            => !query.Any(predicate);
        public static bool None<TAspect>(this QueryOfAspect<TAspect> query) where TAspect : struct, IAspect 
            => !query.Any();
    }
}