using System.Collections.Generic;

namespace Yogurt
{
    public ref struct AspectsEnumerator<TAspect> where TAspect : struct, IAspect<TAspect>
    {
        public int Count { get; }
        
        private HashSet<Entity>.Enumerator enumerator;

        public AspectsEnumerator(HashSet<Entity> entities)
        {
            Count = entities.Count;
            enumerator = entities.GetEnumerator();
        }

        public AspectsEnumerator<TAspect> GetEnumerator() => this;
        public TAspect Current => enumerator.Current.As<TAspect>();
        public bool MoveNext() => enumerator.MoveNext();
    }
}