using UnityEngine;
using Ximmerse.XR.Utils;
using Ximmerse.XR.Internal;
using System.Collections;
using System.Linq;

namespace Ximmerse.XR.Tag
{
    /// <summary>
    /// Implement tracing capabilities and handle appropriate events.
    /// </summary>
    public class XRTracking : MonoBehaviour
    {
        #region Property
        TrackingManager trackingManager;
        GameObject tracking_clone;
        private long predTimestampNano = 0;
        private Vector3 poslost;
        GameObject followGo;
        Quaternion rotlost;
        TagTracking tagTracking;
        private TrackingEvent trackingEvent;
        private bool first = false;
        private bool exit = false;
        private bool onfirstTrackingEnter = false;

        private bool onTrackingEnter = false;

        private bool onTrackingStay = false;

        private bool onTrackingExit = false;

        private bool trackingstate;
        int index = 0;
        long timestamp = 0;
        int state = 0;
        float posX = 0;
        float posY = 0;
        float posZ = 0;
        float rotX = 0;
        float rotY = 0;
        float rotZ = 0;
        float rotW = 0;
        float confidence = 0;
        float marker_distance = 0;
        #endregion

        #region Unity
        private void Start()
        {
            CloneTagTracking();
            trackingEvent = gameObject.GetComponent<TrackingEvent>();
        }

        private void Update()
        {
            tagTracking = GetComponent<TagTracking>();
            TagTracking();
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
        }
        #endregion

        #region Method
        /// <summary>
        /// Correct the coordinate information.
        /// </summary>
        private void CloneTagTracking()
        {
            trackingManager = TrackingManager.Instance;
            if (trackingManager == null)
            {
                trackingManager = new GameObject("TagTracking Manager").AddComponent<TrackingManager>();
            }
            tracking_clone = new GameObject(gameObject.name);
            tracking_clone.transform.parent = trackingManager.transform;
        }

        /// <summary>
        /// Tag Tracking function
        /// </summary>
        private void TagTracking()
        {
#if !UNITY_EDITOR
            NativePluginApi.Unity_getTagTracking2(tagTracking.TrackId,
                ref index, ref timestamp, ref state, ref posX, ref posY, ref posZ,
                ref rotX, ref rotY, ref rotZ, ref rotW,
                ref confidence, ref marker_distance);
#endif
            if (TagProfileLoading.Instance!=null)
            {
                trackingstate = IsTracking();
                if (trackingstate)
                {
                    poslost = Vector3.zero;
                    rotlost = new Quaternion(0, 0, 0, 1);
                    tracking_clone.transform.localPosition = new Vector3(posX, posY, posZ);
                    tracking_clone.transform.localRotation = new Quaternion(rotX, rotY, rotZ, rotW);
                    gameObject.transform.position = tracking_clone.transform.position;
                    gameObject.transform.rotation = tracking_clone.transform.rotation;
                    if (tagTracking.DebugView)
                    {
                        DrawDebugView(tagTracking.Size);
                    }
                }
                else
                {
                    if (tagTracking.TrackingIsLost == LostState.FollowHead)
                    {
                        if (poslost == Vector3.zero && rotlost == new Quaternion(0, 0, 0, 1))
                        {
                            poslost = gameObject.transform.position;
                            rotlost = gameObject.transform.rotation;
                            if (followGo == null)
                            {
                                followGo = new GameObject();
                            }
                            followGo.transform.position = poslost;
                            followGo.transform.rotation = rotlost;
                            if (trackingManager.xROrigin != null)
                            {
                                followGo.transform.parent = trackingManager.xROrigin.Camera.transform;
                            }
                            else
                            {
                                followGo.transform.parent = FindObjectOfType<Camera>().transform;
                                throw new System.Exception("XR Origin is not found");
                            }
                        }
                        gameObject.transform.position = followGo.transform.position;
                        gameObject.transform.rotation = followGo.transform.rotation;
                    }
                }
            }
        }

        /// <summary>
        /// Plot the axes
        /// </summary>
        /// <param name="size"></param>
        private void DrawDebugView(float size)
        {
            RxDraw.DrawWirePlane(transform.position, transform.rotation, size, size, Color.green);
            if (trackingManager.xROrigin != null)
            {
                var textRotation = Quaternion.LookRotation(transform.position - trackingManager.xROrigin.Camera.transform.position);
                textRotation = textRotation.PitchNYaw();
                string debugTxt = tagTracking.TrackId.ToString();
                RxDraw.Text3D(transform.position, textRotation, 0.02f, debugTxt, Color.green);
            }
            else
            {
                var textRotation = Quaternion.LookRotation(transform.position - FindObjectOfType<Camera>().transform.position);
                textRotation = textRotation.PitchNYaw();
                string debugTxt = tagTracking.TrackId.ToString();
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
            if (TagProfileLoading.Instance.TrackingTagList.Contains(tagTracking.TrackId))
            {
                return true;
            }
            else return false;
        }
        /// <summary>
        /// First identification
        /// </summary>
        /// <returns></returns>
        private bool OnFirstTrackingEnter()
        {
            if (trackingstate && first == false)
            {
                first = true;
                onfirstTrackingEnter = true;
            }
            else if (trackingstate && first == true)
            {
                onfirstTrackingEnter = false;
            }
            return onfirstTrackingEnter;
        }
        /// <summary>
        /// Identify
        /// </summary>
        /// <returns></returns>
        private bool OnTrackingEnter()
        {
            if (trackingstate && onTrackingEnter == false && onTrackingStay == false)
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
        private bool OnTrackingStay()
        {
            if (trackingstate)
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
        private bool OnTrackingExit()
        {
            if (trackingstate == false && exit)
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
        #endregion
    }
}

