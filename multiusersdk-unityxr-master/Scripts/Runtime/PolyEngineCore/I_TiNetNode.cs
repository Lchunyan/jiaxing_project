using System;

namespace Ximmerse.XR.UnityNetworking
{
    using System.Net;

    /// <summary>
    /// internal temp structure 
    /// </summary>
    internal struct _InternalTiNetNode : I_TiNetNode
    {
        public bool IsLocalNode { get; set; }

        public IPAddress Address { get; set; }

        public string NodeName { get; set; }

        public int NodeID { get; set; }

        public string CustomTag { get; set; }

        public DateTime NodeStartTime { get; set; }

        public object UserData { get; set; }

        public TiNetNodeState State { get; set; }

        /// <summary>
        /// The ip end point for reliable channel.
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
        /// The node's filter keyword.
        /// </summary>
        public string FilterKeyword
        {
            get; set;
        }
    }

    /// <summary>
    /// TiNet node state.
    /// </summary>
    public enum TiNetNodeState : byte
    {
        NotConnected = 0, Connecting, Connected,
    }

    /// <summary>
    /// Interface : Ti Net node.
    /// </summary>
    public interface I_TiNetNode
    {
        /// <summary>
        /// 是否本地网络节点?
        /// </summary>
        bool IsLocalNode
        {
            get;
        }

        /// <summary>
        /// The node's address.
        /// </summary>
        [System.Obsolete("Use [EndPoint_Reliable] and [EndPoint_Unreliable] instead.")]
        IPAddress Address
        {
            get;
        }

        /// <summary>
        /// The node's end point , in reliable channel.
        /// </summary>
        IPEndPoint EndPoint_Reliable
        {
            get;
        }

        /// <summary>
        /// The node's end point , in unreliable channel.
        /// </summary>
        IPEndPoint EndPoint_Unreliable
        {
            get;
        }

        /// <summary>
        /// Gets the node unique name.
        /// </summary>
        string NodeName
        {
            get;
        }

        /// <summary>
        /// Gets the node id
        /// </summary>
        int NodeID
        {
            get;
        }

        /// <summary>
        /// The node's custom tag.
        /// </summary>
        string CustomTag
        {
            get;
        }

        /// <summary>
        /// The node's filter keyword.
        /// </summary>
        string FilterKeyword
        {
            get;
        }

        /// <summary>
        /// The node's start time in reality.
        /// </summary>
        DateTime NodeStartTime
        {
            get;
        }

        /// <summary>
        /// The user data allows application script to extent TiNetNode instance.
        /// </summary>
        object UserData
        {
            get; set;
        }

        /// <summary>
        /// 节点连接状态
        /// </summary>
        TiNetNodeState State
        {
            get;
        }
    }
}