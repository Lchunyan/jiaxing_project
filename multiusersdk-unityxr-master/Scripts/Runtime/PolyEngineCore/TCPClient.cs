using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using Ximmerse.XR.Asyncoroutine;
using System.Net.Sockets;
using System.Net;
using System;

namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// Tcp client wrapper
    /// </summary>
    public class TCPClient : NetClient
    {
        /// <summary>
        /// For async connecting state data.
        /// </summary>
        private class ConnectingState
        {
            public TcpClient Client { get; set; }
            public bool Success { get; set; }
        }


        /// <summary>
        /// Timeout for establishing connection
        /// </summary>
        [MinValue(.1f)]
        public float ConnectionTimeout = 10;

        /// <summary>
        /// Timeout for dropping connection
        /// </summary>
        [SerializeField, Tooltip("Timeout for dropping connection"), MinValue(0.1f)]
        float m_DropTimeout = 10;

        /// <summary>
        /// 超时断连时间 - 默认6秒。
        /// 在这个时间内，如没有收到任何信息， 则连接自动断开.
        /// </summary>
        public float DropTimeout
        {
            get => m_DropTimeout;
            set
            {
                m_DropTimeout = value;
                if (this.socketWrapper != null)
                {
                    socketWrapper.DropConnectionTimeout = value;
                }
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
                if (this.socketWrapper != null)
                {
                    socketWrapper.UsingRawSocket = value;
                }
            }
        }

        /// <summary>
        /// Sets the socket option to keep it alive ?
        /// </summary>
        public bool KeepAlive = false;

        /// <summary>
        /// Send buffer size, default value = 8192 (8Kb)
        /// </summary>
        [SerializeField, Tooltip("Send buffer size, default value = 8192 (8Kb)")]
        public int m_SendBufferSize = 8192;

        /// <summary>
        /// Send buffer size, default value = 8192 (8Kb)
        /// </summary>
        public int SendBufferSize
        {
            get
            {
                return m_SendBufferSize;
            }
            set
            {
                m_SendBufferSize = value;
                if (this.tcpClient != null && tcpClient.Client != null && tcpClient.Client.Connected)
                {
                    tcpClient.SendBufferSize = value;
                }
            }
        }

        public bool DebugMode = false;

        TcpClient tcpClient = null;

        /// <summary>
        /// Raw tcp client object.
        /// </summary>
        public TcpClient TcpClient { get => tcpClient; }

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
            }
        }

        TcpSocketWrapper socketWrapper = null;

        /// <summary>
        /// 所有收到的数据包.
        /// </summary>
        List<NetworkDataPacket> RecvDataPackets = new List<NetworkDataPacket>();

        /// <summary>
        /// Received raw byte[]
        /// </summary>
        List<byte[]> RecvRawMsg = new List<byte[]>();

        /// <summary>
        /// Is the client currently connected ?
        /// </summary>
        public override bool IsClientConnected
        {
            get
            {
                return this.clientState == TcpClientState.Connected;
            }
        }

        /// <summary>
        /// Is the client connecting?
        /// </summary>
        public override bool IsClientConnecting
        {
            get
            {
                return this.clientState == TcpClientState.Connecting;
            }
        }

        /// <summary>
        /// Is the connect disconnected ?
        /// </summary>
        public override bool IsClientDisconnected
        {
            get
            {
                return this.clientState == TcpClientState.NotConnect;
            }
        }

        private void DisConnect(TcpConnection DisConnetion)
        {
            Debug.LogFormat("Client DisConnect To Server,Server IP:{0},Server Port:{1}", DisConnetion.ClientIP, DisConnetion.Port);
            //reader.StopRead();
            socketWrapper = null;
            clientState = TcpClientState.NotConnect;
        }

        public enum TcpClientState
        {
            NotConnect = 0,

            Connecting,

            Connected,
        }

        /// <summary>
        /// TcpClient 的状态， 无连接/连接中/已经连接
        /// </summary>
        [InspectFunction]
        public TcpClientState clientState { get; private set; }

        public const int kDefaultSentTimeout = 3000;

        /// <summary>
        /// Connection setup time.
        /// </summary>
        public float ConnectionStartTime
        {
            get; private set;
        }

        private void Awake()
        {
            clientState = TcpClientState.NotConnect;
            SocketLLMgr.CreateInstance();//Make sure socket manager exists.
        }

        private void OnDestroy()
        {
            StopClient();
        }


        void Update()
        {
            //每帧循环, 对收到了消息数据的 socket 通道,
            if (this.clientState == TcpClientState.Connected)
            {
                if (this.socketWrapper.RecvMessageCount > 0)
                {
                    //触发收到消息的事件 - OnRecvData
                    try
                    {
                        RecvDataPackets.Clear();
                        RecvRawMsg.Clear();
                        socketWrapper.FlushRawRecvData(RecvRawMsg);
                        for (int rawIndex = 0; rawIndex < RecvRawMsg.Count; rawIndex++)
                        {
                            byte[] data = RecvRawMsg[rawIndex];

                            if (DebugMode)
                            {
                                Debug.LogFormat("Recv data from : {0}, data length = {1}", socketWrapper.RemoteAddress, data.Length);
                            }
                            NetworkDataPacket dataPacket = new NetworkDataPacket()
                            {
                                PeerAddress = socketWrapper.RemoteAddress,
                                PeerPort = socketWrapper.RemotePort,
                                Data = data,
                                endPoint = socketWrapper.ipEndpoint,
                            };
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
                        Debug.LogFormat(this, "[TCP Client] 总共收到来自: {0} 一共 {1} 条数据", socketWrapper.RemoteAddress + ":" + socketWrapper.RemotePort, RecvDataPackets.Count);
                    }

                    //C# event : on receive data packet.
                    for (int i = 0; i < RecvDataPackets.Count; i++)
                    {
                        this.InvokeOnReceiveData(RecvDataPackets[i]);
                    }

                    //C# event : on flush data packets:
                    InvokeOnFlushData(RecvDataPackets);

                    RecvDataPackets.Clear();
                }
            }


            //检查连接错误
            if (this.clientState == TcpClientState.Connected && socketWrapper != null)
            {
                bool error = false;
                //只有在心跳开关的时候才做超时判断:
                if (this.HeartBeat && socketWrapper.timeoutException)
                {
                    error = true;
                    Debug.LogErrorFormat("Tcp client timeout after {0} seconds of connection.", Time.time - ConnectionStartTime);
                }

                if (error)
                {
                    socketWrapper.Dispose();
                    clientState = TcpClientState.NotConnect;
                    this.InvokeOnClientDisconnect();
                    socketWrapper = null;
                }
            }
        }


        /// <summary>
        /// Start client.
        /// </summary>
        [InspectFunction]
        public async override Task<bool> StartClient()
        {
            try
            {
                if (clientState != TcpClientState.NotConnect)
                {
                    Debug.Log("TcpClient already connecte.");
                    return false;
                }

                tcpClient = new TcpClient();
                tcpClient.Client.SendTimeout = kDefaultSentTimeout;
                tcpClient.SendTimeout = kDefaultSentTimeout;

                if (this.KeepAlive)
                {
                    if (Application.platform != RuntimePlatform.IPhonePlayer)//doesn't work on iphone
                    {
                        tcpClient.SetSocketKeepAlive(true, 1, 1);
                    }
                }

                Debug.LogFormat("TCP client start client on : {0}:{1}",
                   ServerAddress, ServerPort);


                IPAddress ipAddr = IPAddress.Parse(ServerAddress);
                //Dns.Resolve("");


                Debug.LogFormat("TCP client parse IP Address: {0}",
                   ipAddr.ToString());
                int port = ServerPort;
                clientState = TcpClientState.Connecting;

                //On client is connecting
                this.InvokeOnClientConnecting();

                ConnectingState stateData = new ConnectingState { Client = tcpClient, Success = false };
                IAsyncResult result = tcpClient.BeginConnect(ipAddr, port, EndConnect
                    , stateData);

                var taskConn = Task.Run<bool>(() =>
                {
                    var signal = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(ConnectionTimeout));
                    //Debug.LogFormat("Tcp client connect timeout: {0}", !signal);
                    return signal;
                });
                await taskConn;
                bool timeout = !taskConn.Result;
                //连接超时，或者连接失败:
                if (timeout || stateData.Success == false)
                {
                    try
                    {
                        tcpClient.Dispose();
                        tcpClient = null;
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                    Debug.LogErrorFormat("Tcp client connect fail to remote: {0}:{1}", ServerAddress, port);
                    clientState = TcpClientState.NotConnect;
                    this.InvokeOnClientDisconnect();
                    return false;
                }
                else
                {
                    //Debug.LogFormat("Tcp client connect success to remote: {0}", tcpClient.Client.RemoteEndPoint.ToString());
                    clientState = TcpClientState.Connected;
                    tcpClient.Client.SendBufferSize = this.SendBufferSize;
                    this.socketWrapper = new TcpSocketWrapper(tcpClient, 0, Time.time, unityContext: this, heartBeat: this.m_Heartbeat);
                    socketWrapper.UsingRawSocket = this.m_UsingRawSocket;//是否使用原始 socket.
                    socketWrapper.DropConnectionTimeout = this.m_DropTimeout;
                    ConnectionStartTime = Time.time;

                    //keep-alive
                    if (this.KeepAlive)
                    {
                        if (Application.platform != RuntimePlatform.IPhonePlayer)//doesn't work on iphone
                        {
                            tcpClient.SetSocketKeepAlive(true, 1, 1);
                        }
                    }

                    this.InvokeOnClientConnected();
                    return true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }


        void EndConnect(IAsyncResult ar)
        {
            var state = (ConnectingState)ar.AsyncState;
            TcpClient client = state.Client;

            //如果 client == null, 意味着远端地址不可触
            if (client.Client == null)
            {
                state.Success = false;
                return;
            }

            try
            {
                //如果end connect异常，说明连接失败
                client.EndConnect(ar);
                state.Success = true;//标记状态。
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                state.Success = false;//标记状态。
            }
        }

        /// <summary>
        /// Stops client.
        /// </summary>
        [InspectFunction]
        public override void StopClient()
        {
            if (socketWrapper != null && clientState == TcpClientState.Connected)
            {
                Debug.LogFormat("Tcp client to server: {0}:{1} is stopped.", ServerAddress, ServerPort);
                //dispose tcp client:
                try
                {
                    this.socketWrapper.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex, this);
                }
                clientState = TcpClientState.NotConnect;
                this.InvokeOnClientDisconnect();
            }
        }

        /// <summary>
        /// Sends message to remote.
        /// The client must be connected prior to sending.
        /// </summary>
        /// <param name="Message"></param>
        public override void Send(byte[] Message)
        {
            if (tcpClient != null && clientState == TcpClientState.Connected)
            {
                //SocketLLMgr.PushTcpSendRequest(tcpClient, Message, 0, Message.Length, OnSent);
                socketWrapper.PushOutgoingMessage(Message, true);
            }
        }

        /// <summary>
        /// Send byte[] start at offset and write length = length;
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        //[InspectFunction]
        public void Send(byte[] data, int offset, int length)
        {
            if (tcpClient != null && clientState == TcpClientState.Connected)
            {
                //SocketLLMgr.PushTcpSendRequest(tcpClient, data, offset, length, OnSent);
                socketWrapper.PushOutgoingMessage(data, offset, length, true);
            }
        }

        /// <summary>
        /// Sends network packet.
        /// </summary>
        /// <param name="networkPacket"></param>
        public void Send(TiNetworkPacket networkPacket)
        {
            if (tcpClient != null && clientState == TcpClientState.Connected)
            {
                socketWrapper.PushOutgoingTinetPacket(networkPacket);
            }
        }

        /// <summary>
        /// Send string 
        /// </summary>
        /// <param name="Text"></param>
        [InspectFunction]
        public void SendText(string Text)
        {
            if (tcpClient != null && clientState == TcpClientState.Connected)
            {
                socketWrapper.PushOutgoingMessage(Text);
            }
        }

        /// <summary>
        /// Ping by sending an dummy message.
        /// </summary>
        [InspectFunction]
        public void Ping()
        {
            if (tcpClient != null && clientState == TcpClientState.Connected)
            {
                socketWrapper.PushOutgoingMessage(TcpSocketWrapper.kDummyMessage, false);
            }
        }

        /// <summary>
        /// 发送 TCP 消息的回调。
        /// </summary>
        /// <param name="socketError"></param>
        /// <param name="sentSize"></param>
        private void OnSent(SocketError socketError, int sentSize)
        {
            if (this.DebugMode || socketError != SocketError.Success)
            {
                Debug.LogFormat("Socket sent error code: {0}, sent bytes = {1}, server = {2}:{3}",
                    socketError, sentSize, this.ServerAddress, this.ServerPort);
            }
            //连接已经中断。
            if (socketError == SocketError.Shutdown)
            {
                this.clientState = TcpClientState.NotConnect;
                Debug.LogFormat("TCP client to {0}:{1} is stopped on socket shutdown.", this.ServerAddress, this.ServerPort);
                this.tcpClient.Dispose();
                this.tcpClient = null;
                clientState = TcpClientState.NotConnect;
                this.InvokeOnClientDisconnect();
            }
        }

        /// <summary>
        /// 异步 Poll.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        [InspectFunction]
        public async Task<KeyValuePair<bool, int>> Poll(float time = 3, SelectMode mode = SelectMode.SelectRead)
        {
            bool polled = false;
            int availableBytes = -1;
            await Task.Run(
                () =>
                {
                    polled = tcpClient.Client.Poll((int)(1000000 * time), SelectMode.SelectRead);
                    Debug.LogFormat("Poll: {0}, available; {1}, is connected: {2}", polled, tcpClient.Client.Available, tcpClient.Client.Connected);
                    availableBytes = tcpClient.Client.Available;
                }

                );
            return new KeyValuePair<bool, int>(polled, availableBytes);
        }
    }
}
