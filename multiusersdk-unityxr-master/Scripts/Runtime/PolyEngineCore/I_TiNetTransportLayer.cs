using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// Parameter to describe tinet node.
    /// </summary>
    public struct TiNetNodeDescriptor
    {
        /// <summary>
        /// Broadcast port
        /// </summary>
        public int BroadcastPort;

        /// <summary>
        /// Reliable port
        /// </summary>
        public int ReliablePort;

        /// <summary>
        /// Unreliable port
        /// </summary>
        public int UnreliablePort;

        /// <summary>
        /// Should broadcast ID
        /// </summary>
        public bool BroadcastID;

        /// <summary>
        /// Should auto joint network?
        /// </summary>
        public bool AutoJointNetwork;

        /// <summary>
        /// Tinet filter mode.
        /// </summary>
        public TiNet.ConnectionFilterMode filterMode;

        /// <summary>
        /// The node tag.
        /// </summary>
        public string NodeTag;

        /// <summary>
        /// If true, on application pause will cause the transport layer disconnect
        /// </summary>
        public bool DetachNetworkOnAppPause;

        /// <summary>
        /// machine name of  this tinet
        /// </summary>
        public string TiNetMachineName;

        /// <summary>
        /// Node ID of this tinet node
        /// </summary>
        public int TiNetNodeID;

        /// <summary>
        /// The filter keyword.
        /// </summary>
        public string FilterKeyword;

        /// <summary>
        /// The remote node's address.
        /// </summary>
        public IPAddress RemoteAddress;

        /// <summary>
        /// Remote node only, the remote node's network start time.
        /// </summary>
        public long NetworkStartTime;
    }

    /// <summary>
    /// TiNet transport layer.
    /// </summary>
    public interface I_TiNetTransportLayer
    {
        int PacketBufferSize
        {
            get;
        }
        /// <summary>
        /// Is network running ?
        /// </summary>
        bool IsNetworkActive
        {
            get;
        }

        /// <summary>
        /// Init the transport layer.
        /// </summary>
        /// <param name="parameter"></param>
        void Initialize(TiNetNodeDescriptor parameter);

        /// <summary>
        /// Starts networking.
        /// </summary>
        /// <returns>Success or not.</returns>
        bool StartNetwork();

        /// <summary>
        /// Stops networking.
        /// </summary>
        void StopNetwork();

        /// <summary>
        /// Enqueue an outgoing message, to specific target node.
        /// If target node is null, sends to all connected node.
        /// </summary>
        /// <param name="tinetMessage"></param>
        /// <param name="node">The target node.</param>
        /// <param name="reliable"></param>
        void EnqueueMessage(TiNetMessage tinetMessage, I_TiNetNode node, bool reliable);

        /// <summary>
        /// UDP broadcast to LAN
        /// </summary>
        void BroadcastToLAN(TiNetMessage tinetMessage);

        /// <summary>
        /// event : on transport layer connecting to node.
        /// </summary>
        event Action<I_TiNetNode> OnConnectingToNode;

        /// <summary>
        /// event : on transport layer connected to node.
        /// </summary>
        event Action<I_TiNetNode> OnConnectedToNode;

        /// <summary>
        /// On receives remote message.
        /// </summary>
        event Action<TiNetMessage, I_TiNetNode> OnMessage;

        /// <summary>
        /// On disconnect connection.
        /// </summary>
        event Action<I_TiNetNode> OnDisconnect;

        /// <summary>
        /// Function : on transport layer discover a new TiNet node.
        /// The result is a I_TiNetNode reference.
        /// </summary>
        event Func<TiNetNodeDescriptor, I_TiNetNode> OnDiscoverNewNode;
    }
}