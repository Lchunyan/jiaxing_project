using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ximmerse.XR.UnityNetworking;
using System;
using I_TiNetNode = Ximmerse.XR.UnityNetworking.I_TiNetNode;
using TiNet = Ximmerse.XR.UnityNetworking.TiNet;

namespace Ximmerse.XR
{
	/// <summary>
	/// Sync game object's activeness state.
	/// </summary>
	[RequireComponent(typeof(SyncIdentity))]
	public class SyncGameObjectState : TiNetMonoBehaviour
	{
		bool? m_PrevSyncActiveness;

        long? m_SyncTimeStamp;

        bool? prevGameObjectActiveness = false;

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

        /// <summary>
        /// Sends sync transform message to speicific node , or to all nodes.
        /// </summary>
        /// <param name="node"></param>
        private void SendSyncMessage(I_TiNetNode node)
        {
            SyncGameObjectStateMessage syncGameObjectState = TiNetUtility.GetMessage<SyncGameObjectStateMessage>();
            syncGameObjectState.ActiveSelf = this.SyncIdentity.TargetGameObject.activeSelf;
            syncGameObjectState.TimeTicks = DateTime.Now.Ticks;
            syncGameObjectState.NetworkID = this.SyncIdentity.NetworkID;

            m_SyncTimeStamp = syncGameObjectState.TimeTicks;
            m_PrevSyncActiveness = this.SyncIdentity.TargetGameObject.activeSelf;

            if (node == null)
            {
                syncGameObjectState.SendToAllReliable();
            }
            else
            {
                syncGameObjectState.SendToReliable(node);
            }

            //Debug.LogFormat("Send sync game object state message : {0}", syncGameObjectState.ActiveSelf);
        }

        private void Update()
        {
            if(this.SyncIdentity.IsOwned)
            {
                if(prevGameObjectActiveness.HasValue && prevGameObjectActiveness.Value != this.SyncIdentity.TargetGameObject.activeSelf)
                {
                    SendSyncMessage(null);
                }
            }

            prevGameObjectActiveness = SyncIdentity.TargetGameObject.activeSelf; 
        }


        protected override void TiNet_OnNodeConnected(I_TiNetNode node)
        {
            if (m_PrevSyncActiveness.HasValue)
            {
                SendSyncMessage(node);
                //Debug.LogFormat(gameObject, "{0} send sync gameobject state message on node connected", name);
            }
        }


        /// <summary>
        /// Message callback : on game object sync activeness sync
        /// </summary>
        [TiNetMessageCallback(MessageCode.kSyncGameObjectState)]
        static void OnSyncTransformMessage(TiNetMessage message, I_TiNetNode node)
        {
            SyncGameObjectStateMessage syncGameObjectStateMsg = message as SyncGameObjectStateMessage;
            int networkID = syncGameObjectStateMsg.NetworkID;
            //同步 sync gameobject state 信息:
            if (Ximmerse.XR.SyncIdentity.Get(networkID, out SyncIdentity entity) && entity != null && entity.IsOwned == false && entity.HasComponent(out SyncGameObjectState syncGameObjectState))
            {
                long syncTime = syncGameObjectStateMsg.TimeTicks;
                //只取时间戳最新的更新:
                if (syncGameObjectState.m_SyncTimeStamp.HasValue == false || syncGameObjectState.m_SyncTimeStamp.Value < syncTime)
                {
                    entity.targetGameObject.SetActive(syncGameObjectStateMsg.ActiveSelf);
                    syncGameObjectState.m_SyncTimeStamp = syncTime;
                }

            }
        }
    }
}