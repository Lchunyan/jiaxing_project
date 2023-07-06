namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// 此消息用于问询 tinet node 的节点信息。
    /// </summary>
    [MessageAttribute(messageCode: 0x0004)]
    public class QueryNodeInfoMessage : TiNetMessage
    {
        public enum MessageFlag : byte
        {
            /// <summary>
            /// 问询包
            /// </summary>
            Ask = 0,
            /// <summary>
            /// 回复包
            /// </summary>
            Ack,
        }
        public MessageFlag messageFlag;
        public int UnreliablePort;
        public int ReliablePort;
        public string Address;
        public string NodeName;
        public int NodeID;
        public string CustomTag;
        public long NodeStartTime;
        public bool mutualConnection;

        public override void OnDeserialize()
        {
            messageFlag = (MessageFlag)this.ReadByte();
            UnreliablePort = this.ReadInt();
            ReliablePort = this.ReadInt();
            Address = this.ReadString();
            NodeName = this.ReadString();
            NodeID = this.ReadInt();
            CustomTag = this.ReadString();
            NodeStartTime = this.ReadLong();
            mutualConnection = this.ReadBool();
        }

        public override void OnSerialize()
        {
            this.WriteByte((byte)messageFlag);
            this.WriteInt(UnreliablePort);
            this.WriteInt(ReliablePort);
            this.WriteString(Address);
            this.WriteString(NodeName);
            this.WriteInt(NodeID);
            this.WriteString(CustomTag);
            this.WriteLong(NodeStartTime);
            WriteBool(mutualConnection);
        }

        public override string ToString()
        {
            return string.Format("Flag: {0}, Unreliable Port: {1}, Reliable Port: {2}, Address : {3}, Node Name: {4}, ID: {5}"
                ,this.messageFlag, this.UnreliablePort, this.ReliablePort, this.Address, this.NodeName, this.NodeID);
        }

    }
}