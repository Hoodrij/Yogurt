namespace Yogurt
{
    public interface IAspect
    {
        Entity Entity { get; set; }
    }

    public static class AspectEx
    {
        public static bool Exist<TAspect>(this TAspect aspect) where TAspect : IAspect => aspect.Entity.Exist;
        public static void Kill<TAspect>(this TAspect aspect) where TAspect : IAspect => aspect.Entity.Kill();
        
        public static Life Life<TAspect>(this TAspect aspect) where TAspect : IAspect => aspect.Entity.Life();

        public static Entity SetParent<TAspect>(this Entity entity, TAspect parent) where TAspect : struct, IAspect => entity.SetParent(parent.Entity);
        public static TAspect SetParent<TAspect>(this TAspect aspect, IAspect parent) where TAspect : struct, IAspect => aspect.Entity.SetParent(parent.Entity).As<TAspect>();
        public static TAspect SetParent<TAspect>(this TAspect aspect, Entity parent) where TAspect : struct, IAspect => aspect.Entity.SetParent(parent).As<TAspect>();
        
        public static TAspect As<TAspect>(this Entity entity) where TAspect : struct, IAspect => new() { Entity = entity };
        
        public static string Name<TAspect>(this TAspect aspect) where TAspect : IAspect
        {
            return $"{aspect.GetType()}_{aspect.Entity}";
        }
    }
}