namespace Yogurt
{
    public static partial class QueryEx
    {
        public static EntitiesEnumerator AsEnumerable(this QueryOfEntity query)
        {
            return query.GetGroup().GetEntities();
        }

        public static AspectsEnumerator<TAspect> AsEnumerable<TAspect>(this QueryOfAspect<TAspect> query) where TAspect : struct, IAspect
        {
            return query.GetGroup().GetAspects<TAspect>();
        }
    }
}
