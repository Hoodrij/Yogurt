using NUnit.Framework;

namespace Yogurt.Tests
{
    public class TestUnsafeSpan
    {
        [Test]
        public unsafe void Test_Add()
        {
            UnsafeSpan<Entity> span = new UnsafeSpan<Entity>();
            span.Initialize(4);
            
            Entity entity1 = Entity.Create();
            span.Add(entity1);
            
            Assert.IsTrue(entity1 == *span[0]);
        }
    }
}