using System.Collections.Generic;

namespace Yogurt
{
    internal class EntityDebugView
    {
        public int ID => entity.ID;
        public unsafe bool Alive => entity.Meta->IsAlive;
        public int Age => entity.Age;
        public List<IComponent> Components => entity == Entity.Null ? new() : entity.GetComponents();
        public unsafe Entity Parent => entity == Entity.Null ? Entity.Null : entity.Meta->Parent;
        public List<Entity> Childs => entity == Entity.Null ? new() : GetChilds();

        private Entity entity;
            
        public EntityDebugView(Entity entity)
        {
            this.entity = entity;
        }

        private unsafe List<Entity> GetChilds()
        {
            return new UnsafeSpanDebugView<Entity>(entity.Meta->Childs).Items;
        }
    }
}