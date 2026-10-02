using System.Collections.Generic;

namespace Yogurt
{
    internal sealed unsafe class PostProcessor
    {
        private enum OperationKind : byte
        {
            ComponentsChanged,
            Kill,
        }

        private readonly struct EntityOperation
        {
            internal readonly Entity Entity;
            internal readonly OperationKind Kind;

            internal EntityOperation(Entity entity, OperationKind kind)
            {
                Entity = entity;
                Kind = kind;
            }
        }

        private readonly Queue<EntityOperation> operations = new();

        private long visit;

        public void EnqueueComponentChange(Entity entity, EntityMeta* meta, ComponentID componentId)
        {
            if (!meta->HasPendingChanges)
            {
                meta->HasPendingChanges = true;
                operations.Enqueue(new EntityOperation(entity, OperationKind.ComponentsChanged));
            }

            Mask.Set(EntityMeta.PendingComponents(meta), componentId);
        }

        public void EnqueueKill(Entity entity)
        {
            operations.Enqueue(new EntityOperation(entity, OperationKind.Kill));
        }

        public void Clear()
        {
            while (operations.Count > 0)
            {
                Entity entity = operations.Dequeue().Entity;
                EntityMeta* meta = entity.Meta;
                if (meta->Age == entity.Age)
                    EntityMeta.ClearPending(meta);
            }
        }

        public void Update()
        {
            if (operations.Count == 0)
                return;

            while (operations.Count > 0)
            {
                EntityOperation operation = operations.Dequeue();
                Entity entity = operation.Entity;
                EntityMeta* meta = entity.Meta;
                if (meta->Age != entity.Age)
                    continue;

                if (operation.Kind == OperationKind.Kill)
                {
                    ProcessKill(entity, meta);
                    continue;
                }

                if (!meta->IsAlive)
                {
                    EntityMeta.ClearPending(meta);
                    continue;
                }

                ProcessComponentsChanged(entity, meta, ++visit);
            }
        }

        private static void ProcessComponentsChanged(Entity entity, EntityMeta* meta, long visit)
        {
            meta->HasPendingChanges = false;
            ulong* pending = EntityMeta.PendingComponents(meta);
            int words = Mask.Words;
            for (int i = 0; i < words; i++)
            {
                ulong word = pending[i];
                if (word == 0)
                    continue;

                pending[i] = 0;
                do
                {
                    Storage storage = Storage.Of((ushort)((i << 6) + Mask.TrailingZeroCount(word)));
                    word &= word - 1;

                    Group[] groups = storage.Groups;
                    int count = storage.GroupsCount;
                    for (int g = 0; g < count; g++)
                    {
                        groups[g].ProcessChange(entity, meta, visit);
                    }
                } while (word != 0);
            }
        }

        private static void ProcessKill(Entity entity, EntityMeta* meta)
        {
            ulong* components = EntityMeta.Components(meta);
            int words = Mask.Words;
            for (int i = 0; i < words; i++)
            {
                ulong word = components[i];
                if (word == 0)
                    continue;

                components[i] = 0;
                do
                {
                    Storage.Of((ushort)((i << 6) + Mask.TrailingZeroCount(word))).ClearEntity(entity);
                    word &= word - 1;
                } while (word != 0);
            }

            for (int i = 0; i < meta->Groups.Count; i++)
            {
                Groups.Get(*meta->Groups[i]).TryRemove(entity);
            }

            EntityMeta.Release(meta);
            WorldFacade.RemoveEntity(entity);
        }
    }
}
