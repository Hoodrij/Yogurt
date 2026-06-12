namespace Yogurt
{
    public struct AspectsEnumerator<TAspect> where TAspect : struct, IAspect
    {
        private EntityEnumerator enumerator;

        public int Count => enumerator.Count;

        internal AspectsEnumerator(EntityEnumerator entities)
        {
            enumerator = entities;
        }

        public TAspect Current => enumerator.Current.As<TAspect>();
        public bool MoveNext() => enumerator.MoveNext();
    }
}
