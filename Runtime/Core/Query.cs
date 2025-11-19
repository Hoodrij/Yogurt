using System.Collections;
using System.Collections.Generic;

namespace Yogurt
{
    /// <summary>
    /// QueryOfEntity query = Query.Of Player>();
    /// Entity entity = Query.Of Player>().Single();
    /// Player player = Query.Single Player>();
    ///
    /// PlayerAspect playerAspect = Query.Single PlayerAspect>();
    /// PlayerAspect playerAspect1 = Query.Of PlayerAspect>().Single();
    /// </summary>
    public interface Query
    {
        static QueryOfEntity Of<TComponent>() where TComponent : IComponent
        {
            return new QueryOfEntity().With<TComponent>();
        }

        static TComponent Single<TComponent>() where TComponent : IComponent
        {
            return Of<TComponent>().Single().Get<TComponent>();
        }

        static QueryOfAspect<TAspect> Of<TAspect>(Void _ = default) where TAspect : struct, IAspect
        {
            return AspectCache.Get<TAspect>();
        }

        static TAspect Single<TAspect>(Void _ = default) where TAspect : struct, IAspect
        {
            return Of<TAspect>().Single();
        }
    }

    public ref struct QueryOfEntity
    {
        internal Mask Included;
        internal Mask Excluded;

        public QueryOfEntity With<TComponent>() where TComponent : IComponent
        {
            Included.Set(ComponentID.Of<TComponent>());
            return this;
        }
        
        public QueryOfEntity Without<TComponent>() where TComponent : IComponent
        {
            Excluded.Set(ComponentID.Of<TComponent>());
            return this;
        }

        internal readonly Group GetGroup()
        {
            Composition composition = new Composition(Included, Excluded);
            Group group = Group.GetGroup(composition);
            return group;
        }

        public readonly EntitiesEnumerator GetEnumerator() => GetGroup().GetEntities();
        public readonly Entity Single() => GetGroup().Single();
    }
    
    public ref struct QueryOfAspect<TAspect> where TAspect : struct, IAspect
    {
        internal Mask Included;
        internal Mask Excluded;
        
        public QueryOfAspect<TAspect> With<TComponent>() where TComponent : IComponent
        {
            Included.Set(ComponentID.Of<TComponent>());
            return this;
        }
        
        public QueryOfAspect<TAspect> Without<TComponent>() where TComponent : IComponent
        {
            Excluded.Set(ComponentID.Of<TComponent>());
            return this;
        }
        
        internal readonly Group GetGroup()
        {
            Composition composition = new Composition(Included, Excluded);
            Group group = Group.GetGroup(composition);
            return group;
        }
        
        public readonly AspectsEnumerator<TAspect> GetEnumerator() => GetGroup().GetAspects<TAspect>();
        public readonly TAspect Single() => GetGroup().Single().As<TAspect>();
    }
}