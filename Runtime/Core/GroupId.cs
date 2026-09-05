using System;

namespace Yogurt
{
    internal readonly struct GroupId : IUnmanaged<GroupId>
    {
        internal readonly int Value;

        internal GroupId(int value) => Value = value;

        public bool Equals(GroupId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is GroupId other && Equals(other);
        public override int GetHashCode() => Value;

        void IUnmanaged<GroupId>.Initialize() { }
        void IDisposable.Dispose() { }
    }
}
