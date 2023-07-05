using Unity.XR.CoreUtils;
using UnityEngine;

namespace Ximmerse.XR.Tag
{
    /// <summary>
    /// Used to correct the coordinates of the TagGroundPlane component.
    /// </summary>
    public class TagGoManager : MonoBehaviour
    {
        private XROrigin xROrigin;
        private static TagGoManager instance;
        public static TagGoManager Instance
        {
            get
            {
                return instance;
            }
        }

        private void Awake()
        {
            instance = this;
            xROrigin = FindObjectOfType<XROrigin>();
            if (xROrigin != null)
            {
                gameObject.transform.position = new Vector3(-xROrigin.transform.position.x, -xROrigin.transform.position.y - xROrigin.CameraYOffset, -xROrigin.transform.position.z);

                gameObject.transform.rotation = Quaternion.Inverse(xROrigin.transform.rotation);
            }
        }
        void Update()
        {
            if (xROrigin != null)
            {
                gameObject.transform.position = new Vector3(-xROrigin.transform.position.x, -xROrigin.transform.position.y - xROrigin.CameraYOffset, -xROrigin.transform.position.z);

                gameObject.transform.rotation = Quaternion.Inverse(xROrigin.transform.rotation);
            }
        }
    }
}

