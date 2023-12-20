using System.Linq;
using NUnit.Framework;

namespace Yogurt.Tests
{
    public class TestEntity
    {
        [Test]
        public void Test_Creation()
        {
            Entity entity = Entity.Create();
            
            Assert.IsTrue(entity.Exist);
            Assert.IsTrue(entity != Entity.Null);
            Assert.IsTrue(entity != default);
        }
        
        [Test]
        public void Test_Death()
        {
            Entity entity = Entity.Create();
            entity.Kill();
            
            Assert.IsTrue(!entity.Exist);
        }
        
        [Test]
        public void Test_Death_WithoutComponents()
        {
            Entity entity = Entity.Create();
            entity.Add(new AnyComponent());
            entity.Remove<AnyComponent>();
            
            Assert.IsTrue(!entity.Exist);
        }
        
        [Test]
        public void Test_HasComponent()
        {
            Entity entity = Entity.Create();
            entity.Add(new AnyComponent());
            
            Assert.IsTrue(entity.Has<AnyComponent>());
        }
        
        [Test]
        public void Test_RemoveComponent()
        {
            Entity entity = Entity.Create();
            entity.Add(new AnyComponent());
            entity.Add(new AnyComponent2());
            
            Assert.IsTrue(entity.Has<AnyComponent>());
            entity.Remove<AnyComponent>();
            Assert.IsTrue(!entity.Has<AnyComponent>());
        }
    }
}