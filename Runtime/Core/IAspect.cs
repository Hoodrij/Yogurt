namespace Yogurt
{
    public interface IAspect<TSelf> where TSelf : IAspect<TSelf>
    {
        Entity Entity { get; set; }
    }

    public static class AspectEx
    {
        public static bool Exist<TAspect>(this TAspect aspect) where TAspect : struct, IAspect<TAspect> 
            => aspect.Entity.Exist;
        public static void Kill<TAspect>(this TAspect aspect) where TAspect : struct, IAspect<TAspect> 
            => aspect.Entity.Kill();
        public static bool TryGet<TAspect, TComponent>(this TAspect aspect, out TComponent component) where TAspect : struct, IAspect<TAspect> where TComponent : IComponent 
            => aspect.Entity.TryGet(out component);
        public static void Add<TAspect, TComponent>(this TAspect aspect, TComponent component) where TAspect : struct, IAspect<TAspect> where TComponent : IComponent 
            => aspect.Entity.Set(component);
        public static void Set<TAspect, TComponent>(this TAspect aspect, TComponent component) where TAspect : struct, IAspect<TAspect> where TComponent : IComponent 
            => aspect.Entity.Set(component);
        public static bool Has<TAspect, TComponent>(this TAspect aspect) where TAspect : struct, IAspect<TAspect> where TComponent : IComponent 
            => aspect.Entity.Has<TComponent>();
        public static void Remove<TAspect, TComponent>(this TAspect aspect) where TAspect : struct, IAspect<TAspect> where TComponent : IComponent 
            => aspect.Entity.Remove<TComponent>();
        public static TOtherAspect SetParent<TAspect, TOtherAspect>(this TAspect aspect, TOtherAspect parent) where TAspect : IAspect<TAspect> where TOtherAspect : struct, IAspect<TOtherAspect> 
            => aspect.Entity.SetParent(parent).As<TOtherAspect>();

        public static string Name<TAspect>(this TAspect aspect) where TAspect : IAspect<TAspect>
        {
            return $"{aspect.GetType()}_{aspect.Entity}";
        }
    }
}