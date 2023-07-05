using System;
using UnityEngine;
using System.Runtime.CompilerServices;
using Unity.Jobs;
using System.Threading;
using System.Threading.Tasks;

namespace Ximmerse.XR.Asyncoroutine
{
    /// <summary>
    /// Wait for end of frame
    /// </summary>
    public struct WaitEndOfFrame { }

    /// <summary>
    /// Waits for fixed update.
    /// </summary>
    public struct WaitFixedUpdate { }

    /// <summary>
    /// unity awaiter wait mode
    /// </summary>
    internal enum UnityAwaiterWaitMode : byte
    {
        WaitForNextFrame = 0,

        WaitForEndOfFrame = 1,

        WaitForFrameLateUpdate = 2,

        WaitForGameTime = 3,

        WaitForRealtime = 4,

        WaitForFixedUpdate = 5,

        /// <summary>
        /// Waits for job handle to complete 
        /// </summary>
        WaitForJobHandle = 6,

        /// <summary>
        /// Waits for job handle to complete in max frame.
        /// </summary>
        WaitForJobHandleInMaxFrame = 7,

        /// <summary>
        /// Waits for job handle to complete in max time
        /// </summary>
        WaitForJobHandleInMaxTime = 8,

        /// <summary>
        /// Waits for a task array.
        /// </summary>
        WaitForTasks = 9,

        /// <summary>
        /// Waits for single task
        /// </summary>
        WaitForTask = 10,
    }

    /// <summary>
    /// Unity awaiter , used for thread-safe scenario.
    /// </summary>
    public class UnityAwaiter : INotifyCompletion
    {
        protected Action action = null;

        internal bool isCompleted = false;

        internal int TargetFrame = 0;

        /// <summary>
        /// The wait mode
        /// </summary>
        internal UnityAwaiterWaitMode waitMode;

        /// <summary>
        /// Target time.
        /// </summary>
        internal float Time;

        /// <summary>
        /// The job handle.
        /// </summary>
        internal JobHandle? jobHandle;

        internal CancellationToken cancellationToken = default(CancellationToken);

        internal Task[] tasks;

        internal Task task;

        private bool isCancelled;

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

        /// <summary>
        /// Call on the task is cancelled.
        /// </summary>
        internal void OnCancel()
        {
            isCompleted = true;
            cancellationToken = default(CancellationToken);//reset fields
            isCancelled = true;//mark cancel flags
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
            //Debug.LogFormat("Get result , cancelled : {0}", isCancelled);
            if(isCancelled)
            {
                throw new TaskCanceledException();
            }
            else
            {
                return default(System.Object);
            }
        }

        public void Reset()
        {
            action = null;
            waitMode = default(UnityAwaiterWaitMode);
            this.isCompleted = false;
            jobHandle = default(JobHandle?);
            cancellationToken = default(CancellationToken);
            isCancelled = false;
            tasks = null;
        }
    }
}
