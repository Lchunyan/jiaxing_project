using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ximmerse.XR.Tag
{
    [RequireComponent(typeof(TagTracking))]
    public class MarkerController : MonoBehaviour
    {
        private enum Controller
        {
            left = 0,
            right = 1,
        }

        [SerializeField] 
        private Controller m_marker = Controller.left;

        private bool isButtonDown;

        private int triggerValue;

        private void OnValidate()
        {
            if (m_marker == Controller.left)
            {
                gameObject.GetComponent<TagTracking>().TrackId = 82;
            }
            if (m_marker == Controller.right)
            {
                gameObject.GetComponent<TagTracking>().TrackId = 81;
            }
        }

        public bool IsTriggerButtonDown()
        {
#if !UNITY_EDITOR
            if (XDevicePlugin.xdevc_ctrl_get_button_state_bitmask(XDevicePlugin.xdevc_get_controller((int)m_marker)) == 32)
            {
                isButtonDown = true;
            }
            else
            {
                isButtonDown = false;
            }
#endif
            return isButtonDown;
        }

        public int TriggerValue()
        {
#if !UNITY_EDITOR
            triggerValue = XDevicePlugin.xdevc_ctrl_get_trigger(XDevicePlugin.xdevc_get_controller((int)m_marker));
#endif
            return triggerValue;
        }

        private void Update()
        {
            IsTriggerButtonDown();
        }
    }
}

