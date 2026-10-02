using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Yogurt
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct EntityMeta
    {
        internal UnsafeSpan<GroupId> Groups;
        internal UnsafeSpan<Entity> Childs;
        internal Entity Parent;
        internal int ParentIndex;
        internal int Id;
        // Hot fields last: they share a cache line with the component mask that follows the struct.
        internal int Age;
        internal ushort ComponentCount;
        internal bool HasPendingChanges;
        internal bool IsAlive;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ulong* Components(EntityMeta* meta) => (ulong*)(meta + 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ulong* PendingComponents(EntityMeta* meta) => (ulong*)(meta + 1) + Mask.Words;

        internal static void ClearPending(EntityMeta* meta)
        {
            meta->HasPendingChanges = false;
            Mask.Clear(PendingComponents(meta), Mask.Words);
        }

        // Invariant: released slots have all-zero masks, so creation does not clear them. The caller zeroes the
        // component mask; the pending mask is already empty (a dead entity's queued change is cleared before its kill).
        internal static void Release(EntityMeta* meta)
        {
            meta->Parent = default;
            meta->ParentIndex = 0;
            meta->ComponentCount = 0;
            meta->HasPendingChanges = false;
            meta->Groups.Clear();
            meta->Childs.Clear();
        }

        internal void Dispose()
        {
            Groups.Dispose();
            Childs.Dispose();
        }
    }
}
