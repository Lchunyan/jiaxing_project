using System.Collections.Generic;
using System.Net;
using UnityEngine;
using System;
using Object = UnityEngine.Object;
using System.Collections.Concurrent;

namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// TiNetwork packet pool.
    /// </summary>
    public static class TiNetworkPacketPool
    {
        const int kInitPoolCount = 50;

        /// <summary>
        /// Buffer size count for each network packet.
        /// </summary>
        const int kInitBufferSizePerPacket = 1024 * 5;

        //static internal List<TiNetworkPacket> packets = new List<TiNetworkPacket>();

        static internal ConcurrentQueue<TiNetworkPacket> packets = new ConcurrentQueue<TiNetworkPacket>();

        static object mutex = new object();

        /// <summary>
        /// Init the pool
        /// </summary>
        public static void Init()
        {
#if UNITY_2021_1_OR_NEWER
            //packets.Clear();
            while(packets.IsEmpty == false)
            {
                if(!packets.TryDequeue(out TiNetworkPacket dequeue))
                {
                    break;
                }
            }
#endif
            for (int i = 0; i < kInitPoolCount; i++)
            {
                var pooledPacket = new TiNetworkPacket(kInitBufferSizePerPacket);
                packets.Enqueue(pooledPacket);
                //packets.Add(pooledPacket);
            }
        }

        /// <summary>
        /// Gets network packet from pool.
        /// </summary>
        /// <returns></returns>
        public static TiNetworkPacket GetNetworkPacket()
        {
            //int packetCount = 0;
            //TiNetworkPacket pooledPacket = null;
            //lock (mutex)
            //{
            //    packetCount = packets.Count;
            //    if (packetCount > 0)
            //    {
            //        pooledPacket = packets[packets.Count - 1];
            //        packets.RemoveAt(packets.Count - 1);
            //        pooledPacket.isProcessed = false;
            //    }
            //}
            if (packets.TryDequeue(out TiNetworkPacket pooledPacket))
            {
                //Debug.LogFormat("Gets a pooled instance : {0}", pooledPacket.isPooled);
                pooledPacket.isProcessed = false;
                return pooledPacket;
            }

            var newPacket = new TiNetworkPacket(kInitBufferSizePerPacket);
            //Debug.LogFormat("Gets a non-pooled instance : {0}", pooledPacket.isPooled);
            return newPacket;

        }

        /// <summary>
        /// Returns the network data packet to pooled list.
        /// </summary>
        /// <param name="packet"></param>
        public static void Return(this TiNetworkPacket packet)
        {
            //lock (mutex)
            //{
            //    packets.Add(packet);
            //    packet.isProcessed = true;
            //}

            packet.isProcessed = true;
            packets.Enqueue(packet);
        }
    }
}