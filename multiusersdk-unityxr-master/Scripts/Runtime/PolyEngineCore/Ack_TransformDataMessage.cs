namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// 针对 TransferDataMessage 的应答.
    /// </summary>
    [MessageAttribute(messageCode: 0x0006)]
    internal class Ack_TransformDataMessage : TiNetMessage
    {
        /// <summary>
        /// 相应的文件名。
        /// </summary>
        public string name;

        /// <summary>
        /// 响应的 package index.
        /// </summary>
        public ushort packetIndex;

        public override void OnDeserialize()
        {
            name = ReadString();
            packetIndex = ReadUShort();
        }

        public override void OnSerialize()
        {
            WriteString(name);
            WriteUShort(packetIndex);
        }
    }
}