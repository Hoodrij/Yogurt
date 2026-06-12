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
            return GroupCache<TComponent>.Get();
        }

        static ref TComponent Single<TComponent>() where TComponent : IComponent
        {
            return ref Of<TComponent>().Single().Get<TComponent>();
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

    public struct QueryOfEntity
    {
        internal Mask Included;
        internal Mask Excluded;
        internal Group CachedGroup;
        internal int CachedVersion;

        public QueryOfEntity With<TComponent>() where TComponent : IComponent
        {
            Included.Set(ComponentID<TComponent>.Value);
            CachedGroup = null;
            return this;
        }

        public QueryOfEntity Without<TComponent>() where TComponent : IComponent
        {
            Excluded.Set(ComponentID<TComponent>.Value);
            CachedGroup = null;
            return this;
        }

        internal readonly Group GetGroup()
        {
            if (CachedGroup != null && CachedVersion == World.Version)
                return CachedGroup;

            return Group.GetGroup(new Composition(Included, Excluded));
        }

        public readonly EntityEnumerator GetEnumerator() => GetGroup().GetEntities();

        public readonly Entity Single() => GetGroup().Single();
    }

    public struct QueryOfAspect<TAspect> where TAspect : struct, IAspect
    {
        internal Mask Included;
        internal Mask Excluded;
        internal Group CachedGroup;
        internal int CachedVersion;

        public QueryOfAspect<TAspect> With<TComponent>() where TComponent : IComponent
        {
            Included.Set(ComponentID<TComponent>.Value);
            CachedGroup = null;
            return this;
        }

        public QueryOfAspect<TAspect> Without<TComponent>() where TComponent : IComponent
        {
            Excluded.Set(ComponentID<TComponent>.Value);
            CachedGroup = null;
            return this;
        }

        internal readonly Group GetGroup()
        {
            if (CachedGroup != null && CachedVersion == World.Version)
                return CachedGroup;

            return Group.GetGroup(new Composition(Included, Excluded));
        }

        public readonly AspectsEnumerator<TAspect> GetEnumerator() => GetGroup().GetAspects<TAspect>();

        public readonly TAspect Single() => GetGroup().Single().As<TAspect>();
    }
}
