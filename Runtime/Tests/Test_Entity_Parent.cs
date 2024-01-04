using NUnit.Framework;

namespace Yogurt.Tests
{
    internal class Test_Entity_Parent
    {
        [OneTimeTearDown] public void TearDown() => WorldFacade.World?.Dispose();
        
        [Test]
        public unsafe void ParentOverride()
        {
            Entity parent = Entity.Create()
                .Add(new AnyComponent());
            Entity parent2 = Entity.Create()
                .Add(new AnyComponent());

            Entity child = Entity.Create()
                .Add(new AnyComponent());
                
            child.SetParent(parent);
            Assert.IsTrue(child.Meta->Parent == parent);
            
            child.SetParent(parent2);
            Assert.IsTrue(child.Meta->Parent == parent2);
            
            parent.Kill();
            parent2.Kill();
            Assert.IsTrue(!child.Exist);
        }
    }
}