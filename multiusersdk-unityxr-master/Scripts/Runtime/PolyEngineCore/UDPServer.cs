using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// UDP server.
    /// </summary>
    public class UDPServer : NetServer
    {
        UDPSocket ServerSocket = null;

        /// <summary>
        /// The buffer size in KB.
        /// </summary>
        [Range(5,50)]
        [Tooltip("The buffer size in KB.")]
        public int BufferKB = 5;

        /// <summary>
        /// The persist port flag.
        /// If persist port = false, will auto increase the server port number until it's available.
        /// Turn this option on if you want the udp server to be started in anyway.
        /// Turn this option off if the udp server port does matter, and requires the UDP Server to be started exactly at the port.
        /// </summary>
        [Tooltip("The persist port flag.\nIf persist port = false, will auto increase the server port number until it's available.\nTurn this option on if you want the udp server to be started in anyway.\nTurn this option off if the udp server port does matter. In this case, the UDP Server to be started exactly at the port. ")]
        public bool PersistPort = false;

        [SerializeField]
        [Tooltip("If true, wait for all of the sent message to be sent at end of frame.")]
        bool m_WaitForSentMessage = false;

        /// <summary>
        /// If true, wait for all of the sent message to be sent at end of frame.
        /// </summary>
        /// <value><c>true</c> if force flush all message; otherwise, <c>false</c>.</value>
        public bool WaitForSentMessage
        {
            get
            {
                return m_WaitForSentMessage;
            }
            set
            {
                m_WaitForSentMessage = value;
            }
        }

        [SerializeField]
        private bool m_dontDestroyOnLoad;


        bool m_IsServerRunning;
        /// <summary>
        /// Gets a value indicating whether this <see cref="T:UDPManager"/> is server running.
        /// </summary>
        /// <value><c>true</c> if is server running; otherwise, <c>false</c>.</value>
        [InspectFunction]
        public override bool IsServerRunning { get => m_IsServerRunning; }



        /// <summary>
        /// is the server accepting new client.
        /// </summary>
        public override bool AcceptNewClient
        {
            get => IsServerRunning;
        } 

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:UDPServer"/> dont destroy on load1.
        /// </summary>
        /// <value><c>true</c> if dont destroy on load1; otherwise, <c>false</c>.</value>
        public bool dontDestroyOnLoad { get { return m_dontDestroyOnLoad; } set { m_dontDestroyOnLoad = value; } }

        /// <summary>
        /// The peer connections.
        /// </summary>
        internal List<UDPConnection> peerConnections
        {
            get
            {
                return this.ServerSocket != null ? ServerSocket.allPeerConnections : null;
            }
        }

        /// <summary>
        /// Event : on server receives connection. Parameter specify the remote IP endpoint.
        /// </summary>
        internal event System.Action<I_Connection> onConnected = null;

        [Header (" --- Heart Beat ---")]
        /// <summary>
        /// If heart beating on, require all clients sends heart beat message in fixed time rate.
        /// </summary>
        public bool HeartBeat = true;

        /// <summary>
        /// The heart beat interval.
        /// </summary>
        [Tooltip ("Require heart beat interval time for client")]
        [MinValue ( 0.1f , label = "Heart-Beat Interval")]
        public float ClientHeartBeatInterval = 1;

        /// <summary>
        /// Lost connection re-try time. 
        /// The connection lost heart beat more than [LostConnectionRetryTime] X [ClientHeartBeatInterval] will be dropped.
        /// </summary>
        [Tooltip ("Lost connection re-try time. The connection lost heart beat more than [LostConnectionRetryTime] X [ClientHeartBeatInterval] will be dropped.")]
        public int LostConnectionRetryTime = 2;

        /// <summary>
        /// Gets the how many live connection to this udp server.
        /// </summary>
        /// <value>The connection count.</value>
        [InspectFunction]
        public override int ConnectionCount
        {
            get
            {
                return peerConnections != null ? peerConnections.Count : 0;
            }
        }

        /// <summary>
        /// The async-send ops.
        /// </summary>
        List<System.IAsyncResult> AsyncSendOps = new List<IAsyncResult>();

        [Header(" --- Debug ---")]
        public bool DebugLog = false;

        void Awake()
        {
            m_IsServerRunning = false;
            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
        }


        void OnDestroy()
        {
            if (IsServerRunning)
            {
                StopServer();
            }
        }

        private void Start()
        {
            if (AutoStart)
                StartServer();
        }

        [InspectFunction]
        public override bool StartServer()
        {
            if(IsServerRunning)
            {
                return false;//already running 
            }
            ServerSocket = new UDPSocket(BufferKB * 1024);
            PENetworkUtils.GetLocalIP(out IPAddress localIP);
            int _serverPort = this.ServerPort;
            bool isServerStarted = ServerSocket.StartServer(localIP, ref _serverPort, HeartBeat, LostConnectionRetryTime * ClientHeartBeatInterval, this.ClientHeartBeatInterval, this.PersistPort);
            
            if(isServerStarted)
            {
                this.ServerPort = _serverPort;
                m_IsServerRunning = true;
                ServerAddress = localIP.ToString();
                StartCoroutine(Daemon());
                Debug.LogFormat("Start UDP server @{0} : {1}", localIP, ServerPort);
            }
            else
            {
                m_IsServerRunning = false;
                Debug.LogErrorFormat(this, "Server's not started at {0}:{1}", localIP,this.ServerPort);
            }
            return m_IsServerRunning;

            //TODO : handle disconnect event
        }

        [InspectFunction]
        public override void StopServer()
        {
            if(ServerSocket != null)
            {
                ServerSocket.StopServer();
                ServerSocket = null;
                m_IsServerRunning = false;
                Debug.Log("Socket server stop.");
            }
        }

        /// <summary>
        /// Broadcast a text message to all peer clients.
        /// This is synchronous call.
        /// </summary>
        /// <param name="UTF8Text">Text.</param>
        [InspectFunction]
        public void Broadcast(string UTF8Text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(UTF8Text);
            this.ServerSocket.BroadcastToAllPeers(buffer, buffer.Length, 0);
        }

        /// <summary>
        /// Broadcast the buffer.
        /// This is synchronous call.
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        public void Broadcast (byte[] buffer, int length, int offset)
        {
            this.ServerSocket.BroadcastToAllPeers(buffer, length, offset);
        }


        /// <summary>
        /// Broadcasts the buffer in asynchronous mode.
        /// </summary>
        /// <param name="utf8Text">Text.</param>
        [InspectFunction]
        public void BroadcastAsync(string utf8Text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(utf8Text);
            ServerSocket.BroadcastAsync(buffer, buffer.Length, 0, AsyncSendOps);
        }

        /// <summary>
        /// Broadcasts the buffer in asynchronous mode.
        /// </summary>
        /// <param name="Message">Buffer.</param>
        /// <param name="length">Length.</param>
        /// <param name="offset">Offset.</param>
        public override void BroadcastAsync (byte[] Message, int length, int offset)
        {
            AsyncSendOps.Clear();
            ServerSocket.BroadcastAsync(Message, length, offset, AsyncSendOps);
        }

        /// <summary>
        /// Sends the buffer to the connection synchronously.
        /// </summary>
        /// <param name="connection">Connection.</param>
        /// <param name="Message">Message.</param>
        /// <param name="Offset">Offset.</param>
        /// <param name="Length">Length.</param>
        public override void SendTo (I_Connection connection, byte[] Message, int Offset, int Length)
        {
            ServerSocket.SendSync(connection.ipEndpoint, Message, Offset, Length);
        }

        /// <summary>
        /// Sends buffer to target endpoint asynchronously.
        /// </summary>
        /// <param name="connection">Connection.</param>
        /// <param name="Message">Message.</param>
        /// <param name="Offset">Offset.</param>
        /// <param name="Length">Length.</param>
        public IAsyncResult SendToAsync (I_Connection connection, byte[] Message, int Offset, int Length)
        {
            return ServerSocket.SendAsync(connection.ipEndpoint, Message, Offset, Length);
        }

        /// <summary>
        /// Daemon : server coroutine.
        /// </summary>
        /// <returns>The daemon.</returns>
        IEnumerator Daemon ()
        {
            WaitForEndOfFrame waitEOF = new WaitForEndOfFrame();
            while (IsServerRunning)
            {
                if(ServerSocket.isServerRunning)
                {
                    lock (ServerSocket.recvPackets)
                    {
                        if (ServerSocket.recvPackets.Count > 0)
                        {
                            for (int i = 0, ServerSocketserverRecvPacketsCount = ServerSocket.recvPackets.Count; i < ServerSocketserverRecvPacketsCount; i++)
                            {
                                var packet = ServerSocket.recvPackets[i];
                                if (DebugLog)
                                {
                                    Debug.LogFormat("UDP receive : {0} => {1}", packet.PeerAddress, System.Text.Encoding.UTF8.GetString(packet.Data));
                                }
                                this.InvokeOnReceiveData(packet);
                            }
                            this.InvokeOnFlushData(ServerSocket.recvPackets);
                            ServerSocket.recvPackets.Clear();
                        }
                    }
                    //Update heart-beat time :
                    if ((Time.time - ServerSocket.LastUpdateHeartbeatTime) >= this.ClientHeartBeatInterval)
                    {
                        ServerSocket.UpdateHeartBeat();
                    }
                    this.ServerSocket.UnityTime = Time.time;
                    this.ServerSocket.UnityRealTime = Time.realtimeSinceStartup;
                    //foreach (var udpConn in this.peerConnections)
                    //{
                    //    Debug.LogFormat("Conn : {0}, buffer size: {1}", udpConn.ipEndpoint.ToString(), udpConn.asyncEventArgs.SendPacketsSendSize); 
                    //}

                    //Handle async send operation - remove the complete async operation result.
                    if (AsyncSendOps.Count > 0)
                    {
                        for (int i = AsyncSendOps.Count - 1; i >= 0; i--)
                        {
                            if (AsyncSendOps[i].IsCompleted)
                            {
                                AsyncSendOps.RemoveAt(i);
                            }
                        }
                    }

                    //Handle new connection event :
                    lock (ServerSocket.newConnections)
                    {
                        if (this.ServerSocket.newConnections != null && ServerSocket.newConnections.Count > 0)
                        {
                            for (int i = 0, ServerSocketnewConnectionsCount = ServerSocket.newConnections.Count; i < ServerSocketnewConnectionsCount; i++)
                            {
                                var conn = ServerSocket.newConnections[i];
                                this.onConnected?.Invoke(conn);
                                //this.invoke
                            }
                            //clear the new connection per frame:
                            this.ServerSocket.newConnections.Clear();
                        }
                    }
                   

                    //still there're pending async-operation:
                    if (m_WaitForSentMessage && AsyncSendOps.Count > 0)
                    {
                        yield return waitEOF;
                        if (AsyncSendOps.Count > 0)
                        {
                            for (int i = AsyncSendOps.Count - 1; i >= 0; i--)
                            {
                                if (!AsyncSendOps[i].IsCompleted)
                                {
                                    AsyncSendOps[i].AsyncWaitHandle.WaitOne();//blocks to wait for complete
                                }
                                AsyncSendOps.RemoveAt(i);//remove the index
                            }
                        }
                    }
                    yield return null;
                }
                //When UDP server socket paused by error:
                else
                {
                    m_IsServerRunning = false;
                }
            }
        }
    }

}