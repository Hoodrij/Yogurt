namespace Yogurt.Tests
{
    public struct AnyAspect : IAspect
    {
        public Entity Entity { get; set; }

        public AnyComponent comp => this.Get<AnyComponent>();
    }
}