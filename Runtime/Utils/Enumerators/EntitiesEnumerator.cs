using System.Collections.Generic;

namespace Yogurt
{
    public ref struct EntitiesEnumerator
    {
        public int Count { get; }
        
        private HashSet<Entity>.Enumerator enumerator;

        public EntitiesEnumerator(HashSet<Entity> entities)
        {
            Count = entities.Count;
            enumerator = entities.GetEnumerator();
        }

        public EntitiesEnumerator GetEnumerator() => this;
        public Entity Current => enumerator.Current;
        public bool MoveNext() => enumerator.MoveNext();
    }
}