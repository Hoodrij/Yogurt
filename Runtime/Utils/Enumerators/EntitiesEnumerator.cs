using System.Collections.Generic;

namespace Yogurt
{
    public ref struct EntitiesEnumerator
    {
        public int Count { get; }
        private int index;
        
        private EntitiesSparseSet entities;

        public EntitiesEnumerator(EntitiesSparseSet entities)
        {
            this.entities = entities;
            Count = entities.Count;
            Current = default;
            index = 0;
        }

        public EntitiesEnumerator GetEnumerator() => this;
        public Entity Current { get; private set; }

        public bool MoveNext()
        {
            if (index >= Count)
                return false;

            Current = entities.Dense[index].Value;
            index++;
            return true;
        }
    }
}