using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Ximmerse.XR.Asyncoroutine;

namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// LAN broadcasting listener receives broadcast message inside LAN.
    /// </summary>
    public class BroadcastListener : MonoBehaviour
    {
        [SerializeField]
        bool m_AutoStart = true;
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:PolyEngine.UnityNetworking.BroadcastListener"/>
        /// auto start.
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

        /// <summary>
        /// Start delay time.
        /// </summary>
        public float StartDelay = 1;

        [SerializeField]
        [Tooltip("The broadcasting target port number.")]
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

        /// <summary>
        /// The buffer size in KB.
        /// </summary>
        [Range(1, 50)]
        [Tooltip("The buffer size in KB.")]
        public int m_BufferKB = 1;

        public int BufferKB
        {
            get
            {
                return m_BufferKB;
            }
            set
            {
                if(m_BufferKB != value)
                {
                    m_BufferKB = value;
                    m_Buffer = new byte[m_BufferKB];
                }
            }
        }
        SocketAsyncEventArgs socketRecvArgs = null;
        Socket socket = null;
        byte[] m_Buffer = null;
        public bool DebugLog = false;
        private bool isSocketDisposed = false;

        /// <summary>
        /// Event : listen broadcast message.
        /// 1st parameter : the ip address of the broadcaster
        /// 2nd parameter : the byte[] buffer holds the received message.
        /// 3rd parameter : the length of the actual message.
        /// </summary>
        public event System.Action<string, byte[], int> OnListenMessage = null;
        /// <summary>
        /// Gets a value indicating whether this <see cref="T:PolyEngine.UnityNetworking.BroadcastListener"/> is listening.
        /// </summary>
        /// <value><c>true</c> if is listening; otherwise, <c>false</c>.</value>
        [InspectFunction]
        public bool IsListening
        {
            get; private set;
        }

        string BroadcasterIP;
        int BroadcasterMessageLength;

        bool isTriggered = false;

        Coroutine daemonCoroutine;

        void Awake()
        {
            m_Buffer = new byte[BufferKB * 1024];
        }

        private async void Start()
        {
            if(m_AutoStart)
            {
                if (StartDelay > 0)
                    await new WaitForGameTime(StartDelay);
                StartListening();
            }
        }


        private void OnDisable()
        {
            StopListening();
        }

        [ContextMenu ("Start listening")]
        public void StartListening()
        {
            if(IsListening)
            {
                Debug.LogWarningFormat(this.gameObject, "Broadcaster listener already started : {0}", this.name);
                return;
            }
           
            IPAddress any = IPAddress.Any;
            var ep = new IPEndPoint(any, m_Port) as EndPoint;
            if(socket != null)
            {
                try
                {
                    socket.Dispose();
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
            }
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            socket.Bind(ep);
            isSocketDisposed = false;
            socketRecvArgs = new SocketAsyncEventArgs();
            socketRecvArgs.RemoteEndPoint = ep;
            socketRecvArgs.SetBuffer(this.m_Buffer, 0, m_Buffer.Length);
            socketRecvArgs.Completed += (object sender, SocketAsyncEventArgs e) => 
            {
                if(isSocketDisposed)
                {
                    return;
                }
                var ipEndPointStr = ((IPEndPoint)e.RemoteEndPoint).ToString();
                ipEndPointStr = ipEndPointStr.Remove(ipEndPointStr.IndexOf(":"));
                this.BroadcasterIP = ipEndPointStr;

                BroadcasterMessageLength = e.BytesTransferred;
                BroadcasterIP = ipEndPointStr;
                isTriggered = true;

                //loop recv:
                ((Socket)sender).ReceiveFromAsync(e);
            };
            socket.ReceiveFromAsync(socketRecvArgs);
            IsListening = true;

            daemonCoroutine = StartCoroutine(Daemon());

            Debug.LogFormat(this.gameObject, "Starts broadcaster listenr : {0}", this.name);
        }

        IEnumerator Daemon ()
        {
            while (IsListening)
            {
                if(isTriggered)
                {
                    isTriggered = false;
                    try
                    {
                        OnListenMessage?.Invoke(BroadcasterIP, this.m_Buffer, this.BroadcasterMessageLength);
                    }
                    catch (System.Exception exc)
                    {
                        Debug.LogException(exc);
                    }

                    if (DebugLog)
                        Debug.LogFormat("Receive bytes: {0} from IP:{1}", BroadcasterMessageLength, BroadcasterIP);
                }
                yield return null;
            }
        }


        [ContextMenu("Stop listening")]
        public void StopListening()
        {
            if(IsListening && socket != null)
            {
                if(socketRecvArgs != null)
                {
                    socketRecvArgs.Dispose();
                    socketRecvArgs = null;
                }
                socket.Close();
                socket.Dispose();
           
                socket = null;
                isSocketDisposed = true;
                IsListening = false;

                Debug.LogFormat(this.gameObject, "Broadcast listener: {0} is stopped", name);
            }

            if(daemonCoroutine != null)
            {
                StopCoroutine(daemonCoroutine);
            }
        }
    }
}
