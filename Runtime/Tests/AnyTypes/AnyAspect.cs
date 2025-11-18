namespace Yogurt.Tests
{
    internal struct AnyAspect : IAspect<AnyAspect>
    {
        public Entity Entity { get; set; }

        public AnyComponent comp => Entity.Get<AnyComponent>();
    }
}