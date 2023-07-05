using System;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace Ximmerse.XR.Asyncoroutine
{
    /// <summary>
    /// Sub thread awaiter, used for thread-no-safe awaiter.
    /// </summary>
    public class UnitySubthreadAwaiter : INotifyCompletion
    {
        protected Action action = null;

        internal bool isCompleted = false;

        /// <summary>
        /// Waits for target time-ticks
        /// </summary>
        internal long targetTicks;

        void INotifyCompletion.OnCompleted(Action continuation)
        {
            this.action = continuation;
            isCompleted = false;
        }

        internal void Complete()
        {
            isCompleted = true;
            try
            {
                action?.Invoke();
            }
            catch (Exception exc)
            {
                Debug.LogException(exc);
            }
        }

        public bool IsCompleted
        {
            get => isCompleted;
        }

        public System.Object GetResult()
        {
            return this;
        }

        public void Reset()
        {
            //action = null;
            isCompleted = false;
            this.targetTicks = 0;
        }
    }
}
