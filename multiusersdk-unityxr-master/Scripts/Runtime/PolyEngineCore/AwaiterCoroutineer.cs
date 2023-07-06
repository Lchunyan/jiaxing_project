using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using UnityEngine;
using System;
using Object = UnityEngine.Object;
using Unity.Jobs;
using System.Threading.Tasks;
namespace Ximmerse.XR.Asyncoroutine
{
    /// <summary>
    /// 核心异步 Coroutine 容器.
    /// 支持自更模式和代理更新模式。
    /// </summary>
    [ExecuteAlways]
    public class AwaiterCoroutineer : MonoBehaviour
    {
        static object referenceInstance = null;

        public static bool HasInstance()
        {
            var _instance = FindObjectOfType<AwaiterCoroutineer>();
            if (_instance)
            {
                referenceInstance = _instance;
            }
            return referenceInstance != null;
        }

        private static AwaiterCoroutineer _instance;
        public static AwaiterCoroutineer Instance
        {
            get
            {
                Install();
                return _instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        internal static void Install()
        {
            var _instance = FindObjectOfType<AwaiterCoroutineer>();
            if (_instance)
            {
                referenceInstance = _instance;
            }
            if (referenceInstance == null)//use c# class for null comparision.
            {
                _instance = new GameObject().AddComponent<AwaiterCoroutineer>();
#if !UNITY_EDITOR //非源码模式，隐藏 AwaiterCoroutineer 对象。
                _instance.gameObject.hideFlags = HideFlags.HideAndDontSave;
#endif
                if (Application.isPlaying)
                {
                    DontDestroyOnLoad(_instance.gameObject);
                }
                else
                {
                    _instance.gameObject.hideFlags = HideFlags.DontSave;
                }
                referenceInstance = _instance;
            }
        }

        internal SynchronizationContext SynchronizationContext { get; private set; }

        /// <summary>
        /// 用于 main thread awaiter 的thread locker
        /// </summary>
        static System.Object mutex_for_main_thread_awaiter = new object();

        /// <summary>
        /// 用于 sub thread awaiter 的thread locker
        /// </summary>
        static System.Object mutex_for_sub_thread_awaiter = new object();

        /// <summary>
        /// 缓存的 UnityAwaiter.
        /// </summary>
        internal List<UnityAwaiter> Pooled_Unity_Awaiter = new List<UnityAwaiter>(32);

        /// <summary>
        /// 激活的 UnityAwaiter.
        /// </summary>
        internal List<UnityAwaiter> Active_Unity_Awaiter = new List<UnityAwaiter>(32);

        /// <summary>
        /// 缓存池中的子线程计时等待器.
        /// </summary>
        internal List<UnitySubthreadAwaiter> Pooled_Thread_No_Safe_Time_Awaiter = new List<UnitySubthreadAwaiter>(32);

        /// <summary>
        /// 激活的子线程计时等待器.
        /// </summary>
        internal List<UnitySubthreadAwaiter> Active_Thread_No_Safe_Time_Awaiter = new List<UnitySubthreadAwaiter>(32);

        /// <summary>
        /// Internal ticking timers.
        /// </summary>
        internal List<PETimer> tickingTimers = new List<PETimer>();

        static internal int FrameCount = 0;

        static internal float GameTime, GameRealtime;

        /// <summary>
        /// 如果 UpdateByExternalCall == true, 则 AwaiterCoroutineer 自身不会在Update()中更新
        /// WaitForNextFrame | WaitForGameTime | WaitForRealTime 的等待器，外部脚本需要显式调用
        /// UpdateJob() 代码来更新等待器列表。
        /// </summary>
        public bool UpdateByExternalCall = false;

        /// <summary>
        /// 如果 LateUpdateByExternalCall == true, 则 AwaiterCoroutineer 自身不会在 LateUpdate()中更新
        /// WaitForLateUpdate 的等待器，外部脚本需要显式调用
        /// LateUpdateJob() 代码来更新信息。
        /// </summary>
        public bool LateUpdateByExternalCall = false;

        /// <summary>
        /// 如果 FixedUpdateByExternalCall == true, 则 AwaiterCoroutineer 自身不会在 FixedUpdate()中更新
        /// WaitForFixedUpdate 的等待器，外部脚本需要显式调用 FixedUpdateJob() 方法来更新等待器列表。
        /// </summary>
        public bool FixedUpdateByExternalCall = false;

        /// <summary>
        /// 如果 EndOfFrameUpdateByExternalCall == true, 则 AwaiterCoroutineer 自身不会在 WaitForEndOfFrame()中更新
        /// WaitForEndOfFrame 的等待器，外部脚本需要显式调用 EndOfFrameUpdateJob() 方法来更新等待器列表。
        /// </summary>
        public bool EndOfFrameUpdateByExternalCall = false;

        /// <summary>
        /// Gets wait for next frame awaiter .
        /// </summary>
        /// <returns></returns>
        internal static UnityAwaiter GetWaitForNextFrameAwaiter(int WaitForFrameCount = 1)
        {
            //确保 instance 一定存在:
            if (referenceInstance == null)
            {
                Install();
            }

            UnityAwaiter awaiter = null;
            lock (mutex_for_main_thread_awaiter)
            {
                int c = _instance.Pooled_Unity_Awaiter.Count;
                if (c > 0)
                {
                    awaiter = _instance.Pooled_Unity_Awaiter[c - 1];
                    _instance.Pooled_Unity_Awaiter.RemoveAt(c - 1);
                }
                else
                {
                    awaiter = new UnityAwaiter();
                }

                awaiter.Reset();
                awaiter.waitMode = UnityAwaiterWaitMode.WaitForNextFrame;
                awaiter.TargetFrame = FrameCount + WaitForFrameCount;
                //Debug.LogFormat("Set target frame count = {0}, frameCount = {1}, Time.frameCount = {2}", awaiter.TargetFrame, FrameCount, Time.frameCount);
                _instance.Active_Unity_Awaiter.Add(awaiter);
            }
            return awaiter;
        }

        /// <summary>
        /// Gets a wait for multi-tasks awaiter.
        /// </summary>
        /// <returns></returns>
        internal static UnityAwaiter GetWaitForTasksAwaiter(Task[] tasks)
        {
            //确保 instance 一定存在:
            if (referenceInstance == null)
            {
                Install();
            }

            UnityAwaiter awaiter = null;
            lock (mutex_for_main_thread_awaiter)
            {
                int c = _instance.Pooled_Unity_Awaiter.Count;
                if (c > 0)
                {
                    awaiter = _instance.Pooled_Unity_Awaiter[c - 1];
                    _instance.Pooled_Unity_Awaiter.RemoveAt(c - 1);
                }
                else
                {
                    awaiter = new UnityAwaiter();
                }

                awaiter.Reset();
                awaiter.waitMode = UnityAwaiterWaitMode.WaitForTasks;
                awaiter.tasks = tasks;
                //Debug.LogFormat("Set target frame count = {0}, frameCount = {1}, Time.frameCount = {2}", awaiter.TargetFrame, FrameCount, Time.frameCount);
                _instance.Active_Unity_Awaiter.Add(awaiter);
            }
            return awaiter;
        }


        /// <summary>
        /// Gets a wait for task awaiter.
        /// </summary>
        /// <returns></returns>
        internal static UnityAwaiter GetWaitForTaskAwaiter(Task task)
        {
            //确保 instance 一定存在:
            if (referenceInstance == null)
            {
                Install();
            }

            UnityAwaiter awaiter = null;
            lock (mutex_for_main_thread_awaiter)
            {
                int c = _instance.Pooled_Unity_Awaiter.Count;
                if (c > 0)
                {
                    awaiter = _instance.Pooled_Unity_Awaiter[c - 1];
                    _instance.Pooled_Unity_Awaiter.RemoveAt(c - 1);
                }
                else
                {
                    awaiter = new UnityAwaiter();
                }

                awaiter.Reset();
                awaiter.waitMode = UnityAwaiterWaitMode.WaitForTask;
                awaiter.task = task;
                //Debug.LogFormat("Set target frame count = {0}, frameCount = {1}, Time.frameCount = {2}", awaiter.TargetFrame, FrameCount, Time.frameCount);
                _instance.Active_Unity_Awaiter.Add(awaiter);
            }
            return awaiter;
        }

        /// <summary>
        /// Gets wait for end of frame awaiter .
        /// </summary>
        /// <returns></returns>
        internal static UnityAwaiter GetWaitEndOfFrameAwaiter()
        {
            //确保 instance 一定存在:
            if (referenceInstance == null)
            {
                Install();
            }

            UnityAwaiter awaiter = null;
            lock (mutex_for_main_thread_awaiter)
            {
                int c = _instance.Pooled_Unity_Awaiter.Count;
                if (c > 0)
                {
                    awaiter = _instance.Pooled_Unity_Awaiter[c - 1];
                    _instance.Pooled_Unity_Awaiter.RemoveAt(c - 1);
                }
                else
                {
                    awaiter = new UnityAwaiter();
                }

                awaiter.Reset();
                awaiter.waitMode = UnityAwaiterWaitMode.WaitForEndOfFrame;
                _instance.Active_Unity_Awaiter.Add(awaiter);
                //  Debug.LogFormat("Insert EoF awaiter @{0}", Time.frameCount);
            }

            return awaiter;
        }

        /// <summary>
        /// Gets wait for fixed update 
        /// </summary>
        /// <returns></returns>
        internal static UnityAwaiter GetWaitForFixedUpdateAwaiter()
        {
            //确保 instance 一定存在:
            if (referenceInstance == null)
            {
                Install();
            }

            UnityAwaiter awaiter = null;
            lock (mutex_for_main_thread_awaiter)
            {
                int c = _instance.Pooled_Unity_Awaiter.Count;
                if (c > 0)
                {
                    awaiter = _instance.Pooled_Unity_Awaiter[c - 1];
                    _instance.Pooled_Unity_Awaiter.RemoveAt(c - 1);
                }
                else
                {
                    awaiter = new UnityAwaiter();
                }
                awaiter.Reset();
                awaiter.waitMode = UnityAwaiterWaitMode.WaitForFixedUpdate;
                _instance.Active_Unity_Awaiter.Add(awaiter);
            }
            return awaiter;
        }

        /// <summary>
        /// Gets wait for late update 
        /// </summary>
        /// <returns></returns>
        internal static UnityAwaiter GetWaitForLateUpdateAwaiter()
        {
            //确保 instance 一定存在:
            if (referenceInstance == null)
            {
                Install();
            }

            UnityAwaiter awaiter = null;
            lock (mutex_for_main_thread_awaiter)
            {
                int c = _instance.Pooled_Unity_Awaiter.Count;
                if (c > 0)
                {
                    awaiter = _instance.Pooled_Unity_Awaiter[c - 1];
                    _instance.Pooled_Unity_Awaiter.RemoveAt(c - 1);
                }
                else
                {
                    awaiter = new UnityAwaiter();
                }
                awaiter.Reset();
                awaiter.waitMode = UnityAwaiterWaitMode.WaitForFrameLateUpdate;
                awaiter.TargetFrame = FrameCount;//LateUpdate 不需要跳一帧。
                _instance.Active_Unity_Awaiter.Add(awaiter);
            }
            return awaiter;
        }

        /// <summary>
        /// Gets awaiter for game time.
        /// </summary>
        /// <returns></returns>
        internal static UnityAwaiter GetWaitForGameTimeAwaiter(float time, bool realtime)
        {
            //确保 instance 一定存在:
            if (referenceInstance == null)
            {
                Install();
            }

            UnityAwaiter awaiter = null;
            lock (mutex_for_main_thread_awaiter)
            {
                int c = _instance.Pooled_Unity_Awaiter.Count;
                if (c > 0)
                {
                    awaiter = _instance.Pooled_Unity_Awaiter[c - 1];
                    _instance.Pooled_Unity_Awaiter.RemoveAt(c - 1);
                }
                else
                {
                    awaiter = new UnityAwaiter();
                }
                awaiter.Reset();
                //waits for realtime:
                if (realtime)
                {
                    awaiter.waitMode = UnityAwaiterWaitMode.WaitForRealtime;
                    awaiter.Time = GameRealtime + time;
                }
                //waits for game time:
                else
                {
                    awaiter.waitMode = UnityAwaiterWaitMode.WaitForGameTime;
                    awaiter.Time = GameTime + time;
                }
                _instance.Active_Unity_Awaiter.Add(awaiter);
            }
            return awaiter;
        }

        /// <summary>
        /// Gets wait for job handle
        /// </summary>
        /// <param name="jobHandle"></param>
        /// <returns></returns>
        internal static UnityAwaiter GetWaitForJobHandle(JobHandle jobHandle, int? WaitForMaxFrameCount, float? WaitForMaxTime)
        {
            //确保 instance 一定存在:
            if (referenceInstance == null)
            {
                Install();
            }

            UnityAwaiter awaiter = null;
            lock (mutex_for_main_thread_awaiter)
            {
                int c = _instance.Pooled_Unity_Awaiter.Count;
                if (c > 0)
                {
                    awaiter = _instance.Pooled_Unity_Awaiter[c - 1];
                    _instance.Pooled_Unity_Awaiter.RemoveAt(c - 1);
                }
                else
                {
                    awaiter = new UnityAwaiter();
                }
                awaiter.Reset();
                if (WaitForMaxTime.HasValue)
                {
                    awaiter.waitMode = UnityAwaiterWaitMode.WaitForJobHandleInMaxTime;//for jobs in max time
                    awaiter.Time = GameTime + WaitForMaxTime.Value;
                }
                else if (WaitForMaxFrameCount.HasValue)
                {
                    awaiter.waitMode = UnityAwaiterWaitMode.WaitForJobHandleInMaxFrame;//for jobs in max frame
                    awaiter.TargetFrame = FrameCount + WaitForMaxFrameCount.Value;
                }
                else
                {
                    awaiter.waitMode = UnityAwaiterWaitMode.WaitForJobHandle;//wait for jobs
                }
                awaiter.jobHandle = jobHandle;
                _instance.Active_Unity_Awaiter.Add(awaiter);
            }
            return awaiter;
        }

        Coroutine coroutine_EndOfFrame;

        bool endOfFrameUpdate;

        Thread unsafeTimerInvoker = null;

        /// <summary>
        /// If true, there is awaiter in the calling queue.
        /// </summary>
        /// <returns></returns>
        public bool HasAwaiter()
        {
            return !this.Active_Unity_Awaiter.IsNullOrEmpty();
        }

        private void Awake()
        {
            if (referenceInstance == null)
            {
                _instance = this;
                referenceInstance = this;
            }
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(_instance);
            }
            SynchronizationContext = SynchronizationContext.Current;

            //Create pooled WaitForNextFrameAwaiter instance, default 20.
            for (int i = 0; i < 50; i++)
            {
                Pooled_Unity_Awaiter.Add(new UnityAwaiter());
                Pooled_Thread_No_Safe_Time_Awaiter.Add(new UnitySubthreadAwaiter());
            }

            endOfFrameUpdate = true;
            coroutine_EndOfFrame = StartCoroutine(EndOfFrame());

            //启动子线程等待器:
            unsafeTimerInvoker = new Thread(UnsafeThreadTimeAwaiterHandler);
            unsafeTimerInvoker.Start();
        }

        private void Start()
        {
            FrameCount = Time.frameCount;
        }

        private void OnDestroy()
        {
            if (coroutine_EndOfFrame != null)
            {
                //StopCoroutine(coroutine_EndOfFrame);
                endOfFrameUpdate = false;
                coroutine_EndOfFrame = null;
            }
            _instance = null;
            referenceInstance = null;
        }

        internal void StartAwaiterCoroutine<T>(AwaiterCoroutine<T> awaiterCoroutine)
        {
            StartCoroutine(awaiterCoroutine.Coroutine);
        }

        //void StopAwaiterCoroutine<T>(AwaiterCoroutine<T> awaiterCoroutine)
        //{
        //    StopCoroutine(awaiterCoroutine.Coroutine);
        //}

        private void Update()
        {

            //Updates timers:
            {
                float _time = Time.time;
                //Update timer:
                if (this.tickingTimers.Count > 0)
                {
                    for (int i = tickingTimers.Count - 1; i >= 0; i--)
                    {
                        PETimer timer = tickingTimers[i];
                        if (_time - timer.m_StartTime >= timer.m_TimeLength)
                        {
                            timer.Complete();
                            tickingTimers.RemoveAt(i);
                        }
                        else
                        {
                            timer.Update(_time);
                        }
                    }
                }
            }

            if (UpdateByExternalCall)//使用外部调用
            {
                return;
            }
            UpdateJob();
        }


        private void LateUpdate()
        {
            if (LateUpdateByExternalCall)//使用外部调用
            {
                return;
            }
            LateUpdateJob();
        }

        private void FixedUpdate()
        {
            if (FixedUpdateByExternalCall)//使用外部调用
            {
                return;
            }
            FixedUpdateJob();
        }

        IEnumerator EndOfFrame()
        {
            WaitForEndOfFrame waitEof = new WaitForEndOfFrame();
            while (endOfFrameUpdate)
            {
                yield return waitEof;
                if (!EndOfFrameUpdateByExternalCall)//使用外部调用
                {
                    EndOfFrameUpdateJob();
                }
            }
        }

        /// <summary>
        /// Update()
        /// </summary>
        public void UpdateJob()
        {
            lock (mutex_for_main_thread_awaiter)
            {
                FrameCount = Time.frameCount;
                GameTime = Time.time;
                GameRealtime = Time.realtimeSinceStartup;
                if (Active_Unity_Awaiter.Count > 0)
                {
                    for (int i = Active_Unity_Awaiter.Count - 1; i >= 0; i--)
                    {
                        var awaiter = Active_Unity_Awaiter[i];
                        //如果某个 awaiter 被cancel了: 
                        if (awaiter.cancellationToken.IsCancellationRequested)
                        {
                            //Debug.LogFormat("On cancel an awaiter : {0}", i);
                            awaiter.OnCancel();
                            //回池 awaiter 实例
                            Active_Unity_Awaiter.RemoveAt(i);
                            Pooled_Unity_Awaiter.Add(awaiter);
                            continue;
                        }
                        //在 Update 中，处理 WaitForNextFrame 的 Unity Awaiter.
                        if (awaiter.waitMode == UnityAwaiterWaitMode.WaitForNextFrame && FrameCount >= awaiter.TargetFrame)
                        {
                            //回调action
                            awaiter.Complete();
                            //回池awaiter 实例
                            Active_Unity_Awaiter.RemoveAt(i);
                            Pooled_Unity_Awaiter.Add(awaiter);
                        }
                        //Waits for game time:
                        else if (awaiter.waitMode == UnityAwaiterWaitMode.WaitForGameTime && GameTime >= awaiter.Time)
                        {
                            //回调action
                            awaiter.Complete();
                            //回池awaiter 实例
                            Active_Unity_Awaiter.RemoveAt(i);
                            Pooled_Unity_Awaiter.Add(awaiter);
                        }
                        //Waits for game real time:
                        else if (awaiter.waitMode == UnityAwaiterWaitMode.WaitForRealtime && GameRealtime >= awaiter.Time)
                        {
                            //回调action
                            awaiter.Complete();
                            //回池awaiter 实例
                            Active_Unity_Awaiter.RemoveAt(i);
                            Pooled_Unity_Awaiter.Add(awaiter);
                        }
                        //Waits for job handle 
                        else if (awaiter.waitMode == UnityAwaiterWaitMode.WaitForJobHandle)
                        {
                            if (awaiter.jobHandle.Value.IsCompleted)
                            {
                                awaiter.jobHandle.Value.Complete();
                            }
                            awaiter.Complete();
                            //回池awaiter 实例
                            Active_Unity_Awaiter.RemoveAt(i);
                            Pooled_Unity_Awaiter.Add(awaiter);
                        }
                        //Waits for job handle in max frame
                        else if (awaiter.waitMode == UnityAwaiterWaitMode.WaitForJobHandleInMaxFrame)
                        {
                            bool isCompleted = false;
                            if (awaiter.jobHandle.Value.IsCompleted)//自然完成
                            {
                                awaiter.jobHandle.Value.Complete();
                                isCompleted = true;
                            }
                            else if (awaiter.TargetFrame <= FrameCount)//强制完成
                            {
                                awaiter.jobHandle.Value.Complete();
                                isCompleted = true;
                            }
                            if (isCompleted)
                            {
                                awaiter.Complete();
                                //回池awaiter 实例
                                Active_Unity_Awaiter.RemoveAt(i);
                                Pooled_Unity_Awaiter.Add(awaiter);
                            }
                        }
                        //Waits for job handle in max time
                        else if (awaiter.waitMode == UnityAwaiterWaitMode.WaitForJobHandleInMaxTime)
                        {
                            bool isCompleted = false;
                            if (awaiter.jobHandle.Value.IsCompleted)//自然完成
                            {
                                awaiter.jobHandle.Value.Complete();
                                isCompleted = true;
                            }
                            else if (awaiter.Time >= GameTime)//强制完成
                            {
                                awaiter.jobHandle.Value.Complete();
                                isCompleted = true;
                            }
                            if (isCompleted)
                            {
                                awaiter.Complete();
                                //回池awaiter 实例
                                Active_Unity_Awaiter.RemoveAt(i);
                                Pooled_Unity_Awaiter.Add(awaiter);
                            }
                        }
                        //wait for multi tasks :
                        else if (awaiter.waitMode == UnityAwaiterWaitMode.WaitForTasks)
                        {
                            bool all_task_is_complete = true;
                            for (int j = 0; j < awaiter.tasks.Length; j++)
                            {
                                Task t = awaiter.tasks[j];
                                if (!t.IsCompleted)
                                {
                                    all_task_is_complete = false;
                                    break;
                                }
                            }

                            if (all_task_is_complete)
                            {
                                awaiter.tasks = null;//clear tasks reference
                                awaiter.Complete();
                                //回池awaiter 实例
                                Active_Unity_Awaiter.RemoveAt(i);
                                Pooled_Unity_Awaiter.Add(awaiter);
                            }
                        }
                        //wait for single task :
                        else if (awaiter.waitMode == UnityAwaiterWaitMode.WaitForTask)
                        {
                            if (awaiter.task.IsCompleted)
                            {
                                awaiter.task = null;//clear task reference
                                awaiter.Complete();
                                //回池awaiter 实例
                                Active_Unity_Awaiter.RemoveAt(i);
                                Pooled_Unity_Awaiter.Add(awaiter);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// LateUpdate()
        /// </summary>
        public void LateUpdateJob()
        {
            lock (mutex_for_main_thread_awaiter)
            {
                GameRealtime = Time.realtimeSinceStartup;
                if (Active_Unity_Awaiter.Count > 0)
                {
                    for (int i = Active_Unity_Awaiter.Count - 1; i >= 0; i--)
                    {
                        var awaiter = Active_Unity_Awaiter[i];
                        //在 LateUpdate 中，处理 WaitForLateUpdate 的 Unity Awaiter.
                        if (awaiter.waitMode == UnityAwaiterWaitMode.WaitForFrameLateUpdate)
                        {
                            //回调action
                            awaiter.Complete();
                            //回池awaiter 实例
                            Active_Unity_Awaiter.RemoveAt(i);
                            Pooled_Unity_Awaiter.Add(awaiter);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Fixed Update
        /// </summary>
        public void FixedUpdateJob()
        {
            lock (mutex_for_main_thread_awaiter)
            {
                for (int i = Active_Unity_Awaiter.Count - 1; i >= 0; i--)
                {
                    var awaiter = Active_Unity_Awaiter[i];
                    //在 LateUpdate 中，处理 Waits for fixed update 的 Unity Awaiter.
                    if (awaiter.waitMode == UnityAwaiterWaitMode.WaitForFixedUpdate)
                    {
                        //回调action
                        awaiter.Complete();
                        //回池awaiter 实例
                        Active_Unity_Awaiter.RemoveAt(i);
                        Pooled_Unity_Awaiter.Add(awaiter);
                    }
                }
            }
        }

        /// <summary>
        /// Call this method in external class, to invoke end of frame update .
        /// </summary>
        public void EndOfFrameUpdateJob()
        {
            lock (mutex_for_main_thread_awaiter)
            {
                for (int i = Active_Unity_Awaiter.Count - 1; i >= 0; i--)
                {
                    var awaiter = Active_Unity_Awaiter[i];
                    //在 Update 中，处理 WaitForEndOfFrame 的 Unity Awaiter.
                    if (awaiter.waitMode == UnityAwaiterWaitMode.WaitForEndOfFrame)
                    {
                        //回调action
                        awaiter.Complete();
                        //回池awaiter 实例
                        Active_Unity_Awaiter.RemoveAt(i);
                        Pooled_Unity_Awaiter.Add(awaiter);
                        //   Debug.LogFormat("Complete EoF awaiter @{0}", Time.frameCount);
                    }
                }
            }
        }

        /// <summary>
        /// Gets a thread no safe time-awaiter.
        /// 获取一个线程不安全的子线程等待器.
        /// </summary>
        /// <returns></returns>
        internal static UnitySubthreadAwaiter GetThreadNoSafeTimeAwaiter(long targetTime)
        {
            UnitySubthreadAwaiter threadNoSafeAwaiter = null;
            lock (mutex_for_sub_thread_awaiter)
            {
                int c = _instance.Pooled_Thread_No_Safe_Time_Awaiter.Count;
                if (c > 0)
                {
                    threadNoSafeAwaiter = _instance.Pooled_Thread_No_Safe_Time_Awaiter[c - 1];
                    _instance.Pooled_Thread_No_Safe_Time_Awaiter.RemoveAt(c - 1);
                }
                else
                {
                    threadNoSafeAwaiter = new UnitySubthreadAwaiter();
                }
                threadNoSafeAwaiter.targetTicks = targetTime;
                _instance.Active_Thread_No_Safe_Time_Awaiter.Add(threadNoSafeAwaiter);
            }

            return threadNoSafeAwaiter;
        }

        /// <summary>
        /// 子线程等待器处理线程。
        /// </summary>
        void UnsafeThreadTimeAwaiterHandler()
        {
            while (!ReferenceEquals(referenceInstance, null))
            {
                lock (mutex_for_sub_thread_awaiter)
                {
                    int awaiterCount = Active_Thread_No_Safe_Time_Awaiter.Count;
                    if (awaiterCount > 0)
                    {
                        long nowTick = DateTime.Now.Ticks;
                        for (int i = awaiterCount - 1; i >= 0; i--)
                        {
                            UnitySubthreadAwaiter subThreadAwaiter = Active_Thread_No_Safe_Time_Awaiter[i];
                            if (nowTick >= subThreadAwaiter.targetTicks)//时间到
                            {
                                //Debug.LogFormat("2 : {0} {1}", nowTick, subThreadAwaiter.targetTicks);
                                subThreadAwaiter.Complete();//完成
                                subThreadAwaiter.Reset();
                                Active_Thread_No_Safe_Time_Awaiter.RemoveAt(i);//从活跃队列移除.
                                Pooled_Thread_No_Safe_Time_Awaiter.Add(subThreadAwaiter);//加入到缓存队列.
                            }
                        }
                    }
                }

                Thread.Sleep(1);//等待1ms
            }
        }

        /// <summary>
        /// Registers a timer.
        /// </summary>
        /// <param name="timer"></param>
        internal static void AddTimer(PETimer timer)
        {
            Install();
            var timers = Instance.tickingTimers;
            //duplicate timer:
            if (timer.m_IsTicking || timers.Contains(timer))
            {
                return;
            }
            timers.Add(timer);
        }
    }
}