using Unity.XR.CoreUtils;
using UnityEngine;

namespace Ximmerse.XR.Tag
{
    /// <summary>
    /// Calculate the offset of XROrigin and correct the Tag Tracking data coordinates.
    /// </summary>
    public class TrackingManager : MonoBehaviour
    {
        #region Property
        private XROrigin _xROrigin;
        float Y;
        Vector3 Tran;

        private static TrackingManager instance;
        public static TrackingManager Instance
        {
            get
            {
                return instance;
            }
        }

        public XROrigin xROrigin
        {
            get => _xROrigin;
        }

        private void Awake()
        {
            instance = this;
            _xROrigin = GameObject.FindObjectOfType<XROrigin>();
        }
        #endregion
        #region Unity
        void Start()
        {
            if (xROrigin != null)
            {
                Y = _xROrigin.GetComponent<XROrigin>().CameraYOffset;
            }
        }

        void Update()
        {
            if (xROrigin != null)
            {
                Tran = new Vector3(_xROrigin.transform.position.x, _xROrigin.transform.position.y + Y, _xROrigin.transform.position.z);
                transform.position = Tran;
                transform.rotation = _xROrigin.transform.rotation;
            }
        }
        #endregion
    }
}



