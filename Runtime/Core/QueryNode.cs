using System;

namespace Yogurt
{
    internal sealed class QueryNode
    {
        internal static readonly QueryNode Root = new(Composition.Empty);

        private const int ExcludedFlag = 1 << 16;

        internal readonly Composition Composition;

        private int[] keys = Array.Empty<int>();
        private QueryNode[] children = Array.Empty<QueryNode>();
        private int childCount;

        private Group group;
        private int version = -1;

        private QueryNode(Composition composition)
        {
            Composition = composition;
        }

        internal QueryNode With(ushort componentId) => Child(componentId);
        internal QueryNode Without(ushort componentId) => Child(componentId | ExcludedFlag);

        internal Group GetGroup()
        {
            if (version == World.Version)
                return group;

            group = Groups.GetOrCreate(Composition);
            version = World.Version;
            return group;
        }

        private QueryNode Child(int key)
        {
            for (int i = 0; i < childCount; i++)
            {
                if (keys[i] == key)
                    return children[i];
            }

            return AddChild(key);
        }

        private QueryNode AddChild(int key)
        {
            ushort componentId = (ushort)key;
            QueryNode child = new((key & ExcludedFlag) != 0
                ? Composition.Without(componentId)
                : Composition.With(componentId));

            if (childCount == keys.Length)
            {
                int capacity = Math.Max(2, childCount * 2);
                Array.Resize(ref keys, capacity);
                Array.Resize(ref children, capacity);
            }

            keys[childCount] = key;
            children[childCount] = child;
            childCount++;
            return child;
        }
    }
}
