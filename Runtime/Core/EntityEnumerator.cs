namespace Yogurt
{
    /// <summary>
    /// Allocation-free enumerator over a Group's dense entity array.
    /// Snapshot semantics: the count is captured at creation, so killing entities or
    /// changing components mid-iteration is safe (changes apply on the next flush).
    /// </summary>
    public struct EntityEnumerator
    {
        private readonly Entity[] dense;
        private int index;

        public int Count { get; }

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
