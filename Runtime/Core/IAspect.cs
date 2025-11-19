namespace Yogurt
{
    public interface IAspect
    {
        Entity Entity { get; set; }
    }

    public static class AspectEx
    {
        // replaced by code generation to get rid of boxing
        
        // public static bool Exist(this Aspect aspect) => aspect.Entity.Exist;
        // public static void Kill(this Aspect aspect) => aspect.Entity.Kill();
        // public static TComponent Get<TComponent>(this Aspect aspect)  where TComponent : IComponent => aspect.Entity.Get<TComponent>();
        // public static bool TryGet<TComponent>(this Aspect aspect, out TComponent component)  where TComponent : IComponent => aspect.Entity.TryGet(out component);
        // public static void Add<TComponent>(this Aspect aspect, TComponent component)  where TComponent : IComponent => aspect.Entity.Set(component);
        // public static void Set<TComponent>(this Aspect aspect, TComponent component)  where TComponent : IComponent => aspect.Entity.Set(component);
        // public static bool Has<TComponent>(this Aspect aspect)  where TComponent : IComponent => aspect.Entity.Has<TComponent>();
        // public static void Remove<TComponent>(this Aspect aspect)  where TComponent : IComponent => aspect.Entity.Remove<TComponent>();
        
        public static Entity SetParent<TAspect>(this Entity entity, TAspect parent) where TAspect : struct, IAspect => entity.SetParent(parent.Entity);
        public static IAspect SetParent<TAspect>(this TAspect aspect, IAspect parent) where TAspect : struct, IAspect => aspect.Entity.SetParent(parent.Entity).As<TAspect>();
        public static TAspect SetParent<TAspect>(this TAspect aspect, Entity parent) where TAspect : struct, IAspect => aspect.Entity.SetParent(parent).As<TAspect>();
        
        public static TOther As<TOther>(this IAspect aspect) where TOther : struct, IAspect => aspect.Entity.As<TOther>();
        public static TAspect As<TAspect>(this Entity entity) where TAspect : struct, IAspect => new() { Entity = entity };
        
        public static string Name<TAspect>(this TAspect aspect) where TAspect : IAspect
        {
            return $"{aspect.GetType()}_{aspect.Entity}";
        }
    }
}