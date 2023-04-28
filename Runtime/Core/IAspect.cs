namespace Yogurt
{
    public interface IAspect
    {
        public Entity Entity { get; set; }
    }

    public static class AspectEx
    {
        public static bool Exist(this IAspect aspect) => aspect.Entity.Exist;
        public static void Kill(this IAspect aspect) => aspect.Entity.Kill();

        public static ref TComponent Get<TComponent>(this IAspect aspect) where TComponent : IComponent => ref aspect.Entity.Get<TComponent>();
        public static bool TryGet<TComponent>(this IAspect aspect, out TComponent component) where TComponent : IComponent => aspect.Entity.TryGet(out component);
        public static void Add<TComponent>(this IAspect aspect) where TComponent : IComponent, new() => aspect.Entity.Add<TComponent>();
        public static void Add<TComponent>(this IAspect aspect, TComponent component) where TComponent : IComponent => aspect.Entity.Set(component);
        public static void Set<TComponent>(this IAspect aspect, TComponent component) where TComponent : IComponent => aspect.Entity.Set(component);
        public static bool Has<TComponent>(this IAspect aspect) where TComponent : IComponent => aspect.Entity.Has<TComponent>();
        public static void Remove<TComponent>(this IAspect aspect) where TComponent : IComponent => aspect.Entity.Remove<TComponent>();
        public static TAspect Get<TAspect>(this IAspect aspect, Void _ = default) where TAspect : struct, IAspect => aspect.Entity.As<TAspect>();
        public static TOtherAspect As<TOtherAspect>(this IAspect aspect) where TOtherAspect : struct, IAspect => aspect.Entity.As<TOtherAspect>();

        public static string Name(this IAspect aspect)
        {
            return $"{aspect.GetType()}_{aspect.Entity}";
        }
    }
}