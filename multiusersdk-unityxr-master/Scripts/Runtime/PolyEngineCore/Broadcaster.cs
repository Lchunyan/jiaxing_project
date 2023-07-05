using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// UDP LAN Broadcaster.
    /// broadcast message inside LAN.
    /// </summary>
    public class Broadcaster : MonoBehaviour
    {
        [SerializeField]
        [Tooltip ("The broadcasting target port number.")]
        int m_Port = 9099;

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>The port.</value>
        public int Port
        {
            get
            {
                return m_Port;
            }
            set
            {
                m_Port = value;
            }
        }

        Socket socket = null;

        [SerializeField]
        byte[] m_Buffer = new byte[] { };

        /// <summary>
        /// Gets or sets the buffer.
        /// </summary>
        /// <value>The buffer.</value>
        public byte[] Buffer
        {
            get
            {
                return m_Buffer;
            }
            set
            {
                m_Buffer = value;
            }
        }
        [MinValue(0)]
        int m_BuffetOffset = 0;

        /// <summary>
        /// Gets or sets the buffet offset.
        /// </summary>
        /// <value>The buffet offset.</value>
        public int BuffetOffset
        {
            get
            {
                return m_BuffetOffset;
            }
            set
            {
                m_BuffetOffset = value;
            }
        }

        /// <summary>
        /// Gets or sets the length of the buffer.
        /// If Null, buffer.Length is used.
        /// </summary>
        /// <value>The length of the buffer.</value>
        public int? BufferLength { get; set; }

        [SerializeField]
        bool m_AutoStart = false;
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:PolyEngine.UnityNetworking.Broadcaster"/> auto start.
        /// </summary>
        /// <value><c>true</c> if auto start; otherwise, <c>false</c>.</value>
        public bool AutoStart
        {
            get
            {
                return m_AutoStart;
            }
            set
            {
                m_AutoStart = value;
            }
        }

        [SerializeField]
        bool m_Looping = true;

        /// <summary>
        /// If looping  = true, will keep broadcast the message in every interval second.
        /// Change this value runtime to false to stop broadcasting loop.
        /// </summary>
        /// <value><c>true</c> if looping; otherwise, <c>false</c>.</value>
        public bool Looping
        {
            get
            {
                return m_Looping;
            }
            set
            {
                m_Looping = value;
            }
        }

        [SerializeField]
        [MinValue (0.1f)]
        float m_Interval = 1f;

        /// <summary>
        /// Gets or sets the interval.
        /// </summary>
        /// <value>The interval.</value>
        public float Interval
        {
            get
            {
                return m_Interval;
            }
            set
            {
                m_Interval = value;
            }
        }

        /// <summary>
        /// Is the broadcaster currently keeping broadcasting ?
        /// </summary>
        /// <value><c>true</c> if is loop broadcasting; otherwise, <c>false</c>.</value>
        [InspectFunction]
        public bool IsBroadcasting { get; private set;  }

        Coroutine broadcastingCoroutine;

        IPAddress broadcastAddress = null;

        // Start is called before the first frame update
        void Start()
        {
            if(m_AutoStart)
            {
                StartBroadcasting();
            }
        }

        /// <summary>
        /// Starts broadcasting. If loop = true, will keep broadcasting after calling this method.
        /// If loop = false, broadcast one message then return.
        /// </summary>
        [ContextMenu ("Start broadcast")]
        public void StartBroadcasting ()
        {
            if(Looping)
            {
                broadcastingCoroutine = StartCoroutine(Broadcast());
                Debug.LogFormat("Start looping broadcaster @port: {0}", this.m_Port);
            }
            else
            {
                if(m_Buffer != null && m_Buffer.Length > 0)
                {
                    BroadcastOnce(this.m_Buffer, this.m_BuffetOffset, this.BufferLength ?? m_Buffer.Length);
                    Debug.LogFormat("BroadcastOnce @port: {0}", this.m_Port);
                }
                   
            }
        }

        /// <summary>
        /// Broadcast one message to the LAN, ignore the setting on the component.
        /// </summary>
        [ContextMenu ("Broadcast once")]
        public void BroadcastOnce (byte[] buffer, int offset, int length)
        {
            if(buffer == null || length <= 0 || buffer.Length == 0)
            {
                return;
            }
            if (socket == null)
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.EnableBroadcast = true;
            }
            if (broadcastAddress == null)
            {
                if (PENetworkUtils.GetLocalIP(out IPAddress localIP))
                {
                    PENetworkUtils.GetSubnetBroadcastAddress(localIP, out this.broadcastAddress);
                }
                else
                {
                    broadcastAddress = IPAddress.Broadcast;
                }
            }
            var iep = new IPEndPoint(broadcastAddress, this.m_Port);
            if (m_Buffer.Length > 0)
            {
                socket.SendTo(buffer, offset, length, SocketFlags.None, iep);
            }
        }
        [ContextMenu ("Stop broadcast")]
        public void StopBroadcasting ()
        {
            if(broadcastingCoroutine != null)
            {
                StopCoroutine(broadcastingCoroutine);
                broadcastingCoroutine = null;
            }
            IsBroadcasting = false;
        }

        private void OnDisable()
        {
            StopBroadcasting();
        }

        IEnumerator Broadcast()
        {
            if (socket == null)
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.EnableBroadcast = true;
            }
            if(broadcastAddress == null)
            {
                if(PENetworkUtils.GetLocalIP(out IPAddress localIP))
                {
                    PENetworkUtils.GetSubnetBroadcastAddress(localIP, out this.broadcastAddress);
                }
                else
                {
                    broadcastAddress = IPAddress.Broadcast;
                }
            }
            var iep = new IPEndPoint(broadcastAddress, this.m_Port);
            if (m_Buffer.Length > 0)
            {
                socket.SendTo(this.m_Buffer, this.m_BuffetOffset, BufferLength ?? m_Buffer.Length, SocketFlags.None, iep);
            }
            while (Looping)
            {
                IsBroadcasting = true;
                yield return new WaitForSeconds(m_Interval);
                if (m_Buffer.Length > 0)
                    socket.SendTo(this.m_Buffer, this.m_BuffetOffset, BufferLength ?? m_Buffer.Length, SocketFlags.None, iep);
            }
            IsBroadcasting = false;
            broadcastingCoroutine = null;
            //Debug.LogFormat("Send broadcast message to : {0}", iep.ToString());
        }
    }
}