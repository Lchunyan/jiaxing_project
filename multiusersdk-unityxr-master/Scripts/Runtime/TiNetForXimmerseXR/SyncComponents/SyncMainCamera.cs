using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SpatialTracking;
using UnityEngine.XR;
using I_TiNetNode = Ximmerse.XR.UnityNetworking.I_TiNetNode;
using TiNet = Ximmerse.XR.UnityNetworking.TiNet;
using Ximmerse.XR.UnityNetworking;
namespace Ximmerse.XR
{
    /// <summary>
    /// Sync MainCamera - 同步其他用户的Main camera位置信息。
    /// </summary>
    public class SyncMainCamera : TiNetMonoBehaviour
    {
        /// <summary>
        /// Key = owner TiNet node ID.
        /// Value = main camera referenence.
        /// </summary>
        static Dictionary<int, SyncARCameraInfo> IDControllerMap = new Dictionary<int, SyncARCameraInfo>();

        /// <summary>
        /// 是否RhinoX App 场景 ？
        /// 对于编译RhinoX app 应用， 选择True， 对于其他应用，如MR ， 选择False
        /// </summary>
        [SerializeField]
        [Tooltip("是否RhinoX App 场景 ？\r\n对于编译RhinoX app 应用， 选择True， 对于其他应用，如MR ， 选择False。")]
        bool IsRhinoXApp = true;

        /// <summary>
        /// Main camera prefab - 同步其他用户的手部模式prefab。
        /// </summary>
        [SerializeField]
        GameObject MainCameraPrefab;

        /// <summary>
        /// 自动销毁延时。
        /// 在收不到同步用户的手部数据后，自动销毁本地的手部对象。
        /// </summary>
        [SerializeField]
        [Tooltip("自动销毁延时。\r\n在收不到同步用户的手部数据后，自动销毁本地的手部对象。")]
        float DestroyInterval = 10;

        /// <summary>
        /// 最近一次更新 transform 的时间戳。
        /// </summary>
        long? m_SyncTimeStamp;

        /// <summary>
        /// 同步时间间隔.
        /// </summary>
        [MinValue(0.01f)]
        [Tooltip("同步时间间隔。")]
        public float SyncInterval = 0.05f;

        float m_LastSyncTime;

        int OwnerID;

        /// <summary>
        /// 用于标注更新数据的时间戳
        /// </summary>
        [InspectFunction]
        public long SyncTimeStamp
        {
            get
            {
                return m_SyncTimeStamp.HasValue ? m_SyncTimeStamp.Value : 0;
            }
        }

        Pose mainCameraWorldPose;

        bool isTracked;

        static internal List<XRNodeState> nodeStates = new List<XRNodeState>();

        public static SyncMainCamera instance
        {
            get; private set;
        }

        internal class SyncARCameraInfo
        {
            internal float LastSyncTime;
            internal Transform MainCameraDelegate;
        }

        Camera mainCamera;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            instance = null;
        }

        private void Start()
        {
            mainCamera = Camera.main;
            RefreshTrackState();
        }

        /// <summary>
        /// refresh controller tracking state.
        /// </summary>
        void RefreshTrackState()
        {
#if UNITY_ANDROID
            if (IsRhinoXApp && Application.platform == RuntimePlatform.Android)
            {
                UnityEngine.SpatialTracking.PoseDataFlags retData = UnityEngine.SpatialTracking.PoseDataFlags.NoData;
                UnityEngine.XR.InputTracking.GetNodeStates(nodeStates);
                mainCameraWorldPose = Pose.identity;
                isTracked = false;
                foreach (XRNodeState nodeState in nodeStates)
                {
                    if (nodeState.nodeType == XRNode.CenterEye)
                    {
                        isTracked = true;
                        if (nodeState.TryGetPosition(out mainCameraWorldPose.position))
                        {
                            mainCameraWorldPose.position = mainCamera.transform.parent.TransformPoint(mainCameraWorldPose.position);
                            retData |= PoseDataFlags.Position;
                        }
                        if (nodeState.TryGetRotation(out mainCameraWorldPose.rotation))
                        {
                            mainCameraWorldPose.rotation = mainCamera.transform.parent.TransformRotation(mainCameraWorldPose.rotation);
                            retData |= PoseDataFlags.Rotation;
                        }
                    }
                }
            }
#endif
        }

        /// <summary>
        /// Event : on sync transform is changed.
        /// </summary>
        public static event Action<GameObject> OnSyncTransformChanged = null;

        protected override void TiNet_OnNodeConnected(I_TiNetNode node)
        {
            if (m_SyncTimeStamp.HasValue)
            {
                SendSyncMessage(node);
                //Debug.LogFormat(gameObject, "{0} send sync transform message on node connected", name);
            }
        }

        /// <summary>
        /// Sends sync transform message to speicific node , or to all nodes.
        /// </summary>
        /// <param name="node"></param>
        private void SendSyncMessage(I_TiNetNode node)
        {
            SyncMainCameraMessage syncMainCameraMessage = TiNetUtility.GetMessage<SyncMainCameraMessage>();
            syncMainCameraMessage.OwnerID = TiNetManager.NodeID;
            syncMainCameraMessage.TimeTicks = DateTime.Now.Ticks;
            syncMainCameraMessage.WorldPosition = mainCameraWorldPose.position;
            syncMainCameraMessage.WorldRotation = mainCameraWorldPose.rotation;
            syncMainCameraMessage.Tracked = this.isTracked;
            if (node == null)
            {
                syncMainCameraMessage.SendToAllUnreliable();
            }
            else
            {
                syncMainCameraMessage.SendToReliable(node);
            }

            this.m_SyncTimeStamp = syncMainCameraMessage.TimeTicks;
        }

        protected override void TiNet_OnNodeDisconnected(I_TiNetNode node)
        {

        }

        List<int> IDs_Timeout = new List<int>();

        private void FixedUpdate()
        {
#if UNITY_ANDROID
            UnityEngine.Profiling.Profiler.BeginSample("SyncMainCamera.part1");
            //如果 SyncIdentity 处于 Owned 状态:
            if (IsRhinoXApp && isTracked && (Time.realtimeSinceStartup - m_LastSyncTime) >= SyncInterval)
            {
                RefreshTrackState();
                SendSyncMessage(null);
                m_LastSyncTime = Time.realtimeSinceStartup;
            }
            UnityEngine.Profiling.Profiler.EndSample();
#endif

            UnityEngine.Profiling.Profiler.BeginSample("SyncMainCamera.part2");
            IDs_Timeout.Clear();
            foreach (var pair in IDControllerMap)
            {
                if ((Time.realtimeSinceStartup - pair.Value.LastSyncTime) >= DestroyInterval)
                {
                    IDs_Timeout.Add(pair.Key);
                }
            }
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("SyncMainCamera.part3");
            if (IDs_Timeout.Count > 0)
            {
                for (int i = 0; i < IDs_Timeout.Count; i++)
                {
                    int id = IDs_Timeout[i];
                    if (IDControllerMap.TryGetValue(id, out SyncARCameraInfo info))
                    {
                        if (info.MainCameraDelegate != null)
                            Destroy(info.MainCameraDelegate.gameObject);
                        IDControllerMap.Remove(id);
                    }
                    else
                    {
                        IDControllerMap.Remove(id);
                    }
                }
                IDs_Timeout.Clear();
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }

        /// <summary>
        /// Message callback : on transform message received.
        /// </summary>
        [TiNetMessageCallback(MessageCode.kSyncMainCamera)]
        static void OnSyncMainCameraMessage(TiNetMessage message, I_TiNetNode node)
        {
            SyncMainCameraMessage syncCameraMsg = message as SyncMainCameraMessage;

            if (!IDControllerMap.TryGetValue(syncCameraMsg.OwnerID, out SyncARCameraInfo syncCamInfo))
            {
                syncCamInfo = new SyncARCameraInfo();
                IDControllerMap.Add(syncCameraMsg.OwnerID, syncCamInfo);
            }

            var Instance = SyncMainCamera.instance;

            //Create left hand instance:
            if (syncCameraMsg.Tracked && syncCamInfo.MainCameraDelegate == null)
            {
                syncCamInfo.MainCameraDelegate = Instantiate(Instance.MainCameraPrefab).transform;
            }

            //禁用 non track的本地同步控制器实例对象:
            if (!syncCameraMsg.Tracked)
            {
                if (SyncMainCamera.IDControllerMap.TryGetValue(node.NodeID, out SyncARCameraInfo syncInfo) && syncInfo.MainCameraDelegate && syncInfo.MainCameraDelegate.gameObject.activeSelf)
                {
                    syncInfo.MainCameraDelegate.gameObject.SetActive(false);
                }
            }
            else
            {
                if (SyncMainCamera.IDControllerMap.TryGetValue(node.NodeID, out SyncARCameraInfo syncInfo) && syncInfo.MainCameraDelegate && !syncInfo.MainCameraDelegate.gameObject.activeSelf)
                {
                    syncInfo.MainCameraDelegate.gameObject.SetActive(true);
                }
            }


            //update sync time.
            syncCamInfo.LastSyncTime = Time.realtimeSinceStartup;
            Transform localInstance = syncCamInfo.MainCameraDelegate.transform;
            localInstance.SetPositionAndRotation(syncCameraMsg.WorldPosition, syncCameraMsg.WorldRotation);

            OnSyncTransformChanged?.Invoke(localInstance.gameObject);

            //Debug.LogFormat("On sync transform info : {0}", syncControllerMsg.ControllerIndex);
        }

        bool TransformDirty()
        {
            if (Application.isMobilePlatform)
            {
                //Android always return transform changed == FALSE
                return true;
            }
            else
            {
                return isTracked;
            }
        }
    }
}

