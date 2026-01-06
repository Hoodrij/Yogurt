using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine.Pool;

namespace Yogurt
{
    [DebuggerDisplay("{Name}")]
    [AsyncMethodBuilder(typeof(AsyncLifeMethodBuilder))]
#if CSHARP_10
    public readonly record struct Life() : IDisposable
#else
    public class Life : IDisposable
#endif
    {
        internal readonly int Id = LifePool.Pop();

        private string Name => $"IsAlive - {this.IsAlive()}";

        void IDisposable.Dispose() => this.Kill();
        public override string ToString() => Name;

        public static implicit operator UniTask(Life life) => life.AsUniTask();
        public static implicit operator Life(UniTask task) => new Life().SetParent(task);
        public static implicit operator CancellationToken(Life life) => LifePool.GetToken(life);
        public static implicit operator Life(CancellationToken token) => token.AsLife();
        public static implicit operator bool(Life life) => life.IsAlive();
        public static Life operator &(Life a, Life b) => a.And(b);
        public static Life operator |(Life a, Life b) => a.Or(b);
    }

    public static class LifeAPI
    {
        public static void Kill(this Life life)
            => LifePool.Kill(life);

        public static bool IsAlive(this Life life)
            => LifePool.IsAlive(life);

        public static bool IsDead(this Life life) 
            => !life;

        public static UniTask AsUniTask(this Life life)
            => LifePool.AsUniTask(life);
    
        public static Life AsLife(this CancellationToken token) 
            => new Life().SetParent(token.WaitUntilCanceled().AsTask());

        public static Life And(this Life a, Life b) 
            => new Life().SetParent(LifePool.All(a, b));

        public static Life Or(this Life a, Life b)
            => new Life().SetParent(LifePool.Any(a, b));

        public static Life SetParent(this Life life, UniTask parent)
        {
            KillWithParent(life, parent).Forget();
            return life;
        
            static async UniTask KillWithParent(Life life, UniTask parent)
            {
                await parent;
                life.Kill();
            }
        }

        public static UniTask.Awaiter GetAwaiter(this Life life)
            => LifePool.GetAwaiter(life);
    }

    public static class LifePool
    {
        private static readonly HashSet<int> activeLifetimes = new(capacity: 31);
        private static readonly Dictionary<int, CancellationTokenSource> tokenSources = new(capacity: 31);
        private static readonly Dictionary<int, List<AutoResetUniTaskCompletionSource>> completionSources = new(capacity: 31);
        private static int nextId = 1;
    
        public static int Pop()
        {
            int id = nextId++;
            activeLifetimes.Add(id);
            return id;
        }

        public static bool IsAlive([CanBeNull] Life life)
        {
#if CSHARP_10_OR_NEWER
            return activeLifetimes.Contains(life.Id);
#else
            return life is not null && activeLifetimes.Contains(life.Id);
#endif
        }

        public static async UniTask AsUniTask(Life life)
        {
            await life;
        }

        public static void Kill(Life life)
        {
            if (life.IsDead()) 
                return;
            int id = life.Id;
            activeLifetimes.Remove(id);

            if (tokenSources.TryGetValue(id, out CancellationTokenSource cts))
            {
                cts.Cancel();
                cts.Dispose();
                tokenSources.Remove(id);
            }

            if (completionSources.Remove(id, out List<AutoResetUniTaskCompletionSource> sources))
            {
                foreach (AutoResetUniTaskCompletionSource source in sources)
                {
                    source.TrySetResult();
                }
                ListPool<AutoResetUniTaskCompletionSource>.Release(sources);
            }
        }

        public static CancellationToken GetToken(Life life)
        {
            if (life.IsDead())
                return CancellationToken.None;
            
            if (tokenSources.TryGetValue(life.Id, out CancellationTokenSource cts))
                return cts.Token;

            cts = new CancellationTokenSource();
            tokenSources[life.Id] = cts;
            return cts.Token;
        }
    
        public static async UniTask AsTask(this CancellationTokenAwaitable awaitable)
        {
            await awaitable;
        }
    
        public static async UniTask All(Life a, Life b)
        {
            AutoResetUniTaskCompletionSource srcA = AutoResetUniTaskCompletionSource.Create();
            AutoResetUniTaskCompletionSource srcB = AutoResetUniTaskCompletionSource.Create();
            Run(a, srcA).Forget();
            Run(b, srcB).Forget();
            await srcA.Task;
            await srcB.Task;
        }

        public static async UniTask Any(Life a, Life b)
        {
            AutoResetUniTaskCompletionSource src = AutoResetUniTaskCompletionSource.Create();
            Run(a, src).Forget();
            Run(b, src).Forget();
            await src.Task;
        }
    
        private static async UniTask Run(Life life, AutoResetUniTaskCompletionSource source)
        {
            await life;
            source.TrySetResult();
        }

        public static UniTask.Awaiter GetAwaiter(Life life)
        {
            if (life.IsDead())
                return UniTask.CompletedTask.GetAwaiter();

            AutoResetUniTaskCompletionSource source = AutoResetUniTaskCompletionSource.Create();

            if (!completionSources.TryGetValue(life.Id, out List<AutoResetUniTaskCompletionSource> sources))
            {
                ListPool<AutoResetUniTaskCompletionSource>.Get(out sources);
                completionSources[life.Id] = sources;
            }

            sources.Add(source);
            return source.Task.GetAwaiter();
        }
    }
}