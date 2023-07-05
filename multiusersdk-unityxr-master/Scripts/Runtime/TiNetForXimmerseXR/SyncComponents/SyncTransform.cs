using System.Collections;
using System.Collections.Generic;
using Ximmerse.XR.UnityNetworking;
using UnityEngine;
using System;
using I_TiNetNode = Ximmerse.XR.UnityNetworking.I_TiNetNode;
using TiNet = Ximmerse.XR.UnityNetworking.TiNet;
using TiNetMessageCallback = Ximmerse.XR.UnityNetworking.TiNetMessageCallbackAttribute;
using TiNetMessage = Ximmerse.XR.UnityNetworking.TiNetMessage;
using TiNetUtility = Ximmerse.XR.UnityNetworking.TiNetUtility;


namespace Ximmerse.XR
{
    /// <summary>
    /// Sync transform
    /// </summary>
    [RequireComponent(typeof(SyncIdentity))]
    public class SyncTransform : TiNetMonoBehaviour
    {
        /// <summary>
        /// 最近一次更新 transform 的时间戳。
        /// </summary>
        long? m_SyncTimeStamp;

        /// <summary>
        /// Sync interval.
        /// </summary>
        [MinValue (0.01f)]
        public float SyncInterval = 0.05f;

        float m_LastSyncTime;

        /// <summary>
        /// 对上一次更新的 Owner ID。
        /// </summary>
        private int m_PreviousOwnerID;

        /// <summary>
        /// 对上一次更新的 Owner ID。
        /// </summary>
        [InspectFunction]
        public int PreviousOwnerID
        {
            get
            {
                return m_PreviousOwnerID;
            }
        }

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

        Transform syncTransform
        {
            get => this.SyncIdentity.TargetGameObject.transform;
        }


        /// <summary>
        /// Event : on sync transform is changed.
        /// Parameter : the changed gameobject's.
        /// </summary>
        public event Action<GameObject> OnSyncTransformChanged = null;

        protected override void TiNet_OnNodeConnected(Ximmerse.XR.UnityNetworking.I_TiNetNode node)
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
        private void SendSyncMessage(Ximmerse.XR.UnityNetworking.I_TiNetNode node)
        {
            SyncTransformMessage syncTransMessage = TiNetUtility.GetMessage<SyncTransformMessage>();
            syncTransMessage.NetworkID = this.SyncIdentity.NetworkID;
            syncTransMessage.OwnerID = TiNetManager.NodeID;
            syncTransMessage.TimeTicks = DateTime.Now.Ticks;
            syncTransMessage.WorldPosition = syncTransform.position;
            syncTransMessage.WorldRotation = syncTransform.rotation;
            syncTransMessage.Scale = syncTransform.localScale;
            if(node == null)
            {
                syncTransMessage.SendToAllUnreliable();
            }
            else
            {
                syncTransMessage.SendToReliable(node);
            }

            this.m_SyncTimeStamp = syncTransMessage.TimeTicks;
        }

        protected override void TiNet_OnNodeDisconnected(I_TiNetNode node)
        {

        }

        private void FixedUpdate()
        {
            //如果 SyncIdentity 处于 Owned 状态:
            if (this.SyncIdentity.IsOwned)
            {
                //Debug.LogFormat("Transform has change: {0}, time-interval: {1}", syncTransform.hasChanged, (Time.realtimeSinceStartup - m_LastSyncTime));
                if(TransformDirty() && (Time.realtimeSinceStartup - m_LastSyncTime) >= SyncInterval)
                {
                    SendSyncMessage(null);
                    m_LastSyncTime = Time.realtimeSinceStartup;
                }
            }
        }

        /// <summary>
        /// Message callback : on transform message received.
        /// </summary>
        [TiNetMessageCallbackAttribute(MessageCode.kSyncTransform)]
        static void OnSyncTransformMessage(TiNetMessage message, I_TiNetNode node)
        {
            SyncTransformMessage syncTransMsg = message as SyncTransformMessage;
            int networkID = syncTransMsg.NetworkID;
            //Debug.LogFormat("On sync transform message : {0}", networkID);
            //同步 sync transform 信息:
            if(Ximmerse.XR.SyncIdentity.Get(networkID, out SyncIdentity entity) && entity != null && entity.IsOwned == false && entity.HasComponent(out SyncTransform syncT))
            {
                long syncTime = syncTransMsg.TimeTicks;
                //只取时间戳最新的更新:
                if(syncT.m_SyncTimeStamp.HasValue == false || syncT.m_SyncTimeStamp.Value < syncTime)
                {
                    syncT.m_PreviousOwnerID = syncTransMsg.OwnerID;
                    syncT.m_SyncTimeStamp = syncTransMsg.TimeTicks;

                    Transform t = entity.TargetGameObject.transform;
                    t.position = syncTransMsg.WorldPosition;
                    t.rotation = syncTransMsg.WorldRotation;
                    t.localScale = syncTransMsg.Scale;

                    syncT.OnSyncTransformChanged?.Invoke(t.gameObject);
                }
            }
        }

        bool TransformDirty()
        {
            if(Application.isMobilePlatform)
            {
                //Android always return transform changed == FALSE
                return true;
            }
            else
            {
                bool dirty = syncTransform.hasChanged;
                //Check transform dirty:
                if (dirty)
                {
                    syncTransform.hasChanged = false;
                }
                return dirty;
            }
         
        }
    }
}
