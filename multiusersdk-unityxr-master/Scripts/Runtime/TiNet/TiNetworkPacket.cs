using System.Net;


namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// TiNetwork data packet.
    /// </summary>
    public class TiNetworkPacket
    {
        public readonly byte[] Data;

        public int DataLength;

        /// <summary>
        /// This field is internal used only.
        /// </summary>
        public readonly IPEndPoint endPoint;

        /// <summary>
        /// 标记此network packet是否已经被处理完毕?
        /// 如果 isProcessed == true, 代表此packet内的数据已经被发送。
        /// </summary>
        internal bool isProcessed;

        public TiNetworkPacket(int defaultBufferSize)
        {
            Data = new byte[defaultBufferSize];
            endPoint = new IPEndPoint(IPAddress.Any, 0);
        }
    }
}