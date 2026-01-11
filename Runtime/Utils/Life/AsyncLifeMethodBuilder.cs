using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks.CompilerServices;

namespace Yogurt
{
    [StructLayout(LayoutKind.Auto)]
    public struct AsyncLifeMethodBuilder
    {
        private AsyncUniTaskMethodBuilder target;

        private AsyncLifeMethodBuilder(AsyncUniTaskMethodBuilder target)
        {
            this.target = target;
        }

        [DebuggerHidden]
        public Life Task => target.Task;

        public static AsyncLifeMethodBuilder Create() 
            => new (AsyncUniTaskMethodBuilder.Create());

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine 
            => target.Start(ref stateMachine);

        public void SetStateMachine(IAsyncStateMachine stateMachine) 
            => target.SetStateMachine(stateMachine);

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine 
            => target.AwaitOnCompleted(ref awaiter, ref stateMachine);

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine 
            => target.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);

        public void SetResult() 
            => target.SetResult();

        public void SetException(Exception exception) 
            => target.SetException(exception);
    }
}