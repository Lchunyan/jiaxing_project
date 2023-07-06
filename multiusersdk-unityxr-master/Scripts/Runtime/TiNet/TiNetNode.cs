using System.Net;
using System;
using System.Net.Sockets;

namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// The Ti-Network node.
    /// 代表一个 TiNet 中的网络节点。
    /// </summary>
    [Serializable]
    internal class TiNetNode : I_TiNetNode
    {
        /// <summary>
        /// 是否本地网络节点?
        /// </summary>
        public bool isLocalNode;

        /// <summary>
        /// 是否本地网络节点?
        /// </summary>
        public bool IsLocalNode
        {
            get => isLocalNode;
        }

        /// <summary>
        /// The Node Name
        /// </summary>
        public string nodeName;

        /// <summary>
        /// Gets the node id.
        /// </summary>
        public string NodeName
        {
            get => nodeName;
        }

        public TiNetNodeState state;

        /// <summary>
        /// 节点是否已经建立了连接 ?
        /// </summary>
        public TiNetNodeState State { get => state; }

        /// <summary>
        /// The Node ID
        /// </summary>
        public int nodeID;

        /// <summary>
        /// Gets the node id.
        /// </summary>
        public int NodeID
        {
            get => nodeID;
        }

        /// <summary>
        /// The node's address.
        /// </summary>
        public IPAddress address;

        /// <summary>
        /// The node's address.
        /// </summary>
        public IPAddress Address
        {
            get => address;
        }

        public string customTag = string.Empty;

        /// <summary>
        /// The node's custom tag.
        /// </summary>
        public string CustomTag
        {
            get => customTag;
            set => customTag = value;
        }

        /// <summary>
        /// The node's reliable port.
        /// </summary>
        public int reliablePort;

        /// <summary>
        /// The node's unreliable port.
        /// </summary>
        public int unreliablePort;

        /// <summary>
        /// The time ticks when the ti-node starts.
        /// </summary>
        public long nodeStartTime;

        /// <summary>
        /// The node starts time.
        /// </summary>
        public DateTime NodeStartTime
        {
            get
            {
                return new DateTime(nodeStartTime);
            }
        }

        /// <summary>
        /// The user data allows application script to extent TiNetNode instance.
        /// </summary>
        public object UserData
        {
            get; set;
        }

        /// <summary>
        /// End point for reliable channel.
        /// </summary>
        public IPEndPoint EndPoint_Reliable
        {
            get; set;
        }

        /// <summary>
        /// End point for unreliable channel.
        /// </summary>
        public IPEndPoint EndPoint_Unreliable
        {
            get; set;
        }

        /// <summary>
        /// 内部关联的tcp client组件。可能为空。
        /// 这个组件是用于 error handling的， 收发通信不应该使用此字段，而使用 tcp socket.
        /// </summary>
        internal TCPClient tcpClientComponent;

        /// <summary>
        /// 此 node 的tcp socket发送消息通道， 当 state = connected 的时候有效.
        /// </summary>
        internal Socket tcpSocket;

        /// <summary>
        /// The node's filter keyword.
        /// </summary>
        public string FilterKeyword
        {
            get; internal set;
        }


        public TiNetNode()
        {

        }

        public override string ToString()
        {
            return string.Format("Ti-Node ID: {0}, Address: {1}, Tag: {2}, State: {3}", nodeName, address, CustomTag, state);
        }

        /// <summary>
        /// 记录此节点对上一次发送消息的时间。
        /// 用于心跳检测。
        /// </summary>
        internal DateTime? previousSentMessageTime;

        public override bool Equals(object obj)
        {
            if (obj is TiNetNode)
            {
                var tinetNode = obj as TiNetNode;
                return tinetNode.nodeID == this.nodeID;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.nodeID.GetHashCode();
        }

    }
}