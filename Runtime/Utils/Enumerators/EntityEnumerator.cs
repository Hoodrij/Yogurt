namespace Yogurt
{
    public struct EntityEnumerator
    {
        public readonly int Count;

        private readonly Entity[] dense;
        private int index;

        internal EntityEnumerator(Entity[] dense, int count)
        {
            this.dense = dense;
            Count = count;
            index = -1;
        }

        public Entity Current => dense[index];

        public bool MoveNext() => ++index < Count;
    }
}
