using System.Collections.Generic;

namespace Yogurt
{
    public ref struct AspectsEnumerator<TAspect> where TAspect : struct, IAspect
    {
        public int Count { get; }
        
        private EntitiesEnumerator enumerator;

        public AspectsEnumerator(EntitiesEnumerator entities)
        {
            enumerator = entities;
            Count = enumerator.Count;
        }

        public AspectsEnumerator<TAspect> GetEnumerator() => this;
        public TAspect Current => enumerator.Current.As<TAspect>();
        public bool MoveNext() => enumerator.MoveNext();
    }
}