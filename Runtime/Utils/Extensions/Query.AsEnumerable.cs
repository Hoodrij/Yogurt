using System.Collections.Generic;
using System.Linq;

namespace Yogurt
{
    public static partial class QueryEx
    {
        public static IEnumerable<Entity> AsEnumerable(this QueryOfEntity query)
        {
            return query.GetGroup().AsEnumerable();
        }
        
        public static IEnumerable<TAspect> AsEnumerable<TAspect>(this QueryOfAspect<TAspect> query) where TAspect : struct, IAspect<TAspect>
        {
            return query.GetGroup().AsEnumerable().Select(entity => entity.As<TAspect>());
        }
    }
}