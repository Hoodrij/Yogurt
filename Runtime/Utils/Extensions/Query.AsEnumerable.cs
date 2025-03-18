using System.Collections.Generic;
using System.Linq;

namespace Yogurt
{
    public static partial class QueryEx
    {
        // public static IEnumerable<Entity> AsEnumerable(this QueryOfEntity query)
        // {
        //     foreach (Entity entity in query.GetGroup().GetEntities().GetEnumerator())
        //     {
        //         yield return entity;
        //     }
        // }
        //
        // public static IEnumerable<TAspect> AsEnumerable<TAspect>(this QueryOfAspect<TAspect> query) where TAspect : struct, IAspect
        // {
        //     return query.GetGroup().AsEnumerable().Select(entity => entity.As<TAspect>());
        // }
    }
}