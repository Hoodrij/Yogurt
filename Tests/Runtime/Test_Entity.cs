using NUnit.Framework;

namespace Yogurt.Tests
{
    internal class Test_Entity
    {
        [OneTimeTearDown] public void TearDown() => WorldBridge.World?.Dispose();
        
        [Test]
        public void Creation()
        {
            Entity entity = Entity.Create();
            
            Assert.IsTrue(entity.Exist);
            Assert.IsTrue(entity != Entity.Null);
            Assert.IsTrue(entity != default);
        }
        
        [Test]
        public void Death()
        {
            Entity entity = Entity.Create();
            entity.Kill();
            
            Assert.IsTrue(!entity.Exist);
        }
        
        [Test]
        public void Death_WithoutComponents()
        {
            Entity entity = Entity.Create();
            entity.Add(new AnyComponent());
            entity.Remove<AnyComponent>();
            
            Assert.IsTrue(!entity.Exist);
        }
    }
}