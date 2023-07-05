using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// TiNet transport config.
    /// </summary>
    public static class TiNetTransportConfig
    {

        /// <summary>
        /// 如果连续N个时间点都没有收到网络帧包，则判定连接中断。
        /// </summary>
        public static int LostConnectionDropCount = 5;

        /// <summary>
        /// Network framerate.
        /// default 50ms.
        /// </summary>
        public static float NetworkFramerate = 0.05f;

        /// <summary>
        /// 每个 frame 可传输的最大的 packet 数据长度.
        /// </summary>
        public static int MaxPacketSizePerframe = 508;
    }
}