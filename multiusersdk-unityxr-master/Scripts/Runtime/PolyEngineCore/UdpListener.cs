using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// Udp listener.
    /// A simple class to receive and event up received data.
    /// </summary>
    public class UdpListener : MonoBehaviour
    {
        private const string kLogMsg = "Receive data of length: {0}";

        /// <summary>
        /// The listener port.
        /// </summary>
        [SerializeField]
        int Port = 10033;

        /// <summary>
        /// if true, Start listener at Start().
        /// </summary>
        [Tooltip("Start listener at Start()")]
        public bool AutoStart = true;

        bool m_IsListening;

        [InspectFunction]
        public bool IsListening
        {
            get => m_IsListening;
        }

        string m_BoundLocalAddress = string.Empty;

        /// <summary>
        /// The bound local address;
        /// </summary>
        [InspectFunction]
        public string BoundLocalAddress
        {
            get
            {
                return m_BoundLocalAddress;
            }
        }

        UdpSocketListenerWrapper listener;

        List<TiNetworkPacket> networkDataPackets = new List<TiNetworkPacket>();

        public event Action<TiNetworkPacket> OnReceivePacket = null;

        public event Action<IEnumerable<TiNetworkPacket>> OnReceivePackets = null;

        public bool DebugLog;

        private void Start()
        {
            if (AutoStart)
            {
                StartListener();
            }
        }

        private void OnDestroy()
        {
            if (listener != null && listener.IsBound)
            {
                listener.Dispose();
            }
        }

        /// <summary>
        /// Starts udp listener.
        /// </summary>
        [InspectFunction]
        public void StartListener()
        {
            listener = new UdpSocketListenerWrapper(this);
            listener.BindToLocalAddress(this.Port);
            m_IsListening = true;
            m_BoundLocalAddress = listener.LocalAddress;
        }

        /// <summary>
        /// Stops listener
        /// </summary>
        [InspectFunction]
        public void StopListener()
        {
            listener.Dispose();
            listener = null;
            m_IsListening = false;
        }

        private void Update()
        {
            if (m_IsListening)
            {
                networkDataPackets.Clear();
                if (listener.PopReceivePacket(networkDataPackets) > 0)
                {
                    //Invoke receive packet events:
                    for (int i = 0, networkDataPacketsCount = networkDataPackets.Count; i < networkDataPacketsCount; i++)
                    {
                        var packet = networkDataPackets[i];
                        OnReceivePacket?.Invoke(packet);
                        if (DebugLog)
                        {
                            Debug.LogFormat(kLogMsg, packet.DataLength);
                        }
                    }

                    OnReceivePackets?.Invoke(networkDataPackets);

                    for (int i = 0, networkDataPacketsCount = networkDataPackets.Count; i < networkDataPacketsCount; i++)
                    {
                        var packet = networkDataPackets[i];
                        packet.Return();
                    }
                    networkDataPackets.Clear();
                }
            }
        }
    }
}