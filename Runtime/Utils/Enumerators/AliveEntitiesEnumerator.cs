namespace Yogurt
{
    internal unsafe struct AliveEntitiesEnumerator
    {
        private readonly World world;
        private int index;

        internal AliveEntitiesEnumerator(World world)
        {
            this.world = world;
            index = 0;
        }

        public AliveEntitiesEnumerator GetEnumerator() => this;

        public Entity Current
        {
            get
            {
                EntityMeta* meta = world.EntitiesMetas.Peek(index);
                return new Entity { ID = meta->Id, Age = meta->Age };
            }
        }

        public bool MoveNext()
        {
            while (++index < world.EntitiesMetas.Count)
            {
                if (world.EntitiesMetas.Peek(index)->IsAlive)
                    return true;
            }

            return false;
        }
    }
}
