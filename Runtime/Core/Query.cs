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
        public static QueryOfEntity Of<TComponent>() where TComponent : IComponent
        {
            return new QueryOfEntity().With<TComponent>();
        }

        public static ref TComponent Single<TComponent>() where TComponent : IComponent
        {
            return ref new QueryOfEntity().With<TComponent>().Single().Get<TComponent>();
        }
        
        public static QueryOfAspect<TAspect> Of<TAspect>(Void _ = default) where TAspect : struct, Aspect<TAspect>
        {
            return AspectCache.Get<TAspect>();
        }
        
        public static TAspect Single<TAspect>(Void _ = default) where TAspect : struct, Aspect<TAspect>
        {
            return Of<TAspect>().GetGroup().Single().ToAspect<TAspect>();
        }
    }

    public struct QueryOfEntity : Query, IEnumerable<Entity>
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

        private Group GetGroup()
        {
            Composition composition = new Composition(Included, Excluded);
            Group group = Group.GetGroup(composition);
            return group;
        }
        
        public IEnumerator<Entity> GetEnumerator() => GetGroup().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public Entity Single() => GetGroup().Single();
    }
    
    public struct QueryOfAspect<TAspect> : Query, IEnumerable<TAspect> where TAspect : struct, Aspect<TAspect>
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
        
        internal Group GetGroup()
        {
            Composition composition = new Composition(Included, Excluded);
            Group group = Group.GetGroup(composition);
            return group;
        }
        
        public IEnumerator<TAspect> GetEnumerator()
        {
            foreach (Entity entity in GetGroup())
            {
                yield return entity.ToAspect<TAspect>();
            }
        }
        
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public TAspect Single() => GetGroup().Single().ToAspect<TAspect>();
    }
}