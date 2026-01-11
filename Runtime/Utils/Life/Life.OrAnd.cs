using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Pool;

namespace Yogurt
{
    internal class OrCompletionSource : IUniTaskSource
    {
        private static readonly ObjectPool<OrCompletionSource> pool = new(() => new OrCompletionSource());
        
        private UniTaskCompletionSourceCore<AsyncUnit> core;
        private int remaining;

        public UniTask Task => new UniTask(this, core.Version);

        public static OrCompletionSource Create()
        {
            OrCompletionSource src = pool.Get();
            src.core.Reset();
            src.remaining = 2;
            return src;
        }

        public void OnTaskCompleted()
        {
            if (Interlocked.Decrement(ref remaining) == 1)
            {
                core.TrySetResult(AsyncUnit.Default);
            }
            else if (remaining == 0)
            {
                pool.Release(this);
            }
        }

        public UniTaskStatus GetStatus(short token) => core.GetStatus(token);
        public UniTaskStatus UnsafeGetStatus() => core.UnsafeGetStatus();
        public void GetResult(short token) => core.GetResult(token);
        public void OnCompleted(Action<object> continuation, object state, short token) => core.OnCompleted(continuation, state, token);
    }

    internal class AndCompletionSource : IUniTaskSource
    {
        private static readonly ObjectPool<AndCompletionSource> pool = new(() => new AndCompletionSource());

        private UniTaskCompletionSourceCore<AsyncUnit> core;
        private int remaining;

        public UniTask Task => new UniTask(this, core.Version);

        public static AndCompletionSource Create()
        {
            AndCompletionSource src = pool.Get();
            src.core.Reset();
            src.remaining = 2;
            return src;
        }

        public void OnTaskCompleted()
        {
            if (Interlocked.Decrement(ref remaining) == 0)
            {
                core.TrySetResult(AsyncUnit.Default);
                pool.Release(this);
            }
        }

        public UniTaskStatus GetStatus(short token) => core.GetStatus(token);
        public UniTaskStatus UnsafeGetStatus() => core.UnsafeGetStatus();
        public void GetResult(short token) => core.GetResult(token);
        public void OnCompleted(Action<object> continuation, object state, short token) => core.OnCompleted(continuation, state, token);
    }
}