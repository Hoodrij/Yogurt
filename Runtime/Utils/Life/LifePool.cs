using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine.Pool;

namespace Yogurt
{
    internal static class LifePool
    {
        private static readonly HashSet<int> activeLifes = new(capacity: 31);
        private static readonly Dictionary<int, CancellationTokenSource> tokenSources = new(capacity: 31);
        private static readonly Dictionary<int, List<AutoResetUniTaskCompletionSource>> completionSources = new(capacity: 31);
        private static int nextId = 1;
    
        public static int Pop()
        {
            int id = nextId++;
            activeLifes.Add(id);
            return id;
        }

        public static bool IsAlive([CanBeNull] Life life)
        {
#if CSHARP_10
            return activeLifetimes.Contains(life.Id);
#else
            return life is not null && activeLifes.Contains(life.Id);
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
            activeLifes.Remove(id);

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
        
        public static Life SetParent(Life life, UniTask parent)
        {
            KillWithParent(life, parent).Forget();
            return life;
        
            static async UniTask KillWithParent(Life life, UniTask parent)
            {
                await parent.SuppressCancellationThrow();
                life.Kill();
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

        public static async Life Or(Life a, Life b)
        {
            OrCompletionSource src = OrCompletionSource.Create();
            a.GetAwaiter().SourceOnCompleted(s => ((OrCompletionSource)s).OnTaskCompleted(), src);
            b.GetAwaiter().SourceOnCompleted(s => ((OrCompletionSource)s).OnTaskCompleted(), src);
            await src.Task;
        }

        public static async Life And(Life a, Life b)
        {
            AndCompletionSource src = AndCompletionSource.Create();
            a.GetAwaiter().SourceOnCompleted(s => ((AndCompletionSource)s).OnTaskCompleted(), src);
            b.GetAwaiter().SourceOnCompleted(s => ((AndCompletionSource)s).OnTaskCompleted(), src);
            await src.Task;
        }
    }
}