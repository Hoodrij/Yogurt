using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;

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
        public static implicit operator CancellationToken(Life life) => life.AsToken();
        public static implicit operator Life(CancellationToken token) => token.AsLife();
        public static implicit operator bool(Life life) => life.IsAlive();
        public static Life operator & (Life a, Life b) => a.And(b);
        public static Life operator | (Life a, Life b) => a.Or(b);
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
        
        public static CancellationToken AsToken(this Life life)
            => LifePool.GetToken(life);
    
        public static Life AsLife(this CancellationToken token) 
            => new Life().SetParent(token.WaitUntilCanceled().AsTask());

        public static Life Or(this Life a, Life b) 
            => LifePool.Or(a, b);

        public static Life And(this Life a, Life b) 
            => LifePool.And(a, b);

        public static Life SetParent(this Life life, UniTask parent) 
            => LifePool.SetParent(life, parent);

        public static UniTask.Awaiter GetAwaiter(this Life life)
            => LifePool.GetAwaiter(life);
    }
}