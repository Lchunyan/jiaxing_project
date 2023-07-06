namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// Internal data structure for tcp data wrapper.
    /// </summary>
    internal struct TcpDataPacket
    {
        //public string PeerAddress;
        //public int PeerPort;
        public bool ProcessFlag;//处理标记， 如果已经被socket 发送了， 标记为true.
        public byte[] Data;
        public int offset;
        public int length;
    }
}
