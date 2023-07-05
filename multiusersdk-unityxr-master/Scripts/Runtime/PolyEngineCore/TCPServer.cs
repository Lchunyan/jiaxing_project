using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using Ximmerse.XR.Asyncoroutine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Text;

namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// Tcp server wrapper.
    /// </summary>
    public class TCPServer : NetServer
    {
        [SerializeField]
        float m_DropTimeout = 9;

        /// <summary>
        /// Drop timeout.
        /// </summary>
        public float DropTimeout
        {
            get
            {
                return m_DropTimeout;
            }
            set
            {
                m_DropTimeout = value;
            }
        }

        /// <summary>
        /// 使用原始 Socket 通信， 不会附加 MessageHeader， 不会使用结构化的通信方式。
        /// </summary>
        [SerializeField, Tooltip("使用原始 Socket 通信， 不会附加 MessageHeader， 不会使用结构化的通信方式。")]
        bool m_UsingRawSocket = false;

        /// <summary>
        /// 使用原始 Socket 通信， 不会附加 MessageHeader， 不会使用结构化的通信方式。
        /// </summary>
        public bool UsingRawSocket
        {
            get
            {
                return m_UsingRawSocket;
            }
            set
            {
                m_UsingRawSocket = value;
                for (int i = 0, AcceptedClientsCount = AcceptedClients.Count; i < AcceptedClientsCount; i++)
                {
                    var client = AcceptedClients[i];
                    client.UsingRawSocket = value;
                }
            }
        }


        /// <summary>
        /// Sets the socket option to keep it alive ?
        /// </summary>
        public bool KeepAlive = false;

        /// <summary>
        /// 服务器状态.
        /// </summary>
        public enum ServerState
        {
            NotRunning = 0,
            Running = 1,
        }

        TcpListener tcpListener;

        /// <summary>
        /// 监听超时
        /// </summary>
        const float kAcceptTimeout = 1;

        [InspectFunction]
        public ServerState state
        {
            get; private set;
        }

        [InspectFunction]
        public bool IsListening
        {
            get; private set;
        }

        /// <summary>
        /// For thread safety access to AcceptedClients
        /// </summary>
        object mutexAcceptClients = new object();

        /// <summary>
        /// All of the accept clients.
        /// </summary>
        List<TcpSocketWrapper> AcceptedClients = new List<TcpSocketWrapper>();

        /// <summary>
        /// 所有收到的数据包.
        /// </summary>
        List<NetworkDataPacket> RecvDataPackets = new List<NetworkDataPacket>();

        List<byte[]> RecvRawMsg = new List<byte[]>();

        [InspectFunction]
        public override int ConnectionCount
        {
            get
            {
                return AcceptedClients != null ? AcceptedClients.Count : 0;
            }
        }

        /// <summary>
        /// 是否对新建立的client连接，打开heartbeat 心跳包机制.
        /// </summary>
        [SerializeField]
        bool m_Heartbeat = true;

        public bool HeartBeat
        {
            get => m_Heartbeat;
            set
            {
                m_Heartbeat = value;
                foreach (var c in AcceptedClients)
                {
                    if (c != null)
                    {
                        c.doHeartBeat = value;
                    }
                }
            }
        }

        [SerializeField]
        bool m_DropClientOnException = true;

        /// <summary>
        /// 是否在server和client通信的时候，如果出现了 tcp socket error，则断开和client的连接。
        /// </summary>
        public bool DropClientOnException
        {
            get => m_DropClientOnException;
            set
            {
                m_DropClientOnException = value;
            }
        }

        /// <summary>
        /// is server running?
        /// </summary>
        [InspectFunction]
        public override bool IsServerRunning
        {
            get
            {
                return this.state == ServerState.Running;
            }
        }

        /// <summary>
        /// is server running?
        /// </summary>
        [InspectFunction]
        public override bool AcceptNewClient
        {
            get
            {
                return this.IsListening;
            }
        }

        public bool DebugMode = false;

        /// <summary>
        /// If true, server's accept action will be cancelled
        /// </summary>
        bool CancelAccept;

        CancellationTokenSource CancellAcceptTokenSource;

        /// <summary>
        /// 新建立的连接 (for thread safe acceess)
        /// </summary>
        List<I_Connection> newSetupConnections_thread_safe = new List<I_Connection>();

        /// <summary>
        /// Unity time for multi-thread access
        /// </summary>
        float UnityTime;

        private void Awake()
        {
            state = ServerState.NotRunning;
            CancelAccept = false;
            CancellAcceptTokenSource = new CancellationTokenSource();
            UnityTime = Time.time;
            SocketLLMgr.CreateInstance();
        }

        private void Start()
        {
            UnityTime = Time.time;
            if (AutoStart)
            {
                StartServer();
            }
        }

        private void OnDestroy()
        {
            StopServer();
        }

        /// <summary>
        /// Starts server.
        /// </summary>
        /// <returns></returns>
        [InspectFunction]
        public override bool StartServer()
        {
            if (state == ServerState.Running)
            {
                Debug.Log("Server already started.");
                return false;
            }
            //IPHostEntry host = null;
            IPAddress localIP = null;
            IPEndPoint localEndPoint = null;
            //string localIPAddress = string.Empty;
            //try
            //{
            //    host = Dns.GetHostEntry(Dns.GetHostName());
            //}
            //catch (Exception exce)
            //{
            //    Debug.LogException(exce);
            //    return false;
            //}
            try
            {
                //for (int i = 0, hostAddressListLength = host.AddressList.Length; i < hostAddressListLength; i++)
                //{
                //    var ip = host.AddressList[i];
                //    if (ip.AddressFamily == AddressFamily.InterNetwork)
                //    {
                //        _localIP = ip;
                //        string ipaddr = ip.ToString();
                //        byte[] bytes = _localIP.GetAddressBytes();
                //        localIPAddress = ip.ToString();
                //        break;
                //    }
                //}
                PENetworkUtils.GetLocalIP(out localIP);
                localEndPoint = new IPEndPoint(localIP, ServerPort);
            }
            catch (Exception exc)
            {
                Debug.LogException(exc);
                return false;
            }

            try
            {
                tcpListener = new TcpListener(localEndPoint);
                tcpListener.Start(MaxConnection);//start listening.

                /* Exception on iOS
            SocketException: No such host is known
at System.Net.Sockets.Socket.IOControl(System.Int32 ioControlCode, System.Byte[] optionInValue, System.Byte[] optionOutValue)[0x00000] in < 00000000000000000000000000000000 >:0
  at System.Net.Sockets.Socket.IOControl(System.Net.Sockets.IOControlCode ioControlCode, System.Byte[] optionInValue, System.Byte[] optionOutValue)[0x00000] in < 00000000000000000000000000000000 >:0
  */
                if (Application.platform != RuntimePlatform.IPhonePlayer)
                {
                    tcpListener.SetSocketKeepAlive(true, 1, 1);
                }

                Debug.LogFormat("TCP server starts up at : {0}", localEndPoint);
                state = ServerState.Running;
                ServerAddress = localIP.ToString();



                //reset the cancellation token source:
                if (CancellAcceptTokenSource != null)
                {
                    this.CancellAcceptTokenSource.Dispose();
                    CancellAcceptTokenSource = new CancellationTokenSource();
                }
                CancelAccept = false;
                //开启accept client 循环
                Task.Run(AcceptClientLoop);
                IsListening = true;


                return true;
            }
            catch (Exception exc)
            {
                Debug.LogException(exc);
                tcpListener.Stop();
                tcpListener = null;
                return false;
            }
        }

        /// <summary>
        /// Stops listener and drops all accepted connection.
        /// </summary>
        [InspectFunction]
        public override void StopServer()
        {
            //Stop tcp listener:
            if (this.state == ServerState.Running)
            {
                try
                {
                    StopListening();
                    for (int i = 0; i < AcceptedClients.Count; i++)
                    {
                        TcpSocketWrapper c = AcceptedClients[i];
                        c.Dispose();
                    }
                    AcceptedClients.Clear();
                    this.state = ServerState.NotRunning;
                    Debug.Log("Tcp server stops.");
                }
                catch (Exception exc)
                {
                    Debug.LogException(exc);
                }
            }
        }

        /// <summary>
        /// Start listening - 开启监听器。
        /// </summary>
        [InspectFunction]
        public bool StartListening()
        {
            if (state == ServerState.Running && !IsListening)
            {
                try
                {
                    tcpListener.Start(MaxConnection);
                    if (Application.platform != RuntimePlatform.IPhonePlayer)
                    {
                        tcpListener.SetSocketKeepAlive(true, 1, 1);
                    }

                    IsListening = true;
                    CancelAccept = false;
                    //开启accept client 循环
                    Task.Run(AcceptClientLoop);
                    return true;
                }
                catch (Exception exc)
                {
                    Debug.LogException(exc);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Stop listening - 停止监听器。但是保留已经建立了的连接。
        /// </summary>
        [InspectFunction]
        public void StopListening()
        {
            if (state == ServerState.Running && IsListening)
            {
                CancellAcceptTokenSource.Cancel();
                tcpListener.Stop();
                IsListening = false;
            }
        }

        /// <summary>
        /// 循环接收新的连接.
        /// </summary>
        /// <returns></returns>
        private void AcceptClientLoop()
        {
            Debug.Log("Starts accepting client loop()", this);
            while (!CancelAccept)
            {
                TcpClient newAcceptClient = null;
                try
                {
                    Task<TcpClient> acceptTask = tcpListener.AcceptTcpClientAsync();
                    int waitMillis = (int)TimeSpan.FromSeconds(kAcceptTimeout).TotalMilliseconds;
                    acceptTask.Wait(this.CancellAcceptTokenSource.Token);//如果 Cancel accept 调用了， 则会触发 OperationCanceledException 
                    newAcceptClient = acceptTask.Result;
                    if (this.KeepAlive && Application.platform != RuntimePlatform.IPhonePlayer)
                    {
                        newAcceptClient.SetSocketKeepAlive(true, 1, 1);
                    }
                    Debug.LogFormat("Server accept new client : {0}", newAcceptClient.Client.RemoteEndPoint.ToString());
                }
                //当 
                catch (OperationCanceledException cancelExc)
                {
                    Debug.LogException(cancelExc);
                    CancelAccept = true;
                }
                catch (Exception exc)
                {
                    Debug.LogException(exc);
                }

                if (CancelAccept)
                {
                    Debug.Log("server-accepting is cancelled");
                    break;
                }
                else
                {
                    //建立了新的连接
                    if (newAcceptClient != null && newAcceptClient.Client != null)
                    {
                        Debug.LogFormat("On server accept new client: {0}", newAcceptClient.Client.RemoteEndPoint.ToString());
                        lock (mutexAcceptClients)
                        {
                            TcpSocketWrapper newSocketWrapper = new TcpSocketWrapper(newAcceptClient, AcceptedClients.Count, UnityTime, unityContext: this, heartBeat: this.m_Heartbeat);
                            newSocketWrapper.UsingRawSocket = this.m_UsingRawSocket;//是否使用raw socket.
                            newSocketWrapper.DropConnectionTimeout = this.m_DropTimeout;
                            AcceptedClients.Add(newSocketWrapper);
                            this.newSetupConnections_thread_safe.Add(newSocketWrapper);//for c# event in unity main thread.
                            //Debug.LogFormat("On accept new client : {0}", newAcceptClient.Client.RemoteEndPoint.ToString());
                        }
                    }
                    else
                    {
                        Debug.Log("Accept none client, starting next loop");
                    }
                    Thread.Sleep(1);
                }
            }
        }

        [InspectFunction]
        public void DisconnectClient(int ClientID)
        {
            for (int i = 0; i < AcceptedClients.Count; i++)
            {
                var socketwrapper = AcceptedClients[i];
                if (socketwrapper.ID == ClientID)
                {
                    InvokeOnServerDisconnect(AcceptedClients[i]);
                    socketwrapper.Dispose();
                    AcceptedClients.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Send text to connected tcp client.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="ClientID"></param>
        [InspectFunction]
        public void SendTextToClient(string text, int ClientID)
        {
            GetClient(ClientID).PushOutgoingMessage(text);
        }

        /// <summary>
        /// Broadcasts the buffer to all connected tcp clients in asynchronous mode.
        /// </summary>
        /// <param name="text">Text.</param>
        [InspectFunction]
        public void BroadcastAsync(string text)
        {
            for (int i = 0; i < this.AcceptedClients.Count; i++)
            {
                var client = this.AcceptedClients[i];
                try
                {
                    client.PushOutgoingMessage(text);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        /// <summary>
        /// Sends message to connection, starts from offset , in length.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="Message"></param>
        /// <param name="Offset"></param>
        /// <param name="Length"></param>
        public override void SendTo(I_Connection connection, byte[] Message, int Offset, int Length)
        {
            for (int i = 0; i < AcceptedClients.Count; i++)
            {
                TcpSocketWrapper client = AcceptedClients[i];
                if (client == connection)
                {
                    try
                    {
                        client.PushOutgoingMessage(Message, Offset, Length);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Broadcasts the buffer in asynchronous mode.
        /// </summary>
        /// <param name="Message">Message to broadcast.</param>
        /// <param name="length">Length.</param>
        /// <param name="offset">Offset.</param>
        public override void BroadcastAsync(byte[] Message, int length, int offset)
        {
            for (int i = 0; i < AcceptedClients.Count; i++)
            {
                TcpSocketWrapper client = AcceptedClients[i];
                try
                {
                    client.PushOutgoingMessage(Message, offset, length);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        /// <summary>
        /// Broadcast a ping message to every connected clients.
        /// </summary>
        [InspectFunction]
        public void Ping()
        {
            var empty = new byte[] { };
            for (int i = 0; i < AcceptedClients.Count; i++)
            {
                var client = AcceptedClients[i];
                client.PushOutgoingMessage(empty);
            }
        }


        private void Update()
        {
            UnityTime = Time.time;
            if (state == ServerState.Running)
            {
                lock (mutexAcceptClients)
                {
                    //每帧循环, 对收到了消息数据的 socket 通道, 
                    for (int i = 0; i < AcceptedClients.Count; i++)
                    {
                        var socketwrapper = AcceptedClients[i];
                        if (socketwrapper.RecvMessageCount > 0)
                        {
                            //触发收到消息的事件 - OnRecvData
                            try
                            {
                                RecvDataPackets.Clear();
                                RecvRawMsg.Clear();
                                socketwrapper.FlushRawRecvData(RecvRawMsg);
                                for (int rawIndex = 0; rawIndex < RecvRawMsg.Count; rawIndex++)
                                {
                                    byte[] data = RecvRawMsg[rawIndex];

                                    NetworkDataPacket dataPacket = new NetworkDataPacket()
                                    {
                                        PeerAddress = socketwrapper.RemoteAddress,
                                        PeerPort = socketwrapper.RemotePort,
                                        Data = data,
                                    };
                                    dataPacket.endPoint = socketwrapper.ipEndpoint;
                                    RecvDataPackets.Add(dataPacket);
                                }

                            }
                            catch (Exception e)
                            {
                                Debug.LogException(e);
                            }
                        }

                        if (RecvDataPackets.Count > 0)
                        {
                            if (DebugMode)
                            {
                                Debug.LogFormat("总共收到来自: {0} 一共 {1} 条数据", socketwrapper.RemoteAddress + ":" + socketwrapper.RemotePort, RecvDataPackets.Count);
                            }

                            //C# 事件
                            for (int j = 0; j < RecvDataPackets.Count; j++)
                            {
                                var packet = RecvDataPackets[j];
                                this.InvokeOnReceiveData(packet);
                                //Debug.LogFormat("Recv: {0}", Encoding.UTF8.GetString (packet.Data));
                            }
                            this.InvokeOnFlushData(RecvDataPackets);
                            RecvDataPackets.Clear();
                        }
                    }


                    //检查 socket 的断连情况:
                    for (int i = AcceptedClients.Count - 1; i >= 0; i--)
                    {
                        bool error = false;
                        if (AcceptedClients[i].timeoutException)
                        {
                            if (this.DebugMode)
                                Debug.LogFormat("On tcp server time-out on connetion: {0}:{1}[ID:{2}], connect time: {3}", AcceptedClients[i].RemoteAddress, AcceptedClients[i].RemotePort, AcceptedClients[i].ID, Time.time - AcceptedClients[i].ConnectionTime);
                            error = true;
                        }

                        if (error)
                        {
                            //如果tcp client error，则断开连接并移除client列表
                            if (m_DropClientOnException)
                            {
                                if (this.DebugMode)
                                    Debug.LogFormat("On tcp server drop on connetion: {0}:{1}[ID:{2}], connect time: {3}", AcceptedClients[i].RemoteAddress, AcceptedClients[i].RemotePort, AcceptedClients[i].ID, Time.time - AcceptedClients[i].ConnectionTime);
                                InvokeOnServerDisconnect(AcceptedClients[i]);
                                AcceptedClients[i].Dispose();
                                AcceptedClients.RemoveAt(i);
                            }
                        }
                    }
                }

                //发出 c# 事件 : new connection.
                if (newSetupConnections_thread_safe.Count > 0)
                {
                    for (int i = 0, newSetupConnections_thread_safeCount = newSetupConnections_thread_safe.Count; i < newSetupConnections_thread_safeCount; i++)
                    {
                        var conn = newSetupConnections_thread_safe[i];
                        InvokeOnServerConnected(conn);
                    }
                    newSetupConnections_thread_safe.Clear();
                }
            }
        }

        /// <summary>
        /// Get socket client by client ID.
        /// </summary>
        /// <param name="ClientID"></param>
        /// <returns></returns>
        TcpSocketWrapper GetClient(int ClientID)
        {
            for (int i = 0; i < AcceptedClients.Count; i++)
            {
                if (AcceptedClients[i].ID == ClientID)
                {
                    return AcceptedClients[i];
                }
            }
            return null;
        }

        //[InspectFunction]
        //public void TestParseRead ()
        //{
        //    TcpSocketWrapper.ParseData(AcceptedClients[0].IncomingBytesList, out byte[] buffer3);

        //    if(buffer3 != null)
        //    {
        //        Debug.LogFormat("Parse buffer: {0}, remain bytes: {1}", buffer3.Length, AcceptedClients[0].IncomingBytesList.Count);
        //    }

        //}

        //[InspectFunction]
        //public void TestPrint2ndCache()
        //{
        //    Debug.LogFormat("2级缓存 : {0}", AcceptedClients[0].IncomingBytesList.Count);
        //}

        //[InspectFunction]
        //public void PrintRecevMessage()
        //{
        //    Debug.LogFormat("收到的消息数量 : {0}", AcceptedClients[0].RecvMessageCount);
        //}

    }
}
