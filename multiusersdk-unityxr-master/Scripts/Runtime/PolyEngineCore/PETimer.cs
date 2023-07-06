using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Ximmerse.XR.Asyncoroutine
{
    /// <summary>
    /// Timer.
    /// </summary>
    public class PETimer
    {
        /// <summary>
        /// The timer's total time
        /// </summary>
        internal float m_TimeLength;

        /// <summary>
        /// Callback : tick function per frame.
        /// </summary>
        Action<float, float> tick;

        /// <summary>
        /// Callback : complete callback function.
        /// </summary>
        Action complete;

        /// <summary>
        /// Is currently ticking ?
        /// </summary>
        internal bool m_IsTicking = false;

        /// <summary>
        /// Starts ticking time.
        /// </summary>
        internal float m_StartTime;

        /// <summary>
        /// Is the timer currently ticking ?
        /// </summary>
        public bool IsTicking
        {
            get => m_IsTicking;
        }

        /// <summary>
        /// Gets a timer.
        /// </summary>
        /// <param name="timeElapsed">Total wait for time</param>
        /// <param name="updateFN">Tick function per frame. Parameter 01 = time elapsed, Parameter 02 = normalized elapsed time.</param>
        /// <param name="completeFN">Complete function.</param>
        /// <param name="delay">The start delay time</param>
        /// <param name="autoStart"></param>
        /// <returns></returns>
        public static PETimer GetTimer(float timeElapsed, Action<float, float> updateFN, Action completeFN, float delay = 0, bool autoStart = true)
        {
            return new PETimer(timeElapsed, updateFN, completeFN, autoStart)
            {
                 m_StartTime = Time.time + delay,
            };
        }

        /// <summary>
        /// Ctor PETimer.
        /// </summary>
        /// <param name="timeElapsed">Total wait for time</param>
        /// <param name="updateFN">Tick function per frame. Parameter 01 = time elapsed, Parameter 02 = normalized elapsed time.</param>
        /// <param name="completeFN">Complete function.</param>
        /// <param name="autoStart">Auto starts timer? </param>
        internal PETimer(float timeElapsed, Action<float, float> updateFN, Action completeFN, bool autoStart = true)
        {
            m_TimeLength = timeElapsed;
            tick = updateFN;
            complete = completeFN;
            if (autoStart)
            {
                InternalStartTimer();
            }
        }

        /// <summary>
        /// Starts the timer's ticking process.
        /// </summary>
        public void StartTimer()
        {
            InternalStartTimer();
        }

        void InternalStartTimer()
        {
            if (m_IsTicking)
            {
                return;//Already ticking.
            }
            AwaiterCoroutineer.AddTimer(this);
        }

        internal void Complete()
        {
            if (complete != null)
            {
                complete();
            }

            m_IsTicking = false;
        }

        internal void Update(float now)
        {
            if (this.tick != null)
            {
                //param 01 = elapsed time
                //param 02 = normalized 
                tick(now - m_StartTime, (now - m_StartTime) / m_TimeLength);
            }
        }


    }
}