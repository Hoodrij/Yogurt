using System;
using System.Collections.Generic;
using System.Reflection;

namespace Yogurt
{
    public static class AspectCache
    {
        private static Dictionary<Type, Mask> cache = new();
        
        public static QueryOfAspect<TAspect> Get<TAspect>() where TAspect : struct, Aspect<TAspect>
        {
            QueryOfAspect<TAspect> query = new QueryOfAspect<TAspect>();
            Type aspectType = typeof(TAspect);

            if (!cache.TryGetValue(aspectType, out Mask mask))
            {
                mask = GenerateMask(aspectType);
                cache.Add(aspectType, mask);
            }

            query.Included = mask;
            return query;
        }
        
        private static Mask GenerateMask(Type aspectType)
        {
            Mask mask = default;
            foreach (PropertyInfo field in aspectType.GetProperties())
            {
                Type componentType = field.PropertyType;
                if (componentType.GetInterface(nameof(IComponent)) != null)
                {
                    mask.Set(ComponentID.Of(componentType));
                }
                    
                if (componentType.GetInterface(nameof(Aspect)) != null)
                {
                    mask |= GenerateMask(componentType);
                }
            }

            return mask;
        }

        public static void Clear()
        {
            cache.Clear();
        }
    }
}