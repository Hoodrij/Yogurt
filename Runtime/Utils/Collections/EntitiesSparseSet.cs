namespace Yogurt
{
    internal class Item
    {
        public int Key;
        public Entity Value;
    }
    
    public class EntitiesSparseSet
    {
        public int Capacity => sparse.Length;
        public int Count => count;
        
        internal readonly Item[] Dense;
        private readonly int[] sparse;
        private int count;

        public EntitiesSparseSet(int size)
        {
            Dense = new Item[size];
            sparse = new int[size];
            for (int i = 0; i < size; i++)
            {
                Dense[i] = new Item();
            }
        }
        
        public void Clear()
        {
            count = 0;
        }

        public bool Contains(Entity entity)
        {
            int key = entity;
            return key > 0 && key < sparse.Length && TryGetIndex(entity, out _);
        }

        public void Add(Entity entity)
        {
            if (TryGetIndex(entity, out int index))
            {
                Dense[index].Value = entity;
                return;
            }
            
            Dense[count].Key = entity;
            Dense[count].Value = entity;
            sparse[entity] = count;
            count++;
        }

        public void Remove(Entity entity)
        {
            int key = entity;
            if (!TryGetIndex(entity, out int index)) return;
            Item last = Dense[count - 1];
            Dense[index].Key = last.Key;
            Dense[index].Value = last.Value;
            sparse[last.Key] = sparse[key];
            count--;
        }

        private bool TryGetIndex(Entity entity, out int index)
        {
            int key = entity;
            index = sparse[key];
            return index < count && Dense[index].Key == key;
        }
    }
}