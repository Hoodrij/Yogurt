using System;
using System.Runtime.InteropServices;

namespace Yogurt
{
    internal sealed unsafe class Group
    {
        private Entity[] dense = new Entity[Consts.INITIAL_ENTITIES_COUNT];
        private int[] sparse = new int[Consts.INITIAL_ENTITIES_COUNT]; // 0 = absent, otherwise denseIndex + 1
        private int count;
        private readonly GroupId Id;

        private struct MaskWord
        {
            public int Index;
            public ulong Bits;
        }

        // Non-zero words only: [included][excluded].
        private MaskWord* words;
        private readonly int includedCount;
        private readonly int excludedCount;

        private long lastVisit;

        internal Group(GroupId id, Composition composition)
        {
            Id = id;

            // Subscribe before allocating: a component without storage throws here, before anything can leak.
            ulong[] included = composition.Included;
            ulong[] excluded = composition.Excluded;
            for (int i = 0; i < Math.Max(included.Length, excluded.Length); i++)
            {
                ulong word = (i < included.Length ? included[i] : 0) | (i < excluded.Length ? excluded[i] : 0);
                while (word != 0)
                {
                    Storage.Of((ushort)((i << 6) + Mask.TrailingZeroCount(word))).AddGroup(this);
                    word &= word - 1;
                }
            }

            includedCount = CountNonZero(included);
            excludedCount = CountNonZero(excluded);
            if (includedCount + excludedCount > 0)
            {
                words = (MaskWord*)Marshal.AllocHGlobal((includedCount + excludedCount) * sizeof(MaskWord));
                CopyNonZero(included, words);
                CopyNonZero(excluded, words + includedCount);
            }
        }

        internal void ProcessChange(Entity entity, EntityMeta* meta, long visit)
        {
            if (lastVisit == visit)
                return;

            lastVisit = visit;
            ProcessEntity(entity, meta);
        }

        // Query nodes keep disposed groups cached until they next resolve: drop the arrays, don't just clear them.
        public void Dispose()
        {
            dense = Array.Empty<Entity>();
            sparse = Array.Empty<int>();
            count = 0;

            if (words != null)
            {
                Marshal.FreeHGlobal((IntPtr)words);
                words = null;
            }
        }

        public Entity Single()
        {
            WorldFacade.UpdateWorld();
            return count > 0 ? dense[0] : Entity.Null;
        }

        internal void ProcessEntity(Entity entity, EntityMeta* meta)
        {
            if (Fits(meta))
            {
                if (TryAdd(entity))
                {
                    meta->Groups.Add(Id);
                }
            }
            else
            {
                if (TryRemove(entity))
                {
                    meta->Groups.Remove(Id);
                }
            }
        }

        private bool Fits(EntityMeta* meta)
        {
            // No bounds check: compositions hold registered IDs only, and entity masks cover every registered ID.
            ulong* components = EntityMeta.Components(meta);
            ulong mismatch = 0;
            for (int i = 0; i < includedCount; i++)
            {
                MaskWord word = words[i];
                mismatch |= word.Bits & ~components[word.Index];
            }

            MaskWord* excluded = words + includedCount;
            for (int i = 0; i < excludedCount; i++)
            {
                MaskWord word = excluded[i];
                mismatch |= word.Bits & components[word.Index];
            }

            return mismatch == 0;
        }

        private static int CountNonZero(ulong[] mask)
        {
            int count = 0;
            foreach (ulong word in mask)
            {
                if (word != 0)
                    count++;
            }

            return count;
        }

        private static void CopyNonZero(ulong[] mask, MaskWord* destination)
        {
            for (int i = 0; i < mask.Length; i++)
            {
                if (mask[i] != 0)
                    *destination++ = new MaskWord { Index = i, Bits = mask[i] };
            }
        }

        private bool TryAdd(Entity entity)
        {
            EnsureSparseSize(entity.ID);
            if (sparse[entity.ID] != 0)
                return false;

            if (count == dense.Length)
                Array.Resize(ref dense, Math.Max(Consts.INITIAL_ENTITIES_COUNT, dense.Length * 2));

            dense[count] = entity;
            sparse[entity.ID] = ++count;
            return true;
        }

        internal bool TryRemove(Entity entity)
        {
            if (entity.ID >= sparse.Length)
                return false;

            int indexPlusOne = sparse[entity.ID];
            if (indexPlusOne == 0)
                return false;

            int index = indexPlusOne - 1;
            int lastIndex = count - 1;
            Entity last = dense[lastIndex];

            dense[index] = last;
            sparse[last.ID] = index + 1;
            sparse[entity.ID] = 0;
            dense[lastIndex] = default;
            count = lastIndex;
            return true;
        }

        private void EnsureSparseSize(int id)
        {
            if (id < sparse.Length)
                return;

            int newSize = Math.Max(Consts.INITIAL_ENTITIES_COUNT, sparse.Length);
            while (newSize <= id)
            {
                newSize *= 2;
            }

            Array.Resize(ref sparse, newSize);
        }

        public EntityEnumerator GetEntities()
        {
            WorldFacade.UpdateWorld();
            return new EntityEnumerator(dense, count);
        }

        public AspectsEnumerator<TAspect> GetAspects<TAspect>() where TAspect : struct, IAspect
        {
            return new AspectsEnumerator<TAspect>(GetEntities());
        }
    }
}
