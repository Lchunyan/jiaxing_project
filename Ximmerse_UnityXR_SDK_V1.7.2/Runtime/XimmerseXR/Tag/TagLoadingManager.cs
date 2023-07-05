using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.XR.CoreUtils;
using UnityEngine;
using Ximmerse.XR.Internal;
using Ximmerse.XR.Utils;
using static Ximmerse.XR.PluginVioFusion;

namespace Ximmerse.XR.Tag
{
    public class TagLoadingManager : MonoBehaviour
    {
        #region Property
        private bool Fixstart = false;
        private Vector3 XRpos;
        private XROrigin xROrigin;
        private float xrx, xry, xrz;
        protected Thread threadLoad;
        private List<GameObject> _groundplanelist = new List<GameObject>();
        private List<TagGroundPlane> _debugTagGround = new List<TagGroundPlane>();
        private List<int> _trackingtaglist = new List<int>();

        int beacon_id;
        long beacon_timestamp;
        float beacon_pos0;
        float beacon_pos1;
        float beacon_pos2;
        float beacon_rot0;
        float beacon_rot1;
        float beacon_rot2;
        float beacon_rot3;
        float beacon_tracking_confidence;
        float beacon_min_distance;
        float beacon_correct_weight;

        private int _tagfusion;
        private long predTimestampNano = 0;
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
        bool trakingstate;
        //private XDevicePlugin.XAttrTrackingInfo trackingInfo;

        public List<TagGroundPlane> DebugTagGroundList
        {
            get => _debugTagGround;
            set => _debugTagGround = value;
        }
        public List<GameObject> GroundPlaneList
        {
            get => _groundplanelist;
            set => _groundplanelist = value;
        }
        public List<int> TrackingTagList
        {
            get => _trackingtaglist;
        }
        public int TagFusion
        {
            get => _tagfusion;
        }
        public Thread ThreadLoad
        {
            get => threadLoad;
        }
        #endregion

        #region Unity

        private void Update()
        {
            GetTrackingStateAndData();
        }
#endregion

        #region Method
        /// <summary>
        /// Get the coordinates of the Tag ground plane in the scene and enable large space positioning
        /// </summary>
        private void GroundPlaneStart()
        {
#if !UNITY_EDITOR
            PluginVioFusion.plugin_vio_fusion_reset(0);
            //TagGroundPlane[] enemies = UnityEngine.Object.FindObjectsOfType<TagGroundPlane>();
            if (TagGroundPlane.tagGroundPlaneList != null)
            {
                foreach (var item in TagGroundPlane.tagGroundPlaneList)
                {
                    PluginVioFusion.XAttrBeaconInWorldInfo beacon_in_world_info = item.beacon_info;
                    Debug.Log("id£º" + beacon_in_world_info.beacon_id);
                    PluginVioFusion.plugin_vio_fusion_set_param(ref beacon_in_world_info);
                }
                PluginVioFusion.plugin_vio_fusion_run(0);
            }
#endif
        }
        /// <summary>
        /// Get the coordinates of the Tag ground plane in the text and enable large space positioning
        /// </summary>
        /// <param name="go"></param>
        private void GroundPlaneStart(List<GameObject> go)
        {
#if !UNITY_EDITOR
            PluginVioFusion.plugin_vio_fusion_reset(0);
            if (go != null)
            {
                foreach (var item in go)
                {
                    PluginVioFusion.XAttrBeaconInWorldInfo beacon_in_world_info = item.GetComponent<TagGroundPlane>().beacon_info;
                    Debug.Log(beacon_in_world_info.beacon_id);
                    PluginVioFusion.plugin_vio_fusion_set_param(ref beacon_in_world_info);
                }
                PluginVioFusion.plugin_vio_fusion_run(0);
            }
#endif
        }
        /// <summary>
        /// Start enabling the Large Space component
        /// </summary>
        protected IEnumerator StartFusion()
        {
            while (threadLoad ==null || threadLoad.ThreadState==ThreadState.Running)
            {
                yield return null;
            }
            xROrigin = GameObject.FindObjectOfType<XROrigin>();
            if (xROrigin != null)
            {
                xrx = xROrigin.transform.position.x;
                xry = xROrigin.transform.position.y;
                xrz = xROrigin.transform.position.z;
                XRpos = new Vector3(xrx, xry, xrz);
                StartCoroutine(FixTransform());
                Fixstart = true;
            }
            if (CreatesGroundPlaneByJson.Instance == null)
            {
                SettingTagData();
            }
            else
            {
                if (!CreatesGroundPlaneByJson.Instance.autoCreates)
                {
                    SettingTagData();
                }
            }
        }

        /// <summary>
        /// Correct large spatial coordinates
        /// </summary>
        /// <returns></returns>
        IEnumerator FixTransform()
        {
            while (true)
            {
                if (XRpos != xROrigin.transform.position)
                {
                    SettingTagData();
                    xrx = xROrigin.transform.position.x;
                    xry = xROrigin.transform.position.y;
                    xrz = xROrigin.transform.position.z;
                    XRpos = new Vector3(xrx, xry, xrz);
                }
                yield return new WaitForSeconds(1);
            }

        }
        /// <summary>
        /// Refresh and re-acquire the coordinate information of the large spatial positioning board.
        /// </summary>
        protected void RefreshBeacon()
        {
            if (Fixstart)
            {
               StopCoroutine(FixTransform());
            }
            SettingTagData();
            xrx = xROrigin.transform.position.x;
            xry = xROrigin.transform.position.y;
            xrz = xROrigin.transform.position.z;
            XRpos = new Vector3(xrx, xry, xrz);
            StartCoroutine(FixTransform());
            Fixstart = true;
        }
        /// <summary>
        /// Clearing the algorithm data invalidates the large spatial positioning function.
        /// </summary>
        protected void CleanBeacon()
        {
#if !UNITY_EDITOR
            PluginVioFusion.plugin_vio_fusion_reset(0);
#endif
        }

        /// <summary>
        /// Setting Tag Data
        /// </summary>
        protected void SettingTagData()
        {
            if (CreatesGroundPlaneByJson.Instance != null)
            {
                if (CreatesGroundPlaneByJson.Instance.autoCreates)
                {
                    GroundPlaneStart(GroundPlaneList);
                }
                else
                {
                    GroundPlaneStart();
                }
            }
            else
            {
                GroundPlaneStart();
            }
        }

        /// <summary>
        /// Get the tracking status and the ID of the fusion
        /// </summary>
        protected void GetTrackingStateAndData()
        {
#if !UNITY_EDITOR
            int ret = NativePluginApi.Unity_TagPredict(0);
            _trackingtaglist.Clear();
            if (ret >= 0)
            {
                for (int i = 0; i <= 227; i++)
                {
                    bool ret2 = NativePluginApi.Unity_getTagTracking2(i,
                          ref index, ref timestamp, ref state,
                          ref posX, ref posY, ref posZ,
                          ref rotX, ref rotY, ref rotZ, ref rotW,
                          ref confidence, ref marker_distance);

                    if (ret2 && state > 0)
                    {
                        _trackingtaglist.Add(i);
                    }
                }
            }
#endif
            _tagfusion = GetTagFusionState();
        }

        /// <summary>
        /// Gets a valid Tag ID
        /// </summary>
        /// <returns></returns>
        protected int GetTagFusionState()
        {
#if !UNITY_EDITOR
            NativePluginApi.Unity_getFusionResult(predTimestampNano, ref beacon_id,
                            ref beacon_timestamp,
                            ref beacon_pos0,
                            ref beacon_pos1,
                            ref  beacon_pos2,
                            ref  beacon_rot0,
                            ref  beacon_rot1,
                            ref  beacon_rot2,
                            ref  beacon_rot3,
                            ref  beacon_tracking_confidence,
                            ref  beacon_min_distance,
                            ref  beacon_correct_weight);
#endif
            return beacon_id;
        }
#endregion
    }
}
