using System.Collections;
using System.Collections.Generic;
using Ximmerse.XR.UnityNetworking;
using UnityEngine;
using System;
using UnityEngine.SpatialTracking;
using UnityEngine.XR;
using I_TiNetNode = Ximmerse.XR.UnityNetworking.I_TiNetNode;
using TiNet = Ximmerse.XR.UnityNetworking.TiNet;

namespace Ximmerse.XR
{
    /// <summary>
    /// Sync Controller - 同步其他用户的手部位置信息。
    /// </summary>
    public class SyncController : TiNetMonoBehaviour
    {
        /// <summary>
        /// Key = owner TiNet node ID.
        /// Value = Controller referenence.
        /// </summary>
        static Dictionary<int, SyncControllerInfo> IDControllerMap = new Dictionary<int, SyncControllerInfo>();

        /// <summary>
        /// 是否RhinoX App 场景 ？
        /// 对于编译RhinoX app 应用， 选择True， 对于其他应用，如MR ， 选择False
        /// </summary>
        [SerializeField]
        [Tooltip("是否RhinoX App 场景 ？\r\n对于编译RhinoX app 应用， 选择True， 对于其他应用，如MR ， 选择False。")]
        bool IsRhinoXApp = true;

        /// <summary>
        /// Player hand left/right prefab - 同步其他用户的手部模式prefab。
        /// </summary>
        [SerializeField]
        GameObject PlayerHandPrefabLeft, PlayerHandPrefabRight;

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

        Pose controllerWorldPoseL;

        Pose controllerWorldPoseR;

        bool isLeftTracked, isRightTracked;

        static internal List<XRNodeState> nodeStates = new List<XRNodeState>();

        public static SyncController instance
        {
            get; private set;
        }

        internal class SyncControllerInfo
        {
            internal float LastSyncTime;
            internal Transform ControllerTransformLeft;
            internal Transform ControllerTransformRight;
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
                controllerWorldPoseL = Pose.identity;
                controllerWorldPoseR = Pose.identity;
                isLeftTracked = false;
                isRightTracked = false;
                foreach (XRNodeState nodeState in nodeStates)
                {
                    if (nodeState.nodeType == XRNode.LeftHand)
                    {
                        isLeftTracked = true;
                        if (nodeState.TryGetPosition(out controllerWorldPoseL.position))
                        {
                            controllerWorldPoseL.position = mainCamera.transform.parent.TransformPoint(controllerWorldPoseL.position);
                            retData |= PoseDataFlags.Position;
                        }
                        if (nodeState.TryGetRotation(out controllerWorldPoseL.rotation))
                        {
                            controllerWorldPoseL.rotation = mainCamera.transform.parent.TransformRotation(controllerWorldPoseL.rotation);
                            retData |= PoseDataFlags.Rotation;
                        }
                    }

                    if (nodeState.nodeType == XRNode.RightHand)
                    {
                        isRightTracked = true;
                        if (nodeState.TryGetPosition(out controllerWorldPoseR.position))
                        {
                            controllerWorldPoseR.position = mainCamera.transform.parent.TransformPoint(controllerWorldPoseR.position);
                            retData |= PoseDataFlags.Position;
                        }
                        if (nodeState.TryGetRotation(out controllerWorldPoseR.rotation))
                        {
                            controllerWorldPoseR.rotation = mainCamera.transform.parent.TransformRotation(controllerWorldPoseR.rotation);
                            retData |= PoseDataFlags.Rotation;
                        }
                    }
                }
            }
#endif
        }

        /// <summary>
        /// Event : on sync transform is changed.
        /// Parameter : left and right sync hand transform
        /// </summary>
        public static event Action<GameObject, GameObject> OnSyncTransformChanged = null;

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
            SyncControllerMessage syncControllerMessage = TiNetUtility.GetMessage<SyncControllerMessage>();
            syncControllerMessage.OwnerID = TiNetManager.NodeID;
            syncControllerMessage.TimeTicks = DateTime.Now.Ticks;
            syncControllerMessage.WorldPositionL = controllerWorldPoseL.position;
            syncControllerMessage.WorldRotationL = controllerWorldPoseL.rotation;
            syncControllerMessage.WorldPositionR = controllerWorldPoseR.position;
            syncControllerMessage.WorldRotationR = controllerWorldPoseR.rotation;
            syncControllerMessage.LeftTracked = this.isLeftTracked;
            syncControllerMessage.RightTracked = this.isRightTracked;
            if (node == null)
            {
                syncControllerMessage.SendToAllUnreliable();
            }
            else
            {
                syncControllerMessage.SendToReliable(node);
            }

            this.m_SyncTimeStamp = syncControllerMessage.TimeTicks;
        }

        protected override void TiNet_OnNodeDisconnected(I_TiNetNode node)
        {

        }

        List<int> IDs_Timeout = new List<int>();

        private void FixedUpdate()
        {
#if UNITY_ANDROID
            UnityEngine.Profiling.Profiler.BeginSample("SyncController.part1");
            //如果 SyncIdentity 处于 Owned 状态:
            if (IsRhinoXApp && (isLeftTracked || isRightTracked) && (Time.realtimeSinceStartup - m_LastSyncTime) >= SyncInterval)
            {
                RefreshTrackState();
                SendSyncMessage(null);
                m_LastSyncTime = Time.realtimeSinceStartup;
            }
            UnityEngine.Profiling.Profiler.EndSample();
#endif

            UnityEngine.Profiling.Profiler.BeginSample("SyncController.part2");
            IDs_Timeout.Clear();
            foreach (var pair in IDControllerMap)
            {
                if ((Time.realtimeSinceStartup - pair.Value.LastSyncTime) >= DestroyInterval)
                {
                    IDs_Timeout.Add(pair.Key);
                }
            }
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("SyncController.part3");
            if (IDs_Timeout.Count > 0)
            {
                for (int i = 0; i < IDs_Timeout.Count; i++)
                {
                    int id = IDs_Timeout[i];
                    if (IDControllerMap.TryGetValue(id, out SyncControllerInfo info))
                    {
                        if (info.ControllerTransformLeft != null)
                            Destroy(info.ControllerTransformLeft.gameObject);
                        if (info.ControllerTransformRight != null)
                            Destroy(info.ControllerTransformRight.gameObject);
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
        [TiNetMessageCallback(MessageCode.kSyncController)]
        static void OnSyncControllerMessage(TiNetMessage message, I_TiNetNode node)
        {
            SyncControllerMessage syncControllerMsg = message as SyncControllerMessage;

            if (!IDControllerMap.TryGetValue(syncControllerMsg.OwnerID, out SyncControllerInfo syncControllerInfo))
            {
                syncControllerInfo = new SyncControllerInfo();
                IDControllerMap.Add(syncControllerMsg.OwnerID, syncControllerInfo);
            }

            var Instance = SyncController.instance;

            //Create left hand instance:
            if (syncControllerMsg.LeftTracked && syncControllerInfo.ControllerTransformLeft == null)
            {
                syncControllerInfo.ControllerTransformLeft = Instantiate(Instance.PlayerHandPrefabLeft).transform;
            }
            //Create right hand instance:
            if (syncControllerMsg.RightTracked && syncControllerInfo.ControllerTransformRight == null)
            {
                syncControllerInfo.ControllerTransformRight = Instantiate(Instance.PlayerHandPrefabRight).transform;
            }

            //禁用 non track的本地同步控制器实例对象:
            if (!syncControllerMsg.LeftTracked)
            {
                if (SyncController.IDControllerMap.TryGetValue(node.NodeID, out SyncControllerInfo syncCtrlInfoL) && syncCtrlInfoL.ControllerTransformLeft && syncCtrlInfoL.ControllerTransformLeft.gameObject.activeSelf)
                {
                    syncCtrlInfoL.ControllerTransformLeft.gameObject.SetActive(false);
                }
            }
            else
            {
                if (SyncController.IDControllerMap.TryGetValue(node.NodeID, out SyncControllerInfo syncCtrlInfoL) && syncCtrlInfoL.ControllerTransformLeft && !syncCtrlInfoL.ControllerTransformLeft.gameObject.activeSelf)
                {
                    syncCtrlInfoL.ControllerTransformLeft.gameObject.SetActive(true);
                }
            }

            if (!syncControllerMsg.RightTracked)
            {
                if (SyncController.IDControllerMap.TryGetValue(node.NodeID, out SyncControllerInfo syncCtrlInfoR) && syncCtrlInfoR.ControllerTransformRight && syncCtrlInfoR.ControllerTransformRight.gameObject.activeSelf)
                {
                    syncCtrlInfoR.ControllerTransformRight.gameObject.SetActive(false);
                }
            }
            else
            {
                if (SyncController.IDControllerMap.TryGetValue(node.NodeID, out SyncControllerInfo syncCtrlInfoR) && syncCtrlInfoR.ControllerTransformRight && !syncCtrlInfoR.ControllerTransformRight.gameObject.activeSelf)
                {
                    syncCtrlInfoR.ControllerTransformRight.gameObject.SetActive(true);
                }
            }

            //update sync time.
            syncControllerInfo.LastSyncTime = Time.realtimeSinceStartup;
            Transform localInstanceL = syncControllerInfo.ControllerTransformLeft.transform;
            localInstanceL.SetPositionAndRotation(syncControllerMsg.WorldPositionL, syncControllerMsg.WorldRotationL);

            Transform localInstanceR = syncControllerInfo.ControllerTransformRight.transform;
            localInstanceR.SetPositionAndRotation(syncControllerMsg.WorldPositionR, syncControllerMsg.WorldRotationR);

            OnSyncTransformChanged?.Invoke(localInstanceL.gameObject, localInstanceR.gameObject);

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
                return isLeftTracked || isRightTracked;
            }

        }
    }
}

