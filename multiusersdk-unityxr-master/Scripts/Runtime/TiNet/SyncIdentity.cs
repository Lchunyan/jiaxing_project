using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using I_TiNetNode = Ximmerse.XR.UnityNetworking.I_TiNetNode;
using TiNetUtility = Ximmerse.XR.UnityNetworking.TiNetUtility;
using Ximmerse.XR.UnityNetworking;
namespace Ximmerse.XR
{
    /// <summary>
    /// Sync entity.
    /// 代表场景中的一个可同步物体。
    /// SyncIdentity 的 m_NetworkID 在场景中必须唯一。
    /// </summary>
    [DisallowMultipleComponent]
    public class SyncIdentity : TiNetMonoBehaviour, I_SyncIdentity
    {
        /// <summary>
        /// The target game object.
        /// </summary>
        public GameObject targetGameObject;

        /// <summary>
        /// Sync network ID.
        /// </summary>
        [SerializeField]
        int m_NetworkID = -1;

#pragma warning disable CS0108 // 成员隐藏继承的成员；缺少关键字 new
        /// <summary>
        /// Sync network ID.
        /// </summary>s
        public int NetworkID
#pragma warning restore CS0108 // 成员隐藏继承的成员；缺少关键字 new
        {
            get
            {
                return m_NetworkID;
            }
            set
            {
                m_NetworkID = value;
            }
        }

        /// <summary>
        /// The sync entity's target game object.
        /// </summary>
        public GameObject TargetGameObject
        {
            get
            {
                return targetGameObject;
            }
        }

        internal static List<SyncIdentity> entities = new List<SyncIdentity>();

        public static readonly IReadOnlyList<SyncIdentity> Entities;

        internal static Dictionary<int, SyncIdentity> entitiesMap = new Dictionary<int, SyncIdentity>();

        static SyncIdentity()
        {
            Entities = entities;
        }

        /// <summary>
        /// Gets the sync entity ID.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool Get(int ID, out SyncIdentity entity)
        {
            return entitiesMap.TryGetValue(ID, out entity);
        }

        /// <summary>
        /// Gets the sync entity by target game object.
        /// </summary>
        /// <param name="TargetGameObject"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool Get(GameObject TargetGameObject, out SyncIdentity entity)
        {
            entity = null;
            for (int i = 0, SyncIdentityentitiesCount = entities.Count; i < SyncIdentityentitiesCount; i++)
            {
                var id = entities[i];
                if (id.targetGameObject == TargetGameObject)
                {
                    entity = id;
                    return true;
                }
            }
            return false;
        }

        public static int GetCount()
        {
            return entities.Count;
        }


        int m_OwnerID = -1;


        /// <summary>
        /// Event : on ownership claimed by other node.
        /// Parameter :
        /// - Entity
        /// - Net node reference
        /// - Claim/Unclaim
        /// </summary>
        public static event Action<Ximmerse.XR.SyncIdentity, Ximmerse.XR.UnityNetworking.I_TiNetNode, bool> OnClaimOwnershipByOtherNode;

        #region Claim Owner History

        /// <summary>
        /// 最后一次 claim owner 的时间戳。
        /// </summary>
        long LatestClaimOwnerTimeTick;

        /// <summary>
        /// 是否被本地Node claim owner。
        /// </summary>
        bool IsLatestClaimOwnerByLocalNode;

        /// <summary>
        /// 最后一次对此identity宣示主权的 node.
        /// </summary>
        int LatestClaimOwnerNode;

        #endregion


        /// <summary>
        /// Is owner by current node ?
        /// </summary>
        [InspectFunction]
        public bool IsOwned
        {
            get => m_OwnerID == TiNetManager.NodeID;
        }

        /// <summary>
        /// Gets the Owner ID.
        /// </summary>
        [InspectFunction]
        public int OwnerID { get => m_OwnerID; }
        /// <summary>
        /// Is the sync identity currently owned by other node ?
        /// </summary>
        public bool IsOwnedByOtherNode => m_OwnerID != -1 && m_OwnerID != TiNetManager.NodeID;

        /// <summary>
        /// Is the sync identity currently not owned by any node ?
        /// </summary>
        public bool HasNoOwner => m_OwnerID == -1;

        protected override void Awake()
        {
            base.Awake();
            if (targetGameObject == null)
            {
                targetGameObject = this.gameObject;
            }
            entities.Add(this);
            entitiesMap.Add(this.m_NetworkID, this);
        }

        /// <summary>
        /// Event : on ti-net node connected.
        /// </summary>
        /// <param name="node"></param>
        protected override void TiNet_OnNodeConnected(Ximmerse.XR.UnityNetworking.I_TiNetNode node)
        {
            //如果被权限属于自身节点,则发送一条 claim owner message 到新连接的节点:
            if (this.IsOwned)
            {
                ClaimOwnerMessage claimOwnerMessage = TiNetUtility.GetMessage<ClaimOwnerMessage>();
                claimOwnerMessage.OwnerID = TiNetManager.NodeID;
                claimOwnerMessage.NetworkID = this.m_NetworkID;
                claimOwnerMessage.ClaimOwner = true;
                //send a claim owner message to connected node.
                claimOwnerMessage.SendToReliable(node);
            }
        }

        /// <summary>
        /// Event : on ti-net node disconnected.
        /// </summary>
        /// <param name="node"></param>
        protected override void TiNet_OnNodeDisconnected(I_TiNetNode node)
        {
            //Node 已经断链了， 在 local 端中，强行移除 SyncID 的 ownership
            if (this.OwnerID != -1 && this.OwnerID == node.NodeID)
            {
                m_OwnerID = -1;
            }
        }

        /// <summary>
        /// Event : on network stopped.
        /// </summary>
        protected override void TiNet_OnTiNetStop()
        {
            //network is stopped, unclaim locally.
            if (this.IsOwned)
            {
                m_OwnerID = -1;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            entities.Remove(this);
            entitiesMap.Remove(this.m_NetworkID);
        }


        /// <summary>
        /// Use current TiNet node to claim owner on the sync entity.
        /// </summary>
        [InspectFunction]
        public void ClaimOwner()
        {
            if (m_OwnerID != -1 || IsOwned)
            {
                Debug.LogFormat(this, "SyncIdentity : {0} already owned by : {1}", this.name, this.m_OwnerID);
                //Already owned by others | already owned by current node
                return;
            }
            ClaimOwnerMessage claimOwnerMessage = TiNetUtility.GetMessage<ClaimOwnerMessage>();
            claimOwnerMessage.OwnerID = TiNetManager.NodeID;
            claimOwnerMessage.NetworkID = this.m_NetworkID;
            claimOwnerMessage.ClaimOwner = true;
            claimOwnerMessage.ClaimOwnerTime = DateTime.Now.Ticks;

            claimOwnerMessage.SendToAllReliable();

            this.m_OwnerID = TiNetManager.NodeID;

            //记录Claim owner history:
            this.IsLatestClaimOwnerByLocalNode = true;
            this.LatestClaimOwnerTimeTick = claimOwnerMessage.ClaimOwnerTime;
            this.LatestClaimOwnerNode = TiNet.Instance.TiNetIDNodeID;
        }


        /// <summary>
        /// Use current TiNet node to claim owner on the sync entity.
        /// </summary>
        [InspectFunction]
        public void UnClaimOwner()
        {
            if (!IsOwned)
            {
                //Not owned by me
                return;
            }
            ClaimOwnerMessage claimOwnerMessage = TiNetUtility.GetMessage<ClaimOwnerMessage>();
            claimOwnerMessage.OwnerID = TiNetManager.NodeID;
            claimOwnerMessage.NetworkID = this.m_NetworkID;
            claimOwnerMessage.ClaimOwner = false;
            claimOwnerMessage.ClaimOwnerTime = 0;//Uncliam 情况下，不需要Claim Owner Time
            claimOwnerMessage.SendToAllReliable();

            this.m_OwnerID = -1;
        }

        /// <summary>
        /// Message callback : on ownership claimed by other node.
        /// </summary>
        [TiNetMessageCallback(MessageCode.kClaimOwner)]
        static void OnClaimOwner(TiNetMessage message, I_TiNetNode node)
        {
            ClaimOwnerMessage claimMsg = (ClaimOwnerMessage)message;
            //this.OwnerID = TiNetManager.NodeID;
            if (entitiesMap.TryGetValue(claimMsg.NetworkID, out SyncIdentity entity))
            {
                //Claim:
                if (claimMsg.ClaimOwner == true)
                {
                    //如果已经被own了，检查冲突:
                    if (entity.m_OwnerID != -1)
                    {
                        bool CanBeOwnByNewClaimer = false;//是否可以被消息中的发送者要求接管。

                        //如果claim owner message 的时间早于entity记录的时间, 则使用claim owner message 的owner:
                        if (claimMsg.ClaimOwnerTime < entity.LatestClaimOwnerTimeTick)
                        {
                            CanBeOwnByNewClaimer = true;
                        }
                        //切换owner node到时间更早期的:
                        if (CanBeOwnByNewClaimer)
                        {
                            Debug.LogFormat("Ownership been shift to new node: {0} at time override: {1}/{2}", claimMsg.OwnerID
                                , claimMsg.ClaimOwnerTime, entity.LatestClaimOwnerTimeTick
                                );

                            entity.m_OwnerID = claimMsg.OwnerID;

                            entity.IsLatestClaimOwnerByLocalNode = false;
                            entity.LatestClaimOwnerNode = claimMsg.OwnerID;
                            entity.LatestClaimOwnerTimeTick = claimMsg.ClaimOwnerTime;

                            OnClaimOwnershipByOtherNode?.Invoke(entity, node, true);

                        }
                        else
                        {
                            //不变更owner:
                            Debug.LogFormat("Ownership remain at node: {0} at time override: {1}/{2}", entity.m_OwnerID
                                , claimMsg.ClaimOwnerTime, entity.LatestClaimOwnerTimeTick
                                );
                        }
                    }
                    //没有被own, 可以直接被接管:
                    else
                    {
                        entity.m_OwnerID = claimMsg.OwnerID;
                        OnClaimOwnershipByOtherNode?.Invoke(entity, node, true);

                        //记录claim owner history:
                        entity.IsLatestClaimOwnerByLocalNode = false;
                        entity.LatestClaimOwnerNode = claimMsg.OwnerID;
                        entity.LatestClaimOwnerTimeTick = claimMsg.ClaimOwnerTime;
                    }
                }
                //Unclaim:
                else
                {
                    if (entity.m_OwnerID == claimMsg.OwnerID)
                    {
                        entity.m_OwnerID = -1;
                        OnClaimOwnershipByOtherNode?.Invoke(entity, node, false);
                    }
                }
                //Debug.LogFormat(entity.gameObject, "On claim entity: {0} by : {1}, claim: {2}", claimMsg.NetworkID, claimMsg.OwnerID, claimMsg.ClaimOwner);
            }
            else
            {
                //Debug.LogWarningFormat("On claim missing entity: {0} by : {1}", claimMsg.NetworkID, claimMsg.OwnerID);
            }
        }


        void OnValidate()
        {
            if (targetGameObject == null)
            {
                targetGameObject = this.gameObject;
            }
        }



    }

}

