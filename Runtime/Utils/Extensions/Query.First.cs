namespace Yogurt
{
    public static partial class QueryEx
    {
        public static Entity First(this QueryOfEntity query)
        {
            EntitiesEnumerator enumerator = query.GetEnumerator();
            return enumerator.MoveNext() 
                ? enumerator.Current 
                : default;
        }
        
        public static TAspect First<TAspect>(this QueryOfAspect<TAspect> query) where TAspect : struct, IAspect
        {
            AspectsEnumerator<TAspect> enumerator = query.GetEnumerator();
            return enumerator.MoveNext() 
                ? enumerator.Current 
                : default;
        }
    }
}