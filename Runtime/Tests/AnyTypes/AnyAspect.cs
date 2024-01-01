namespace Yogurt.Tests
{
    internal struct AnyAspect : IAspect
    {
        public Entity Entity { get; set; }

        public AnyComponent comp => this.Get<AnyComponent>();
    }
}