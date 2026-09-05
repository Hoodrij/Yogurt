using System.Collections.Generic;

namespace Yogurt
{
    internal sealed class PostProcessor
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

        public unsafe void EnqueueComponentChange(Entity entity, ComponentID componentId)
        {
            EntityMeta* meta = entity.Meta;
            // A nonempty mask means that this entity already has a queued change.
            if (meta->PendingComponentsMask.IsEmpty)
                operations.Enqueue(new EntityOperation(entity, OperationKind.ComponentsChanged));

            meta->PendingComponentsMask.Set(componentId);
        }

        public void EnqueueKill(Entity entity)
        {
            operations.Enqueue(new EntityOperation(entity, OperationKind.Kill));
        }

        public unsafe void Clear()
        {
            while (operations.Count > 0)
            {
                Entity entity = operations.Dequeue().Entity;
                EntityMeta* meta = entity.Meta;
                if (meta->Age == entity.Age)
                    meta->PendingComponentsMask.Clear();
            }
        }

        public unsafe void Update()
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
                    meta->PendingComponentsMask.Clear();
                    continue;
                }

                Mask changes = meta->PendingComponentsMask;
                meta->PendingComponentsMask.Clear();
                ProcessComponentsChanged(entity, meta, changes);
            }
        }

        private static unsafe void ProcessComponentsChanged(Entity entity, EntityMeta* meta, Mask changes)
        {
            while (changes.TryPopFirst(out ComponentID componentId))
            {
                foreach (Group group in Storage.Of(componentId).Groups)
                {
                    group.ProcessChange(entity, meta, changes);
                }
            }
        }

        private static unsafe void ProcessKill(Entity entity, EntityMeta* meta)
        {
            Mask components = meta->ComponentsMask;
            while (components.TryPopFirst(out ComponentID componentId))
            {
                Storage.Of(componentId).ClearEntity(entity);
            }

            for (int i = 0; i < meta->Groups.Count; i++)
            {
                Groups.Get(*meta->Groups[i]).TryRemove(entity);
            }

            meta->Clear();
            WorldFacade.RemoveEntity(entity);
        }
    }
}
