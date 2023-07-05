using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// UDP client.
    /// </summary>
    public class UDPClient : NetClient
    {
        public enum UDPClientState
        {
            None = 0,

            Connecting = 1,

            Connected = 2,

            ConnectFailed = 3,

            Disconnected = 4,

        }

        UDPSocket ClientSocket = null;


        /// <summary>
        /// The buffer size in KB.
        /// </summary>
        [Range(5, 50)]
        [Tooltip("The buffer size in KB.")]
        public int BufferKB = 5;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:UDPServer"/> dont destroy on load1.
        /// </summary>
        /// <value><c>true</c> if dont destroy on load1; otherwise, <c>false</c>.</value>
        public bool dontDestroyOnLoad { get { return m_dontDestroyOnLoad; } set { m_dontDestroyOnLoad = value; } }

        /// <summary>
        /// Gets client state.
        /// </summary>
        /// <value>The state of the client.</value>
        [InspectFunction]
        public UDPClientState ClientState { get; private set; }

        /// <summary>
        /// Is the client currently connected ?
        /// </summary>
        public override bool IsClientConnected
        {
            get
            {
                return ClientState == UDPClientState.Connected;
            }
        }

        /// <summary>
        /// Is the client connecting?
        /// </summary>
        public override bool IsClientConnecting
        {
            get
            {
                return ClientState == UDPClientState.Connecting;
            }
        }

        /// <summary>
        /// Is the connect disconnected ?
        /// </summary>
        public override bool IsClientDisconnected
        {
            get
            {
                return ClientState == UDPClientState.ConnectFailed || ClientState == UDPClientState.Disconnected || ClientState == UDPClientState.None;
            }
        }

        [SerializeField]
        private bool m_dontDestroyOnLoad;

        Coroutine clientCoroutine = null;

        private bool m_HeartBeat = false;

        /// <summary>
        /// The heart beat interval.
        /// </summary>
        private float m_HeartBeatInterval;

        private float m_LastRefreshHeatBeatTime = 0;

        [Header(" --- Debug ---")]
        public bool DebugLog = false;

        private void Awake()
        {
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
            ClientState = UDPClientState.None;
        }

        /// <summary>
        /// Starts the UDP client.
        /// Client state turns to Connecting.
        /// </summary>
        [InspectFunction]
        public override async Task<bool> StartClient()
        {
            if (ClientState != UDPClientState.None && ClientState != UDPClientState.Disconnected && ClientState != UDPClientState.ConnectFailed)
            {
                Debug.Log("Client UDP already started.");
                return false;
            }
            //Client state => connecting
            ClientState = UDPClientState.Connecting;

            ClientSocket = new UDPSocket(BufferKB * 1024);

            ClientSocket.OnClientIsConnected += ClientSocket_OnClientIsConnected;
            //Starts client connection:
            try
            {
                this.InvokeOnClientConnecting();
                await ClientSocket.Client(ServerAddress, ServerPort);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            //Start daemon:
            if(ClientState == UDPClientState.Connected)
            {
                clientCoroutine = StartCoroutine(Daemon());
                this.InvokeOnClientConnected();
            }
            else
            {
                this.InvokeOnClientDisconnect();
            }

            return ClientState == UDPClientState.Connected;
        }

        /// <summary>
        /// On client socket connected.
        /// </summary>
        void ClientSocket_OnClientIsConnected()
        {
            ClientSocket.OnClientIsConnected -= ClientSocket_OnClientIsConnected;
            this.m_HeartBeatInterval = ClientSocket.HeartBeatInterval;
            this.m_HeartBeat = ClientSocket.heartBeat;
        }


        /// <summary>
        /// Stops the udp client, shutdown the udp socket and dispose it.
        /// </summary>
        [InspectFunction]
        public override void StopClient()
        {
            if(ClientSocket != null)
               this.ClientSocket.DisconnectClient();
        }

        private void OnDestroy()
        {
            if (clientCoroutine != null)
                StopCoroutine(clientCoroutine);
        }


        /// <summary>
        /// Daemon coroutine : monitor client running state.
        /// </summary>
        /// <returns>The running.</returns>
        IEnumerator Daemon()
        {
            bool loop1 = true;
            while (loop1)
            {
                if (ClientSocket.clientSocketState == UDPSocket.ClientUDPSocketState.Connecting)
                {
                    ClientState = UDPClientState.Connecting;
                    this.InvokeOnClientConnecting();
                    yield return null;
                }
                else if (ClientSocket.clientSocketState == UDPSocket.ClientUDPSocketState.ConnectFailed)
                {
                    loop1 = false;
                    ClientState = UDPClientState.Disconnected;
                    Debug.LogFormat("UDP Client state => {0}", ClientState);
                    this.InvokeOnClientDisconnect();
                }
                else if (ClientSocket.clientSocketState == UDPSocket.ClientUDPSocketState.Connected)
                {
                    loop1 = false;
                    ClientState = UDPClientState.Connected;
                    Debug.LogFormat("UDP Client state => {0}", ClientState);
                    this.InvokeOnClientConnected();
                }
            }

            //Check connected ==> disconnected
            if (ClientSocket.clientSocketState == UDPSocket.ClientUDPSocketState.Connected)
            {
                bool loop2 = true;
                WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();
                while (loop2)
                {
                    //Handle auto-reconnect after disconnect:
                    if (ClientSocket.clientSocketState == UDPSocket.ClientUDPSocketState.Disconnected)
                    {
                        ClientState = UDPClientState.Disconnected;
                        loop2 = false;
                        this.InvokeOnClientDisconnect();
                        Debug.LogFormat("UDP Client state => {0}", ClientState);

                        if (AutoReconnect)
                        {
                            Invoke("StartClient", 0.1f);//reconnect a bit later.
                        }
                    }
                    else
                    {
                        //Handle recvieve packets:
                        lock (ClientSocket.recvPackets)
                        {
                            if (ClientSocket.recvPackets.Count > 0)
                            {
                                for (int i = 0, ServerSocketserverRecvPacketsCount = ClientSocket.recvPackets.Count; i < ServerSocketserverRecvPacketsCount; i++)
                                {
                                    var packet = ClientSocket.recvPackets[i];
                                    if (DebugLog)
                                    {
                                        Debug.LogFormat("Client receive {0} bytes.", packet.Data.Length);
                                    }
                                    this.InvokeOnReceiveData(packet);
                                }
                                this.InvokeOnFlushData(ClientSocket.recvPackets);
                                ClientSocket.recvPackets.Clear();
                            }
                        }
                        if (this.m_HeartBeat && (Time.time - m_LastRefreshHeatBeatTime) >= m_HeartBeatInterval)
                        {
                            this.ClientSocket.UpdateHeartBeat();
                            m_LastRefreshHeatBeatTime = Time.time;
                        }
                        this.ClientSocket.UnityTime = Time.time;
                        this.ClientSocket.UnityRealTime = Time.realtimeSinceStartup;
                        yield return waitForFixedUpdate;
                    }
                }
            }


            clientCoroutine = null;
        }

        /// <summary>
        /// Sends text over the UDP client.
        /// Note : this is synchronous operation.
        /// </summary>
        /// <param name="text">Text.</param>
        [InspectFunction]
        public void Send(string text)
        {
            if (ClientState == UDPClientState.Connected)
            {
                ClientSocket.SendSync(System.Text.Encoding.UTF8.GetBytes(text));
            }
            else
            {
                Debug.LogErrorFormat(this, "Can't send when client state == {0}", ClientState);
            }
        }

        /// <summary>
        /// Send the specified buffer over the UDP client.
        /// </summary>
        /// <param name="Message">Buffer.</param>
        public override void Send(byte[] Message)
        {
            if (ClientState == UDPClientState.Connected)
            {
                ClientSocket.SendSync(Message);
            }
            else
            {
                Debug.LogErrorFormat(this, "Can't send when client state == {0}", ClientState);
            }
        }

        /// <summary>
        /// Send the specified buffer over the UDP client.
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        /// <param name="StartOffset">Start offset.</param>
        /// <param name="Length">Length.</param>
        public void Send(byte[] buffer, int StartOffset, int Length)
        {
            if (ClientState == UDPClientState.Connected)
            {
                ClientSocket.SendSync(buffer, StartOffset, Length);
            }
            else
            {
                Debug.LogErrorFormat(this, "Can't send when client state == {0}", ClientState);
            }
        }

        /// <summary>
        /// Sends text over the UDP client.
        /// Note : this is asynchronous operation.
        /// </summary>
        /// <param name="text">Text.</param>
        [InspectFunction]
        public void SendAsync(string text)
        {
            if (ClientState == UDPClientState.Connected)
            {
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                ClientSocket.SendAsync(System.Text.Encoding.UTF8.GetBytes(text));
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            }
            else
            {
                Debug.LogErrorFormat(this, "Can't send when client state == {0}", ClientState);
            }
        }

        /// <summary>
        /// Send the specified buffer over the UDP client.
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        public void SendAsync(byte[] buffer)
        {
            if (ClientState == UDPClientState.Connected)
            {
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                ClientSocket.SendAsync(buffer);
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            }
            else
            {
                Debug.LogErrorFormat(this, "Can't send when client state == {0}", ClientState);
            }
        }


        /// <summary>
        /// Send the specified buffer over the UDP client.
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        public void SendAsync(byte[] buffer, int offset, int size)
        {
            if (ClientState == UDPClientState.Connected)
            {
                ClientSocket.SendAsync(buffer, offset, size);
            }
            else
            {
                Debug.LogErrorFormat(this, "Can't send when client state == {0}", ClientState);
            }
        }
    }
}