using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Yogurt.Unity
{
    public static class Wait
    {
        public static UniTask While(Func<bool> predicate)
        {
            return While(predicate, App.Life);
        }

        public static UniTask While(Func<bool> predicate, Life life)
        {
            return While(static p => p(), predicate, life);
        }

        public static UniTask Until(Func<bool> predicate)
        {
            return Until(predicate, App.Life);
        }

        public static UniTask Until(Func<bool> predicate, Life life)
        {
            return While(static p => !p(), predicate, life);
        }

        public static UniTask Seconds(float seconds)
        {
            return Seconds(seconds, App.Life);
        }

        public static UniTask Seconds(float seconds, Life life)
        {
            return While(static endTime => Time.time < endTime, Time.time + seconds, life);
        }

        public static UniTask Update()
        {
            return UniTask.NextFrame(App.Life);
        }

        private static async UniTask While<T>(Func<T, bool> predicate, T state, Life life)
        {
            while (life && predicate(state))
            {
                await UniTask.NextFrame(App.Life);
            }

            if (!life)
            {
                throw new OperationCanceledException();
            }
        }
    }
}
