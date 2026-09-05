using System.Collections.Generic;
using Yogurt.Utils;

namespace Yogurt
{
    internal static class Groups
    {
        private static readonly Dictionary<Composition, Group> compositions = new(CompositionEqualityComparer.Instance);
        private static readonly List<Group> groups = new();

        internal static int Count => compositions.Count;
        internal static Group Get(GroupId id) => groups[id.Value];

        internal static unsafe Group GetOrCreate(Composition composition)
        {
            if (compositions.TryGetValue(composition, out Group group))
                return group;

            group = new Group(new GroupId(groups.Count), composition);
            groups.Add(group);
            compositions.Add(composition, group);

            // Publish both indexes before the flush can process this group's dependencies.
            foreach (Entity entity in WorldFacade.GetEntities())
            {
                group.ProcessEntity(entity, entity.Meta);
            }

            return group;
        }

        internal static bool TryGet(Composition composition, out Group group)
            => compositions.TryGetValue(composition, out group);

        // Evict only the query lookup. Membership IDs remain valid until world disposal.
        internal static bool Remove(Composition composition) => compositions.Remove(composition);

        internal static void Clear()
        {
            foreach (Group group in groups)
            {
                group.Dispose();
            }

            compositions.Clear();
            groups.Clear();
        }
    }
}
