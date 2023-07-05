using UnityEngine;

namespace Ximmerse.XR.Tag
{
    [AddComponentMenu("Ximmerse XR/Tracking Event")]
    public class TrackingEvent : MonoBehaviour
    {
        [System.Serializable]
        public class OnFirstTrackingEvent : UnityEngine.Events.UnityEvent
        { }
        [System.Serializable]
        public class OnTrackingEnter : UnityEngine.Events.UnityEvent
        { }
        [System.Serializable]
        public class OnTrackingStay : UnityEngine.Events.UnityEvent
        { }
        [System.Serializable]
        public class OnTrackingExit : UnityEngine.Events.UnityEvent
        { }
        [Header("--- Tracking Event ---")]
        public OnFirstTrackingEvent onFirstTrackingEvent = new OnFirstTrackingEvent();
        public OnTrackingEnter onTrackingEnter = new OnTrackingEnter();
        public OnTrackingStay onTrackingStay = new OnTrackingStay();
        public OnTrackingExit onTrackingExit = new OnTrackingExit();
    }
}

