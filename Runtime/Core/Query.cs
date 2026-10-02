namespace Yogurt
{
    public interface Query
    {
        static QueryOfEntity Of<TComponent>() where TComponent : IComponent
        {
            return new QueryOfEntity { Node = ComponentQuery<TComponent>.Node };
        }

        static ref TComponent Single<TComponent>() where TComponent : IComponent
        {
            return ref ComponentQuery<TComponent>.Node.GetGroup().Single().Get<TComponent>();
        }

        static QueryOfAspect<TAspect> Of<TAspect>(Void _ = default) where TAspect : struct, IAspect
        {
            return new QueryOfAspect<TAspect> { Node = AspectCache<TAspect>.Node };
        }

        static TAspect Single<TAspect>(Void _ = default) where TAspect : struct, IAspect
        {
            return AspectCache<TAspect>.Node.GetGroup().Single().As<TAspect>();
        }
    }

    public struct QueryOfEntity
    {
        internal QueryNode Node;

        public QueryOfEntity With<TComponent>() where TComponent : IComponent
        {
            Node = (Node ?? QueryNode.Root).With(ComponentID<TComponent>.Value);
            return this;
        }

        public QueryOfEntity Without<TComponent>() where TComponent : IComponent
        {
            Node = (Node ?? QueryNode.Root).Without(ComponentID<TComponent>.Value);
            return this;
        }

        internal readonly Group GetGroup() => (Node ?? QueryNode.Root).GetGroup();

        public readonly EntityEnumerator GetEnumerator() => GetGroup().GetEntities();

        public readonly Entity Single() => GetGroup().Single();

        public readonly void Warmup()
        {
            if (WorldFacade.World == null)
                return;

            GetGroup();
        }
    }

    public struct QueryOfAspect<TAspect> where TAspect : struct, IAspect
    {
        internal QueryNode Node;

        public QueryOfAspect<TAspect> With<TComponent>() where TComponent : IComponent
        {
            Node = (Node ?? QueryNode.Root).With(ComponentID<TComponent>.Value);
            return this;
        }

        public QueryOfAspect<TAspect> Without<TComponent>() where TComponent : IComponent
        {
            Node = (Node ?? QueryNode.Root).Without(ComponentID<TComponent>.Value);
            return this;
        }

        internal readonly Group GetGroup() => (Node ?? QueryNode.Root).GetGroup();

        public readonly AspectsEnumerator<TAspect> GetEnumerator() => GetGroup().GetAspects<TAspect>();

        public readonly TAspect Single() => GetGroup().Single().As<TAspect>();

        public readonly void Warmup()
        {
            if (WorldFacade.World == null)
                return;

            GetGroup();
        }
    }
}
