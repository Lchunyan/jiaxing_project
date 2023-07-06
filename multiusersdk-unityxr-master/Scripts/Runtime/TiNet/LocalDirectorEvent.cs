using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Events;

namespace Ximmerse.XR
{
    /// <summary>
    /// 监听本地的director ， 并发布事件
    /// </summary>
    public class LocalDirectorEvent : MonoBehaviour
    {
        public PlayableDirector director;

        public UnityEvent onStopped;

        private void Awake()
        {
            director.stopped += Director_stopped;
        }

        private void Director_stopped(PlayableDirector obj)
        {
            onStopped?.Invoke();
            Debug.LogFormat("On director: {0} stopped", obj.name);
        }
    }
}