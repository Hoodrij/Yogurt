using System.Linq;
using NUnit.Framework;

namespace Yogurt.Tests
{
    internal class Test_Query
    {
        [Test]
        public void Basic()
        {
            int count = 100;
            for (int i = 0; i < count; i++)
            {
                Entity.Create()
                    .Add(new AnyComponent());
            }
            
            Assert.IsTrue(Query.Of<AnyComponent>().Count() == count);
            Assert.IsTrue(Query.Single<AnyAspect>().Exist());
        }
    }
}