using System.Collections.Generic;

namespace Yogurt
{
    public static partial class QueryEx
    {
        public static IEnumerable<Entity> AsEnumerable(this QueryOfEntity query)
        {
            using HashSet<Entity>.Enumerator enumerator = query.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        public static IEnumerable<TAspect> AsEnumerable<TAspect>(this QueryOfAspect<TAspect> query) where TAspect : struct, IAspect
        {
            AspectsEnumerator<TAspect> enumerator = query.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }
    }
}
