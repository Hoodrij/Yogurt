using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Yogurt
{
    internal class PostProcessor
    {
        private Queue<EntityOperation> operations = new();

        internal void Enqueue(Action action, Entity entity, ComponentID componentId = default)
        {
            operations.Enqueue(new EntityOperation
            {
                entity = entity,
                componentId = componentId,
                action = action
            });
        }

        internal void Clear() => operations.Clear();

        internal unsafe void Update()
        {
            while (operations.Count > 0)
            {
                EntityOperation operation = operations.Dequeue();
                Entity entity = operation.entity;
                EntityMeta* meta = entity.Meta;
                
                switch (operation.action)
                {
                    case Action.ComponentsChanged:
                        {
                            Stack<Group> groups = Storage.All[operation.componentId].Groups;
                            foreach (Group group in groups)
                            {
                                group.ProcessEntity(entity, meta);
                            }
                        }
                        break;
                    case Action.Kill:
                        {
                            for (int i = 0; i < meta->Groups.Count; i++)
                            {
                                Group.Cache.TryGetValue(meta->Groups[i]->Id, out Group group);
                                group?.TryRemove(entity);
                            }

                            meta->Clear();
                            entity.Age += 1;
                            entity.Age %= int.MaxValue;
                            WorldBridge.RemoveEntity(entity);
                        }
                        break;
                }
            }
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
        private struct EntityOperation
        {
            public Entity entity;
            public ComponentID componentId;
            public Action action;
        }

        internal enum Action : byte
        {
            ComponentsChanged,
            Kill,
        }
    }
}