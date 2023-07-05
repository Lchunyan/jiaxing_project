using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ximmerse.XR.UnityNetworking;
using System.Net;

namespace Ximmerse.XR
{
    /// <summary>
    /// TiNet manager
    /// </summary>
    [RequireComponent (typeof(TiNet))]
    public class TiNetManager : SingletonMonobehaviour<TiNetManager>
    {

        TiNet net;

        protected override void Awake()
        {
            base.Awake();
            net = GetComponent<TiNet>();
            TiNet.OnNodeConnected += TiNet_OnNodeConnected;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            TiNet.OnNodeConnected -= TiNet_OnNodeConnected;
        }

        /// <summary>
        /// On TiNode connected.
        /// </summary>
        /// <param name="node"></param>
        private void TiNet_OnNodeConnected(I_TiNetNode node)
        {
            Debug.LogFormat("On node connected : {0}", node.NodeID);
        }

        /// <summary>
        /// The current node reference.
        /// </summary>
        public static int NodeID
        {
            get => TiNet.Instance.TiNetIDNodeID;
        }
    }


}