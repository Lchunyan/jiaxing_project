using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Jobs;
using System.Threading;
namespace Ximmerse.XR.Asyncoroutine
{
    public static class AwaiterCoroutineExtension
    {
        public static AwaiterCoroutine<IEnumerator> GetAwaiter(this IEnumerator coroutine)
        {
            return new AwaiterCoroutine<IEnumerator>(coroutine);
        }

        //public static AwaiterCoroutine<WaitForNextFrame> GetAwaiter(this WaitForNextFrame waitForNextFrame)
        //{
        //    return new AwaiterCoroutine<WaitForNextFrame>(waitForNextFrame);
        //}

        /// <summary>
        /// Gets awaiter : wait for next frame.
        /// </summary>
        /// <param name="waitForNextFrame"></param>
        /// <returns></returns>
        public static UnityAwaiter GetAwaiter(this WaitForNextFrame waitForNextFrame)
        {
            return AwaiterCoroutineer.GetWaitForNextFrameAwaiter();
        }

        public static UnityAwaiter GetAwaiter(this WaitForTasksInUnityMainThread waitForTasks)
        {
            return AwaiterCoroutineer.GetWaitForTasksAwaiter(waitForTasks.tasks);
        }

        public static UnityAwaiter GetAwaiter(this WaitForTaskInUnityMainThread waitForTask)
        {
            return AwaiterCoroutineer.GetWaitForTaskAwaiter(waitForTask.task);
        }

        public static UnityAwaiter GetAwaiter(this WaitForUpdate waitForUpdate)
        {
            return AwaiterCoroutineer.GetWaitForNextFrameAwaiter(0);
        }

        /// <summary>
        /// Gets awaiter : end of frame.
        /// </summary>
        /// <param name="waitEndOfFrame"></param>
        /// <returns></returns>
        public static UnityAwaiter GetAwaiter(this WaitEndOfFrame waitEndOfFrame)
        {
            return AwaiterCoroutineer.GetWaitEndOfFrameAwaiter();
        }

        /// <summary>
        /// Gets awaiter : late update
        /// </summary>
        /// <param name="waitLateUpdate"></param>
        /// <returns></returns>
        public static UnityAwaiter GetAwaiter(this WaitForLateUpdate waitLateUpdate)
        {
            return AwaiterCoroutineer.GetWaitForLateUpdateAwaiter();
        }

        /// <summary>
        /// Gets awaiter : game time
        /// </summary>
        /// <param name="waitForGameTime"></param>
        /// <returns></returns>
        public static UnityAwaiter GetAwaiter(this WaitForGameTime waitForGameTime)
        {
            return AwaiterCoroutineer.GetWaitForGameTimeAwaiter(waitForGameTime.WaitTime, false);
        }

        /// <summary>
        /// Gets awaiter to wait for job handle to complete.
        /// </summary>
        /// <param name="JobHandle"></param>
        /// <returns></returns>
        public static UnityAwaiter GetAwaiter(this JobHandle JobHandle)
        {
            return AwaiterCoroutineer.GetWaitForJobHandle(JobHandle, null, null);
        }

        /// <summary>
        /// Gets awaiter to wait for job handle to complete.
        /// Waits for maximum time.
        /// </summary>
        /// <param name="JobHandle"></param>
        /// <returns></returns>
        public static UnityAwaiter GetAwaiter(this JobHandle JobHandle, float WaitForMaxTime)
        {
            return AwaiterCoroutineer.GetWaitForJobHandle(JobHandle, null, WaitForMaxTime);
        }

        /// <summary>
        /// Gets awaiter to wait for job handle to complete.
        /// Waits for maximum frame.
        /// </summary>
        /// <param name="JobHandle"></param>
        /// <returns></returns>
        public static UnityAwaiter GetAwaiter(this JobHandle JobHandle, int WaitForFrame)
        {
            return AwaiterCoroutineer.GetWaitForJobHandle(JobHandle, WaitForFrame, null);
        }

        /// <summary>
        /// Gets awaiter 
        /// </summary>
        /// <param name="JobHandle"></param>
        /// <returns></returns>
        public static UnityAwaiter GetAwaiter(this UnityAwaiter awaiter)
        {
            return awaiter;
        }

        /// <summary>
        /// Gets awaiter : game realtime 
        /// </summary>
        /// <param name="waitForRealTime"></param>
        /// <returns></returns>
        public static UnityAwaiter GetAwaiter(this WaitForRealTime waitForRealTime)
        {
            var awaiter = AwaiterCoroutineer.GetWaitForGameTimeAwaiter(waitForRealTime.WaitTime, true);
            awaiter.cancellationToken = waitForRealTime.cancellationToken;
            return awaiter;
        }

        public static AwaiterCoroutine<WaitForSeconds> GetAwaiter(this WaitForSeconds waitForSeconds)
        {
            return new AwaiterCoroutine<WaitForSeconds>(waitForSeconds);
        }

        public static AwaiterCoroutine<WaitForSecondsRealtime> GetAwaiter(this WaitForSecondsRealtime waitForSecondsRealtime)
        {
            return new AwaiterCoroutine<WaitForSecondsRealtime>(waitForSecondsRealtime);
        }

        public static AwaiterCoroutine<WaitForEndOfFrame> GetAwaiter(this WaitForEndOfFrame waitForEndOfFrame)
        {
            return new AwaiterCoroutine<WaitForEndOfFrame>(waitForEndOfFrame);
        }

        /// <summary>
        /// Gets awaiter for fixed update.
        /// </summary>
        /// <param name="waitForFixedUpdate"></param>
        /// <returns></returns>
        public static UnityAwaiter GetAwaiter(this WaitFixedUpdate waitForFixedUpdate)
        {
            return AwaiterCoroutineer.GetWaitForFixedUpdateAwaiter();
        }

        public static AwaiterCoroutine<WaitUntil> GetAwaiter(this WaitUntil waitUntil)
        {
            return new AwaiterCoroutine<WaitUntil>(waitUntil);
        }

        public static AwaiterCoroutine<WaitWhile> GetAwaiter(this WaitWhile waitWhile)
        {
            return new AwaiterCoroutine<WaitWhile>(waitWhile);
        }

#if !UNITY_2019_1_OR_NEWER
        ///WWW is deprecated after 2019
        public static AwaiterCoroutine<WWW> GetAwaiter(this WWW www)
        {
            return new AwaiterCoroutine<WWW>(www);
        }
#endif

        public static AwaiterCoroutine<AsyncOperation> GetAwaiter(this AsyncOperation asyncOperation)
        {
            return new AwaiterCoroutine<AsyncOperation>(asyncOperation);
        }

        public static AwaiterCoroutine<CustomYieldInstruction> GetAwaiter(this CustomYieldInstruction customYieldInstruction)
        {
            return new AwaiterCoroutine<CustomYieldInstruction>(customYieldInstruction);
        }

        //public static AwaiterCoroutineWaitForMainThread GetAwaiter(this WaitForMainThread waitForMainThread)
        //{
        //    return new AwaiterCoroutineWaitForMainThread();
        //}

        public static UnitySubthreadAwaiter GetAwaiter(this SubthreadTimeAwaiter threadNotSafeTimeAwaiter)
        {
            DateTime now = DateTime.Now;
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, (int)threadNotSafeTimeAwaiter.Milliseconds);
            return AwaiterCoroutineer.GetThreadNoSafeTimeAwaiter((now + ts).Ticks);
        }
    }
    /// <summary>
    /// Waits for update routine.
    /// Unlike WaitForNextFrame, WaitForUpdate don't necessariliy waits for 1 frame.
    /// </summary>
    public struct WaitForUpdate { }
    public struct WaitForNextFrame { }//Awaiter type: WaitForNextFrameAwaiter

    /// <summary>
    /// Waits for multi tasks in main unity thread
    /// </summary>
    public struct WaitForTasksInUnityMainThread
    {
        public Task[] tasks;

        /// <summary>
        /// Waits for multi tasks in main unity thread
        /// </summary>
        /// <param name="tasks"></param>
        public WaitForTasksInUnityMainThread(Task[] tasks)
        {
            this.tasks = tasks;
        }
    }
    /// <summary>
    /// Waits for a task in main unity thread
    /// </summary>
    public struct WaitForTaskInUnityMainThread
    {
        public Task task;

        /// <summary>
        /// Waits for multi tasks in main unity thread
        /// </summary>
        /// <param name="tasks"></param>
        public WaitForTaskInUnityMainThread(Task task)
        {
            this.task = task;
        }
    }

    //public struct WaitForMainThread { }
    public struct WaitForLateUpdate { }
    public struct WaitForGameTime
    {
        public float WaitTime;
        public WaitForGameTime(float time)
        {
            this.WaitTime = time;
        }
    }//Wait for Time.time

    /// <summary>
    /// 用于不需要线程安全的时间等待器。
    /// 使用此等待器替换 Task.Delay(milli-second).此等待器没有GC的问题。
    ///
    /// Using this awaiter to replace Task.Delay () for timing awaiting scenario.
    /// This awaiter does not cause GC, and its executing routine is not inside Unity's main-thread routine such as Update/LateUpdate/FixedUpdate.
    /// 
    /// </summary>
    public struct SubthreadTimeAwaiter
    {
        /// <summary>
        /// 等待的毫秒数
        /// </summary>
        public uint Milliseconds;

        public SubthreadTimeAwaiter(uint millis)
        {
            Milliseconds = millis;
        }
    }

    public struct WaitForRealTime
    {
        public float WaitTime;
        public CancellationToken cancellationToken;
        public WaitForRealTime(float time)
        {
            this.WaitTime = time;
            cancellationToken = default(CancellationToken);
        }
        public WaitForRealTime(float time, CancellationToken cancellationToken)
        {
            this.WaitTime = time;
            this.cancellationToken = cancellationToken;
        }
    }//Wait for Time.realTimeSinceStartup
}