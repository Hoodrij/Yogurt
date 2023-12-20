using System.Collections.Generic;
using NUnit.Framework;

namespace Yogurt.Tests
{
    internal class Test_Entity_Components
    {
        [Test]
        public void HasComponent()
        {
            Entity entity = Entity.Create();
            entity.Add(new AnyComponent());
            
            Assert.IsTrue(entity.Has<AnyComponent>());
        }
        
        [Test]
        public void RemoveComponent()
        {
            Entity entity = Entity.Create();
            entity.Add(new AnyComponent());
            entity.Add(new AnyComponent2());
            
            Assert.IsTrue(entity.Has<AnyComponent>());
            entity.Remove<AnyComponent>();
            Assert.IsTrue(!entity.Has<AnyComponent>());
        }
        
        [Test]
        public void MultipleAdd()
        {
            Entity entity = Entity.Create();
            foreach (IComponent component in GetComponents())
            {
                entity.Add(component);
            }

            Assert.IsTrue(entity.GetComponents().Count == 2);
            
            IEnumerable<IComponent> GetComponents()
            {
                yield return new AnyComponent();
                yield return new AnyComponent2();
            }
        }
    }
}