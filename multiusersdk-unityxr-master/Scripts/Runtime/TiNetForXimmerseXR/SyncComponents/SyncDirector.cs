using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Ximmerse.XR.UnityNetworking;

namespace Ximmerse.XR
{
    /// <summary>
    /// Sync director playing.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SyncIdentity))]
    public class SyncDirector : TiNetMonoBehaviour
    {
        /// <summary>
        /// Assumes the sync identity's target game object contains a director component.
        /// </summary>
        PlayableDirector director;

        const float kSyncTimeDistance = 0.5f;

        const float kSyncTimeInterval = 0.5f;

        float m_PreviousSyncTime = 0;

        protected override void Awake()
        {
            base.Awake();
            try
            {
                director = this.SyncIdentity.TargetGameObject.GetComponent<PlayableDirector>();
            }
            catch (System.Exception exc)
            {
                Debug.LogException(exc);
                Debug.LogErrorFormat(gameObject, "Fail to obtain a playable director from : {0}", name);
            }
        }

        private void FixedUpdate()
        {
            if(TiNet.Instance.IsNetworkStarted &&
              (Time.realtimeSinceStartup - m_PreviousSyncTime) >= kSyncTimeInterval)
            {
                m_PreviousSyncTime = Time.realtimeSinceStartup;
                SendSyncMessage();
            }
        }

        /// <summary>
        /// 发送同步director消息。
        /// </summary>
        void SendSyncMessage ()
        {
            SyncDirectorMessage msg = TiNetUtility.GetMessage<SyncDirectorMessage>();
            msg.DirectorTime = (float)director.time;
            msg.State = director.state;
            msg.SystemTime = TiNet.Instance.NetworkStartTime.Value.Ticks;
            msg.NetworkID = this.SyncIdentity.NetworkID;
            m_PreviousSyncTime = Time.realtimeSinceStartup;
            msg.SendToAllUnreliable();
        }

        /// <summary>
        /// Message callback : on transform message received.
        /// </summary>
        [TiNetMessageCallback(MessageCode.kSyncDirector)]
        static void OnSyncDirectorMessage(TiNetMessage message, I_TiNetNode node)
        {
            SyncDirectorMessage syncDirectorMsg = message as SyncDirectorMessage;
            //Debug.LogFormat("On msg : {0}, owner id: {1}", syncDirectorMsg.DirectorTime, syncDirectorMsg.NetworkID);
            if (Ximmerse.XR.SyncIdentity.Get(syncDirectorMsg.NetworkID, out SyncIdentity entity))
            {
                var peerNodeSystemTime = syncDirectorMsg.SystemTime;//其他节点的消息时间。
                //只同步向早于自己加入TiNet 连接圈的消息：
                if (peerNodeSystemTime < TiNet.Instance.NetworkStartTime.Value.Ticks)
                {
                    var director = entity.targetGameObject.GetComponent<PlayableDirector>();
                    float peerDirectorTime = syncDirectorMsg.DirectorTime;
                    //只有当两个 director 时间差大于 kSyncTimeDistance, 或者两者的状态不一样的时候，才实施同步。
                    if (director != null && (director.state != syncDirectorMsg.State ||
                       PEMathf.Distance((float)director.time, peerDirectorTime) > kSyncTimeDistance))
                    {
                        //当处于Playing状态，
                        if (syncDirectorMsg.State == PlayState.Playing)
                        {
                            if (director.state != PlayState.Playing)
                            {
                                director.Play();
                            }
                            director.time = peerDirectorTime;//同步时间;
                        }
                        //其他情况，当两个 director 的 state 不一致的时候:
                        else if (syncDirectorMsg.State == PlayState.Paused && director.state != PlayState.Paused)
                        {
                            director.Stop();
                        }
                        //TODO : handle when peer director state == Delayed.

                    }
                }


            }

        }
    }
}