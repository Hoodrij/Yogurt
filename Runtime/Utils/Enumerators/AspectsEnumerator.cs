using System.Collections.Generic;

namespace Yogurt
{
    public struct AspectsEnumerator<TAspect> where TAspect : struct, IAspect
    {
        public int Count { get; }
        
        private HashSet<Entity>.Enumerator enumerator;

        public AspectsEnumerator(HashSet<Entity> entities)
        {
            Count = entities.Count;
            enumerator = entities.GetEnumerator();
        }

        public TAspect Current => enumerator.Current.As<TAspect>();
        public bool MoveNext() => enumerator.MoveNext();
    }
}