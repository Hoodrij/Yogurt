using System;
using System.Collections.Generic;

namespace Yogurt
{
    internal sealed class Group
    {
        private Entity[] dense = new Entity[Consts.INITIAL_ENTITIES_COUNT];
        private int[] sparse = new int[Consts.INITIAL_ENTITIES_COUNT]; // 0 = absent, otherwise denseIndex + 1
        private int count;
        private readonly Composition composition;
        private readonly Mask components;
        private readonly GroupId Id;

        internal Group(GroupId id, Composition composition)
        {
            Id = id;
            this.composition = composition;

            components = composition.Components;
            while (components.TryPopFirst(out ComponentID componentId))
            {
                Storage.Of(componentId).Groups.Push(this);
            }
        }

        internal unsafe void ProcessChange(Entity entity, EntityMeta* meta, in Mask remainingChanges)
        {
            // Other changed dependencies will reach this group again. Process it only on the last one.
            if (!components.HasAny(remainingChanges))
                ProcessEntity(entity, meta);
        }

        public void Dispose()
        {
            Array.Clear(dense, 0, count);
            Array.Clear(sparse, 0, sparse.Length);
            count = 0;
        }

        public Entity Single()
        {
            WorldFacade.UpdateWorld();
            return count > 0 ? dense[0] : Entity.Null;
        }

        internal unsafe void ProcessEntity(Entity entity, EntityMeta* meta)
        {
            if (composition.Fits(meta))
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

        private bool TryAdd(Entity entity)
        {
            EnsureSparseSize(entity.ID);
            if (sparse[entity.ID] != 0)
                return false;

            if (count == dense.Length)
                Array.Resize(ref dense, dense.Length * 2);

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

            int newSize = sparse.Length;
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

        public IEnumerable<Entity> AsEnumerable()
        {
            WorldFacade.UpdateWorld();
            for (int i = 0; i < count; i++)
            {
                yield return dense[i];
            }
        }
    }
}
