using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ximmerse.XR.UnityNetworking.InternalMessages
{
    /// <summary>
    /// LAN discovery message.
    /// 用于在网络中，标记tinet节点身份的消息体。
    /// </summary>
    [MessageAttribute(messageCode: 0x0001)]
    [System.Serializable]
    public class LANDiscoveryMessage : TiNetMessage
    {
        /// <summary>
        /// TiNet 中 ，Node Name == SystemInfo.deviceUniqueIdentifier
        /// </summary>
        public string NodeName;

        /// <summary>
        /// Guid.NewGuid()
        /// </summary>
        public int NodeID;

        /// <summary>
        /// TCP server 的接口
        /// </summary>
        public int ReliablePort;

        /// <summary>
        /// UDP listener 的接口.
        /// </summary>
        public int UnreliablePort;

        /// <summary>
        /// Preserved
        /// </summary>
        public byte MyRole;

        /// <summary>
        /// 广播身份关键字。
        /// TiNet.FilterWord.
        /// </summary>
        public string Keyword = string.Empty;

        /// <summary>
        /// Custom tag of the TiNet node.
        /// </summary>
        public string CustomTag = string.Empty;

        /// <summary>
        /// The time ticks when the TiNode is started.
        /// </summary>
        public long NodeStartTime;

        public LANDiscoveryMessage()
        {

        }

        public override void OnSerialize()
        {
            this.WriteString(NodeName);
            this.WriteInt(NodeID);
            this.WriteInt(ReliablePort);
            this.WriteInt(UnreliablePort);
            this.WriteByte(MyRole);
            this.WriteString(Keyword);
            this.WriteString(CustomTag);
            this.WriteLong(NodeStartTime);
        }

        public override void OnDeserialize()
        {
            NodeName = this.ReadString();
            NodeID = this.ReadInt();
            ReliablePort = this.ReadInt();
            UnreliablePort = this.ReadInt();
            MyRole = this.ReadByte();
            Keyword = this.ReadString();
            CustomTag = this.ReadString();
            NodeStartTime = ReadLong();
        }

    }
}