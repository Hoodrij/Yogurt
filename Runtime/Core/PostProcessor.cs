using System.Collections.Generic;

namespace Yogurt
{
    internal class PostProcessor
    {
        private struct EntityOperation
        {
            public Entity Entity;
            public ComponentID ComponentId;
            public Action Action;
        }
        internal enum Action : byte
        {
            ComponentsChanged,
            Kill,
        }
        
        private Queue<EntityOperation> operations = new();

        internal void Enqueue(Action action, Entity entity, ComponentID componentId = default)
        {
            operations.Enqueue(new EntityOperation
            {
                Entity = entity,
                ComponentId = componentId,
                Action = action
            });
        }

        internal void Clear() => operations.Clear();

        internal unsafe void Update()
        {
            while (operations.Count > 0)
            {
                EntityOperation operation = operations.Dequeue();
                Entity entity = operation.Entity;
                
                switch (operation.Action)
                {
                    case Action.ComponentsChanged:
                        {
                            if (!entity.Exist) continue;
                            
                            EntityMeta* meta = entity.Meta;
                            
                            Stack<Group> groups = Storage.Of(operation.ComponentId).Groups;
                            foreach (Group group in groups)
                            {
                                group.ProcessEntity(entity, meta);
                            }
                        }
                        break;
                    case Action.Kill:
                        {
                            EntityMeta* meta = entity.Meta;
                            
                            for (int i = 0; i < meta->Groups.Count; i++)
                            {
                                Group.Cache.TryGetValue(*meta->Groups[i], out Group group);
                                group?.TryRemove(entity);
                            }

                            meta->Clear();
                            WorldBridge.RemoveEntity(entity);
                        }
                        break;
                }
            }
        }
    }
}