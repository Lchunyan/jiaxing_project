using Unity.XR.CoreUtils;
using UnityEngine;
using Ximmerse.XR.Utils;
using Ximmerse.XR.Internal;
using System.Collections;

namespace Ximmerse.XR.Tag
{
    /// <summary>
    /// Implement large spatial positioning capabilities and handle appropriate events.
    /// </summary>
    public class GroundSpace : MonoBehaviour
    {
        #region Property

        private TagGoManager _tagGoManager;
        private XROrigin xr;
        private GameObject tagground_clone;
        private float px, py, pz, rx, ry, rz, rw;
        private long predTimestampNano = 0;
        private bool first = false;
        private bool exit = false;
        private bool onfirstTrackingEnter = false;
        private bool onTrackingEnter = false;
        private bool onTrackingStay = false;
        private bool onTrackingExit = false;
        private bool isTrakingState;
        private bool isvalid;
        private GameObject debugaxis;
        protected TrackingEvent trackingEvent;
        [Header("--- Marker Setting ---")]
        [SerializeField]
        protected int trackID = 65;
        [SerializeField]
        protected float m_Confidence = 0.9f;
        [Header("--- Vio Drift Threshold ---")]
        [SerializeField]
        protected float m_distance = 1.0f;
        [SerializeField]
        protected float m_angle = 1.0f;
        [Header("--- Tracking distance ---")]
        [SerializeField]
        protected float m_minDistance = 0.1f;
        [SerializeField]
        protected float m_maxDistance = 1.8f;
        [Header("--- Debug Setting ---")]
        [SerializeField]
        protected bool m_debugView;
        [SerializeField]
        protected float m_size = 0.17f;
        //private XDevicePlugin.XAttrTrackingInfo trackingInfo;
        #endregion

        #region Unity

        protected virtual void Awake()
        {

        }

        protected virtual void OnDestroy()
        {

        }

        private void Start()
        {
            CloneGamGround();
            trackingEvent = GetComponent<TrackingEvent>();
        }

        private void Update()
        {

            if (TagProfileLoading.Instance != null)
            {
                if (trackingEvent != null || m_debugView)
                {
                    isTrakingState = IsTracking();
                    if (trackingEvent != null)
                    {
                        if (OnFirstTrackingEnter())
                        {
                            trackingEvent.onFirstTrackingEvent?.Invoke();
                        }
                        if (OnTrackingEnter())
                        {
                            trackingEvent.onTrackingEnter?.Invoke();
                        }
                        if (OnTrackingStay())
                        {
                            trackingEvent.onTrackingStay?.Invoke();
                        }
                        if (OnTrackingExit())
                        {
                            trackingEvent.onTrackingExit?.Invoke();
                        }
                    }
                    if (m_debugView && IsValid())
                    {
                        DrawDebugView(m_size);
                    }
                }
            }
            
        }
        #endregion

        #region Method
        protected Vector3 FixXRPosition()
        {
            CloneGamGround();
            if (xr != null)
            {
                px = gameObject.transform.position.x * Mathf.Cos(xr.transform.eulerAngles.y * Mathf.Deg2Rad) - gameObject.transform.position.z * Mathf.Sin(xr.transform.eulerAngles.y * Mathf.Deg2Rad) - (xr.transform.position.x * Mathf.Cos(xr.transform.eulerAngles.y * Mathf.Deg2Rad) - xr.transform.position.z * Mathf.Sin(xr.transform.eulerAngles.y * Mathf.Deg2Rad));
                py = tagground_clone.transform.position.y;
                pz = gameObject.transform.position.x * Mathf.Sin(xr.transform.eulerAngles.y * Mathf.Deg2Rad) + gameObject.transform.position.z * Mathf.Cos(xr.transform.eulerAngles.y * Mathf.Deg2Rad) - (xr.transform.position.x * Mathf.Sin(xr.transform.eulerAngles.y * Mathf.Deg2Rad) + xr.transform.position.z * Mathf.Cos(xr.transform.eulerAngles.y * Mathf.Deg2Rad));
            }
            else
            {
                px = gameObject.transform.position.x;
                py = gameObject.transform.position.y;
                pz = gameObject.transform.position.z;
            }
            Vector3 fixpos = new Vector3(px, py, pz);
            return fixpos;
        }
        protected Quaternion FixXRRotation()
        {
            CloneGamGround();
            if (xr != null)
            {
                rx = tagground_clone.transform.rotation.x;
                ry = tagground_clone.transform.rotation.y;
                rz = tagground_clone.transform.rotation.z;
                rw = tagground_clone.transform.rotation.w;
            }
            else
            {
                rx = gameObject.transform.rotation.x;
                ry = gameObject.transform.rotation.y;
                rz = gameObject.transform.rotation.z;
                rw = gameObject.transform.rotation.w;
            }
            Quaternion fixrot = new Quaternion(rx, ry, rz, rw);
            return fixrot;
        }
        /// <summary>
        /// Correct the coordinate information.
        /// </summary>
        private void CloneGamGround()
        {
            if (xr == null)
            {
                xr = FindObjectOfType<XROrigin>();
            }
            _tagGoManager = TagGoManager.Instance;
            if (_tagGoManager == null)
            {
                GameObject tGO = new GameObject("_TagGoManager");
                tGO.AddComponent<TagGoManager>();
                _tagGoManager = tGO.GetComponent<TagGoManager>();
            }

            if (tagground_clone == null)
            {
                tagground_clone = new GameObject();
                tagground_clone.transform.parent = _tagGoManager.transform;
            }
            tagground_clone.transform.localPosition = gameObject.transform.position;
            tagground_clone.transform.localRotation = gameObject.transform.rotation;
            tagground_clone.name = gameObject.name + "_tagground_clone(clone)";
        }

        /// <summary>
        /// Plot the axes
        /// </summary>
        /// <param name="size"></param>
        protected void DrawDebugView(float size)
        {

            RxDraw.DrawWirePlane(transform.position, transform.rotation, size, size, Color.green);
            if (xr != null)
            {
                Quaternion textRotation = Quaternion.LookRotation(transform.position - xr.Camera.transform.position);
                textRotation = textRotation.PitchNYaw();

                string debugTxt = trackID.ToString();

                RxDraw.Text3D(transform.position, textRotation, 0.02f, debugTxt, Color.green);
            }
            else
            {
                Quaternion textRotation = Quaternion.LookRotation(transform.position - FindObjectOfType<Camera>().transform.position);
                textRotation = textRotation.PitchNYaw();

                string debugTxt = trackID.ToString();

                RxDraw.Text3D(transform.position, textRotation, 0.02f, debugTxt, Color.green);
            }

            RxDraw.DrawTranslateGizmos(transform.position, transform.rotation, size * 0.85f);
        }
        /// <summary>
        /// Get the Tag tracking status
        /// </summary>
        /// <returns></returns>
        protected bool IsTracking()
        {
            if (TagProfileLoading.Instance.TrackingTagList.Contains(trackID))
            {
                return true;
            }
            else return false;
        }
        /// <summary>
        /// First identification
        /// </summary>
        /// <returns></returns>
        protected bool OnFirstTrackingEnter()
        {
            if (isTrakingState && first == false)
            {
                first = true;
                onfirstTrackingEnter = true;
            }
            else if (isTrakingState && first == true)
            {
                onfirstTrackingEnter = false;
            }
            return onfirstTrackingEnter;
        }
        /// <summary>
        /// Identify
        /// </summary>
        /// <returns></returns>
        protected bool OnTrackingEnter()
        {
            if (isTrakingState && onTrackingEnter == false && onTrackingStay == false)
            {
                onTrackingEnter = true;
                onTrackingStay = true;
            }
            else
            {
                onTrackingEnter = false;
            }
            return onTrackingEnter;
        }
        /// <summary>
        /// Identifying
        /// </summary>
        /// <returns></returns>
        protected bool OnTrackingStay()
        {
            if (isTrakingState)
            {
                onTrackingStay = true;
                exit = true;
            }
            else
            {
                onTrackingStay = false;
            }
            return onTrackingStay;
        }
        /// <summary>
        /// lose
        /// </summary>
        /// <returns></returns>
        protected bool OnTrackingExit()
        {
            if (isTrakingState == false && exit)
            {
                exit = false;
                onTrackingExit = true;
            }
            else
            {
                onTrackingExit = false;
            }
            return onTrackingExit;
        }
        /// <summary>
        /// Whether the tag is valid
        /// </summary>
        /// <returns></returns>
        protected bool IsValid()
        {
            if (trackID ==TagProfileLoading.Instance.TagFusion)
            {
                isvalid = true;
            }
            else
            {
                isvalid = false;
            }
            return isvalid;
        }
        /// <summary>
        /// Displays the axis model
        /// </summary>
        protected void DisplaysAxisModel()
        {
            if (m_debugView && IsValid())
            {
                if (debugaxis == null)
                {
                    debugaxis = GameObject.Instantiate(Resources.Load("Tag/Prefabs/Debug Axis")) as GameObject;
                    debugaxis.transform.parent = gameObject.transform;
                    debugaxis.transform.localPosition = new Vector3(0, 0, 0);
                    debugaxis.transform.localEulerAngles = new Vector3(0, 0, 0);
                    debugaxis.transform.localScale = new Vector3(m_size, m_size, m_size);
                }
                debugaxis.SetActive(true);
            }
            else
            {
                if (debugaxis != null)
                {
                    debugaxis.SetActive(false);
                }
            }
        }

        #endregion
    }
}

