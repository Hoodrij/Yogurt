using NUnit.Framework;

namespace Yogurt.Tests
{
    internal class Test_UnsafeSpan
    {
        [OneTimeTearDown] public void TearDown() => WorldFacade.World?.Dispose();
        
        [Test]
        public unsafe void Add()
        {
            // UnsafeSpan<Entity> span = new UnsafeSpan<Entity>();
            // span.Initialize(4);
            //
            // Entity entity1 = Entity.Create();
            // span.Add(entity1);
            //
            // Assert.IsTrue(entity1 == *span[0]);
        }
    }
}