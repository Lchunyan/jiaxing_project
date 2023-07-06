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
    /// Network data packet.
    /// 网络数据包
    /// </summary>
    public struct NetworkDataPacket
    {
        public string PeerAddress;
        public int PeerPort;
        public byte[] Data;

        /// <summary>
        /// This field is internal used only.
        /// </summary>
        internal IPEndPoint endPoint;
    }

    /// <summary>
    /// UDP connection status.
    /// </summary>
    public class UDPConnectionStatus
    {
        public float LastHeartBeatTime = 0;

        public float ConnectionSetupTime = 0;

    }

    /// <summary>
    /// Internal UDP socket wrapper. Include sender & receiver.
    /// </summary>
    internal class UDPSocket : System.IDisposable
    {
        /// <summary>
        /// Is this UDP socket connected to a remote end point ?
        /// </summary>
        /// <value><c>true</c> if is client connected; otherwise, <c>false</c>.</value>
        internal bool isClientConnected { get; private set; }

        /// <summary>
        /// Is the UDP server running ?
        /// </summary>
        /// <value><c>true</c> if is server running; otherwise, <c>false</c>.</value>
        internal bool isServerRunning { get; private set; }

        /// <summary>
        /// Default timeout : 1s
        /// </summary>
        int m_Timeout = 1000;

        /// <summary>
        /// Heart beat connection timeout.
        /// </summary>
        float HeartBeatConnectionTimeOut = 2;

        /// <summary>
        /// The heart beat interval.
        /// </summary>
        public float HeartBeatInterval = 1;

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:PolyEngine.UnityNetworking.UDPSocket"/> has caught client exception.
        /// </summary>
        /// <value><c>true</c> if has client exception; otherwise, <c>false</c>.</value>
        public bool HasClientException  { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:PolyEngine.UnityNetworking.UDPSocket"/> has caught server exception.
        /// </summary>
        /// <value><c>true</c> if is server exception; otherwise, <c>false</c>.</value>
        public bool HasServerException { get; private set; }

        private Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private bool isDisposed = false;
        public int BufferSize = 1024 * 20;
        private State m_State;
        private State state
        {
            get
            {
                if(m_State == null)
                {
                    m_State = new State(BufferSize);
                }
                return m_State;
            }
        }
        private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
        private AsyncCallback recv = null;
        //data packet to identity hand-shake from client to server
        private static byte[] kClientHandShakePacket = new byte[]
        {
              0x00, 0x08, 0x12, 0x05,
        };
        //data packet to identity hand-shake from server to client
        private static byte[] kServerHandShakePacket = new byte[]
        {
              0x0A, 0xCF, 0x33, 0x9A, 0x00,/* heart beat flag*/   0x00,0x00,0x00,0x00 /* heart beat interval*/
        };
        //data packet to identity heart beat
        private static byte[] kHeartBeatPacket = new byte[]
        {
              0x2C, 0x7A, 0x99, 0x1B, 0x9F
        };


        IAsyncResult iAsyncReceiveFrom;

        /// <summary>
        /// Client socket exception.
        /// </summary>
        /// <value>The client socket exception.</value>
        public SocketException clientSocketException { get; private set; }

        /// <summary>
        /// Gets the server socket exception.
        /// </summary>
        /// <value>The server socket exception.</value>
        public SocketException serverSocketException { get; private set; }

        /// <summary>
        /// Event : on client is connected. 
        /// </summary>
        public event System.Action OnClientIsConnected = null;

        /// <summary>
        /// Event : on client connection failed.
        /// </summary>
        public event System.Action OnClientConnectionFailed = null;

        /// <summary>
        /// The received data packets.
        /// </summary>
        internal List<NetworkDataPacket> recvPackets = new List<NetworkDataPacket>();

        /// <summary>
        /// All peers' connection, in server only.
        /// </summary>
        public List<UDPConnection> allPeerConnections = new List<UDPConnection>();

        /// <summary>
        /// The new accept connections.
        /// Only available in server socket.
        /// </summary>
        public List<UDPConnection> newConnections = new List<UDPConnection>();

        private List<IPEndPoint> droppedConnections = new List<IPEndPoint>();

        /// <summary>
        /// The connection status map.
        /// Key = udp IP endpoint 
        /// Value = udp connection status
        /// </summary>
        public Dictionary<IPEndPoint, UDPConnectionStatus> ConnectionStatusMap = new Dictionary<IPEndPoint, UDPConnectionStatus>();

        /// <summary>
        /// Action on socket's async operation complete.
        /// </summary>
        public System.EventHandler<SocketAsyncEventArgs> OnAsyncCompleteAction = null;

        bool disposed = false;

        /// <summary>
        /// Gets the last update heartbeat time.
        /// </summary>
        /// <value>The last update heartbeat time.</value>
        public float LastUpdateHeartbeatTime
        {
            get; private set;
        }

        /// <summary>
        /// Client UDP socket state.
        /// </summary>
        public enum ClientUDPSocketState
        {
            None = 0,

            Connecting = 1,

            Connected = 2,

            ConnectFailed = 3,

            Disconnected = 4,
        }

        /// <summary>
        /// Gets the state of the client socket.
        /// </summary>
        /// <value>The state of the client socket.</value>
        public ClientUDPSocketState clientSocketState
        {
            get; private set;
        }

        /// <summary>
        /// Gets or sets the timeout of the UDP socket (default = 1s) .
        /// </summary>
        /// <value>The timeout.</value>
        public float timeout {
         get { return (m_Timeout / 1000); } 
         set { m_Timeout = (int)(value * 1000); } 
         }

        public bool heartBeat;

        /// <summary>
        /// The unity time.
        /// </summary>
        public float UnityTime;

        /// <summary>
        /// The unity real time.
        /// </summary>
        public float UnityRealTime;

        public class State
        {
            public byte[] buffer = null;
            public State (int bufferSize)
            {
                buffer = new byte[bufferSize];
            }
        }

        internal UDPSocket (int BufferSize = 1024 * 5)
        {
            this.BufferSize = BufferSize;
            clientSocketState = ClientUDPSocketState.None;
        }

        /// <summary>
        /// Starts the server.
        /// If the original port is occupied, and persist port == false, will auto increase the port number until the server start successfully.
        /// Return true if the server's been started.
        /// </summary>
        /// <param name="address">Address.</param>
        /// <param name="port">Port.</param>
        /// <param name="heartBeat">If set to <c>true</c> heart beat.</param>
        /// <param name="heartBeatTimeout">Heart beat timeout.</param>
        /// <param name="heartBeatInterval">Heart beat interval.</param>
        /// <param name="persistPort">If set to <c>true</c> persist port.</param>
        public bool StartServer(IPAddress address, ref int port, bool heartBeat = false, float heartBeatTimeout = 2, float heartBeatInterval = 1, bool persistPort = false)
        {
            isDisposed = false;
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            int bindTime = 0;
        Rebind:
            bindTime++;
            try
            {
                _socket.Bind(new IPEndPoint(address, port));
            }
            catch (SocketException socketException)
            {
                //when socket already in use, and persit port is turn off, tries to rebind for max to 10 times:
                if(socketException.SocketErrorCode == SocketError.AddressAlreadyInUse && persistPort == false && bindTime <= 10)//max rebind time : hard code to 10 times
                {
                    //When socket binds in error == already in used, tries to rebind
                    port += 10;//each rebind step = 10
                    goto Rebind;
                }
                else
                {
                    Debug.LogException(socketException);
                    return false;
                }
            }
            catch (System.Exception exc)
            {
                Debug.LogException(exc);
                return false;
            }
            //Start async-receives loop:
            AsyncRecvFrom(isServer : true);
            this.heartBeat = heartBeat;
            this.HeartBeatConnectionTimeOut = heartBeatTimeout;
            this.HeartBeatInterval = heartBeatInterval;
            isServerRunning = true;
            return true;
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        public void StopServer()
        {
            isDisposed = true;
            if (_socket != null)
            {
                _socket.Close();
            }
            isServerRunning = false;
        }

        /// <summary>
        /// Use this UDP socket as a client sender.
        /// </summary>
        /// <returns>The client.</returns>
        /// <param name="address">Address.</param>
        /// <param name="port">Port.</param>
        public async Task Client(string address, int port)
        {
            clientSocketState = ClientUDPSocketState.Connecting;
            var ipAddress = IPAddress.Parse(address);
            //connect sync:
            _socket.SendTimeout = m_Timeout;
            _socket.ReceiveTimeout = m_Timeout;
            _socket.Connect(ipAddress, port);
            HasClientException = false;
            await new WaitForGameTime(0.2f);
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            SendAsync(kClientHandShakePacket);//sends a hand shake message
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法

            EndPoint endPoint = new IPEndPoint(ipAddress, port);
            string exceptionMsg = string.Empty;
            IAsyncResult trySendResult = _socket.BeginReceiveFrom(state.buffer, 0, BufferSize, SocketFlags.None, ref epFrom, recv = (ar) =>
            {
                try
                {
                    State so = (State)ar.AsyncState;
                    int bytes = _socket.EndReceiveFrom(ar, ref epFrom);
                    byte[] recvBytes = so.buffer;
                    //expect a server - response :
                    if(IsServerHandShakePacket (recvBytes, bytes, out bool _heartBeat, out float _heartBeatInterval))
                    {
                        this.heartBeat = _heartBeat;
                        this.HeartBeatInterval = _heartBeatInterval;
                    }
                    else
                    {
                        throw new UnityException("Server response melform message, length:" + bytes);
                    }

                    //_socket.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv, so);
                    Debug.LogFormat("Client get server [{0}:{1}] hand shake response, heart beat : {2}, interval:{3}", epFrom.ToString(), bytes, heartBeat, this.HeartBeatInterval);
                }
                catch (SocketException socketExc)
                {
                    exceptionMsg = string.Format("Error socket code: {0}", socketExc.SocketErrorCode);
                    //For unconnectable UDP socket :
                    //socketExc.SocketErrorCode == SocketError.ConnectionRefused
                    HasClientException = true;
                    clientSocketException = socketExc;
                    if(clientSocketState == ClientUDPSocketState.Connecting)
                    {
                        clientSocketState = ClientUDPSocketState.ConnectFailed;
                    }
                    else if(clientSocketState == ClientUDPSocketState.Connected)
                    {
                        clientSocketState = ClientUDPSocketState.Disconnected;
                    }
                    else
                    {
                        clientSocketState = ClientUDPSocketState.Disconnected;
                    }
                }
                catch (UnityException unityExc)
                {
                    clientSocketState = ClientUDPSocketState.Disconnected;
                    Debug.LogException(unityExc);
                }
            }, state);
            await new WaitForGameTime(1.2f);
            if (!string.IsNullOrEmpty(exceptionMsg))
            {
                Debug.LogErrorFormat("UDP Client connect with exception : {0}", exceptionMsg);
            }
            //_socket.EndReceiveFrom(trySendResult, ref endPoint);
            //如果触发了 exception:
            if (HasClientException)
            {
                isClientConnected = false;
                clientSocketState = ClientUDPSocketState.ConnectFailed;
                OnClientConnectionFailed?.Invoke();
            }
            else
            {
                isClientConnected = true;
                clientSocketState = ClientUDPSocketState.Connected;
                OnClientIsConnected?.Invoke();

                AsyncRecvFrom();
            }

            //Debug.LogFormat("Client connects to : {0}, result: {1}", address, isClientConnected);
            //AsyncRecvFrom();

        }


        public void DisconnectClient ()
        {
            if(clientSocketState == ClientUDPSocketState.Connected)
            {
                try
                {
                    _socket.Disconnect(false);
                }
                catch (System.Exception  exc)
                {
                    Debug.LogException(exc);
                }

                try
                {
                    _socket.Close();
                    _socket.Dispose();
                }
                catch (System.Exception exc)
                {
                    Debug.LogException(exc);
                }

                isClientConnected = false;
                clientSocketState = ClientUDPSocketState.Disconnected;
            }
            else if (clientSocketState == ClientUDPSocketState.Connecting)
            {
                _socket.Close();
                _socket.Dispose();
                isClientConnected = false;
                clientSocketState = ClientUDPSocketState.Disconnected;
            }
            else //connect failed, disconnected, none
            {
                //do nothing
                isClientConnected = false;
            }
        }

        /// <summary>
        /// Sends the buffer async
        /// </summary>
        /// <param name="data">Data.</param>
        public async Task SendAsync(byte[] data)
        {
            IAsyncResult asyncResult = _socket.BeginSend(data, 0, data.Length, SocketFlags.None, (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = _socket.EndSend(ar);
                //Debug.LogFormat("SEND: {0} bytes @thread: {1}", bytes, System.Threading.Thread.CurrentThread.ManagedThreadId);
            }, state);
            //Debug.LogFormat("Send async starts @Time:{0} of frame: {1}", Time.realtimeSinceStartup, Time.frameCount);
            await Task.Run(asyncResult.AsyncWaitHandle.WaitOne);
            //Debug.LogFormat("Send async ends @Time:{0} of frame: {1}, is complete: {2}, state: {3}", Time.realtimeSinceStartup, Time.frameCount, asyncResult.IsCompleted, asyncResult.AsyncState.ToString());
        }


        /// <summary>
        /// Sends the buffer async
        /// </summary>
        /// <param name="data">Data.</param>
        public IAsyncResult SendAsync(byte[] data, int offset, int length)
        {
            IAsyncResult asyncResult = _socket.BeginSend(data, offset, length, SocketFlags.None, (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = _socket.EndSend(ar);
                //Debug.LogFormat("SEND: {0} bytes @thread: {1}", bytes, System.Threading.Thread.CurrentThread.ManagedThreadId);
            }, state);
            return asyncResult;
        }

        /// <summary>
        /// Sends the data in sync mode.
        /// </summary>
        /// <param name="data">Data.</param>
        public void SendSync (byte[] data)
        {
            _socket.Send(data);
        }

        /// <summary>
        /// Sends the data in sync.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="length">Length.</param>
        public void SendSync(byte[] data, int offset, int length)
        {
            _socket.Send(data, offset, length, SocketFlags.None);
        }

        /// <summary>
        /// Sends the buffer synchronously to the IP end point.
        /// </summary>
        /// <param name="endPoint">End point.</param>
        /// <param name="data">Data.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="length">Length.</param>
        public void SendSync (IPEndPoint endPoint, byte[] data, int offset, int length)
        {
            _socket.SendTo(data, offset, length, SocketFlags.None, endPoint);
        }

        /// <summary>
        /// Sends the buffer asynchronously to the IP end point.
        /// Return async result.
        /// </summary>
        /// <param name="endPoint">End point.</param>
        /// <param name="data">Data.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="length">Length.</param>
        public IAsyncResult SendAsync(IPEndPoint endPoint, byte[] data, int offset, int length)
        {
            IAsyncResult asyncResult = _socket.BeginSendTo(data, offset, length, SocketFlags.None, endPoint, this.OnAsyncSendEnd, _socket);
            return asyncResult;
        }

        private void AsyncRecvFrom(bool isServer = false)
        {
            iAsyncReceiveFrom =
            _socket.BeginReceiveFrom(state.buffer, 0, BufferSize, SocketFlags.None, ref epFrom, recv = (ar) =>
            {
                try 
                {
                    if (isDisposed)
                    {
                        return;
                    }
                    State so = (State)ar.AsyncState;
                    int bytes = _socket.EndReceiveFrom(ar, ref epFrom);

                    byte[] data = bytes > 0 ? new byte[bytes] : new byte[0];
                    if(bytes > 0)
                    {
                        System.Array.Copy(so.buffer, data, bytes);
                        //For hand shake packet :
                        if (isServer && IsHandShakeFromClientPacket(data))
                        {
                            IPEndPoint ipEP = (IPEndPoint)epFrom;
                            UDPConnection newConnection = new UDPConnection(_socket, ipEP, UnityTime);
                            allPeerConnections.Add(newConnection);
                            UDPConnectionStatus uDPConnectionStatus = null;
                            if(ConnectionStatusMap.TryGetValue(ipEP, out uDPConnectionStatus))
                            {
                                uDPConnectionStatus.ConnectionSetupTime = UnityTime;
                                uDPConnectionStatus.LastHeartBeatTime = UnityTime;
                            }
                            else
                            {
                                uDPConnectionStatus = new UDPConnectionStatus();
                                uDPConnectionStatus.ConnectionSetupTime = UnityTime;
                                uDPConnectionStatus.LastHeartBeatTime = UnityTime;
                                ConnectionStatusMap.Add(ipEP, uDPConnectionStatus);
                            }
                           

                            //sends a server-response packet:
                            SocketAsyncEventArgs e = newConnection.asyncEventArgs;
                            e.RemoteEndPoint = ipEP;
                            byte[] serverHandShakeBytes = new byte[kServerHandShakePacket.Length];
                            //copy header of server hand shake bytes
                            System.Array.Copy(kServerHandShakePacket, serverHandShakeBytes, kServerHandShakePacket.Length);
                            //send heart beat datas to client:
                            serverHandShakeBytes[4] = this.heartBeat ? (byte)1 : (byte)0;
                            if(heartBeat)
                            {
                                PEUtils.WriteFloat(this.HeartBeatInterval, serverHandShakeBytes, 5);
                            }

                            e.SetBuffer(serverHandShakeBytes, 0, serverHandShakeBytes.Length);
                            bool asyncSent = _socket.SendToAsync(e);
                            string peerAddress = ipEP.Address.ToString();
                            lock (newConnections)
                            {
                                newConnections.Add(newConnection);
                            }
                            Debug.LogFormat("Hand-Shake IP : {0}:{1}", peerAddress, ipEP.Port);
                        }
                        //for heart beat packet :
                        else if (IsHeartBeatPacket (data))
                        {
                            IPEndPoint ipEP = (IPEndPoint)epFrom;
                            //Update heart beat time:
                            if(ConnectionStatusMap.TryGetValue (ipEP, out UDPConnectionStatus uDPConnectionStatus))
                            {
                                if(uDPConnectionStatus != null)
                                {
                                    uDPConnectionStatus.LastHeartBeatTime = UnityTime;//update the last heart beat time
                                }
                            }
                            //Add heart beat packet:
                            else
                            {
                                ConnectionStatusMap.Add(ipEP, new UDPConnectionStatus() { ConnectionSetupTime = UnityTime, LastHeartBeatTime = UnityTime, });
                            }
                            //Debug.LogFormat("Server recv heart beat from: [{0}:{1}]", ipEP.Address.ToString(), ipEP.Port);
                        }
                        //For data packet :
                        else
                        {
                            lock (recvPackets)
                            {
                                IPEndPoint ipEP = (IPEndPoint)epFrom;
                                recvPackets.Add(new NetworkDataPacket()
                                {
                                    PeerAddress = ipEP.Address.ToString(),
                                    PeerPort = ipEP.Port,
                                    Data = data,
                                });
                            }
                        }
                        //Loop next recv from:
                        //Debug.LogFormat("RECV: {0}: {1}, {2}", epFrom.ToString(), bytes, Encoding.UTF8.GetString(so.buffer, 0, bytes));
                        _socket.BeginReceiveFrom(so.buffer, 0, BufferSize, SocketFlags.None, ref epFrom, recv, so);
                    }

                }
                catch (SocketException socketExc)
                {
                    Debug.LogFormat("Error socket code: {0}", socketExc.SocketErrorCode);
                    Debug.LogException(socketExc);
                    serverSocketException = socketExc;
                    //For unconnectable UDP socket :
                    //socketExc.SocketErrorCode == SocketError.ConnectionRefused
                    HasServerException = true;

                    isServerRunning = false;

                    if(clientSocketState == ClientUDPSocketState.Connected)
                    {
                        clientSocketState = ClientUDPSocketState.Disconnected;
                    }
                }
            }, state);
        }

        /// <summary>
        /// Is the packet from client to server.
        /// </summary>
        /// <returns><c>true</c>, if hand shake from client packet was ised, <c>false</c> otherwise.</returns>
        /// <param name="packet">Packet.</param>
        static bool IsHandShakeFromClientPacket (byte[] packet)
        {
            bool isHandShake = true;
            if (packet.Length == kClientHandShakePacket.Length)
            {
                for(int i=0; i<packet.Length; i++)
                {
                    if(packet[i] != kClientHandShakePacket[i])
                    {
                        isHandShake = false;
                    }
                }
            }
            else
            {
                isHandShake = false;
            }
            return isHandShake;
        }

        /// <summary>
        /// Gets the connection status by IP endpoint.
        /// </summary>
        /// <returns>The connection status.</returns>
        /// <param name="endPoint">End point.</param>
        private UDPConnectionStatus GetConnectionStatus (IPEndPoint endPoint)
        {
            foreach (var ip in this.ConnectionStatusMap.Keys)
            {
                if(ip == endPoint)
                {
                    return ConnectionStatusMap[ip];
                }
            }
            return null;
        }

        /// <summary>
        /// IS this heart beat packet ?
        /// </summary>
        /// <returns><c>true</c>, if heart beat packet was ised, <c>false</c> otherwise.</returns>
        /// <param name="packet">Packet.</param>
        static bool IsHeartBeatPacket (byte[] packet)
        {
            bool _isHeartBeatPacket = true;
            if (packet.Length == kHeartBeatPacket.Length)
            {
                for (int i = 0; i < packet.Length; i++)
                {
                    if (packet[i] != kHeartBeatPacket[i])
                    {
                        _isHeartBeatPacket = false;
                    }
                }
            }
            else
            {
                _isHeartBeatPacket = false;
            }
            return _isHeartBeatPacket;
        }

        /// <summary>
        /// Is the packet indicating this is a server hand shake packet ? 
        /// If true, output the heart beat flag and the interval.
        /// </summary>
        /// <returns><c>true</c>, if server hand shake packet was ised, <c>false</c> otherwise.</returns>
        /// <param name="buffer">Packet.</param>
        /// <param name="HeartBeat">If set to <c>true</c> heart beat.</param>
        /// <param name="HeartBeatInterval">Heart beat interval.</param>
        static bool IsServerHandShakePacket (byte[] buffer, int bufferLength, out bool HeartBeat, out float HeartBeatInterval)
        {
            bool _isServerHandShake = true;
            if (bufferLength == kServerHandShakePacket.Length)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (buffer[i] != kServerHandShakePacket[i])
                    {
                        _isServerHandShake = false;
                    }
                }
                HeartBeat = buffer[4] != 0;
                if(HeartBeat)
                {
                    HeartBeatInterval = PEUtils.ReadFloat(buffer, 5);
                }
                else
                {
                    HeartBeatInterval = 0;
                }
            }
            else
            {
                HeartBeat = false;
                HeartBeatInterval = 0;
                _isServerHandShake = false;
            }
            return _isServerHandShake;
        }

        private void EndRecvFrom()
        {
            _socket.EndReceive(iAsyncReceiveFrom);
        }

        /// <summary>
        /// Broadcasts text message to all peers.
        /// Called in server udp only.
        /// </summary>
        public void BroadcastToAllPeers (byte[] buffer, int count, int offset = 0)
        {
            for (int i = 0, allPeerConnectionsCount = allPeerConnections.Count; i < allPeerConnectionsCount; i++)
            {
                var conn = allPeerConnections[i];
                //SocketAsyncEventArgs e = conn.asyncEventArgs;
                //e.RemoteEndPoint = conn.ipEndpoint;
                //e.SetBuffer(buffer, offset, count);
                int sentCount = _socket.SendTo(buffer, offset, count, SocketFlags.None, conn.ipEndpoint);
            }
        }

        /// <summary>
        /// Broadcasts the message asynchronously.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="count">Count.</param>
        /// <param name="offset">Offset.</param>
        public void BroadcastAsync(byte[] message, int count, int offset = 0, List<IAsyncResult> asyncResults = null)
        {
            for (int i = 0, allPeerConnectionsCount = allPeerConnections.Count; i < allPeerConnectionsCount; i++)
            {
                var conn = allPeerConnections[i];
                IAsyncResult asyncResult = _socket.BeginSendTo(message, offset, count, SocketFlags.None, conn.ipEndpoint, OnAsyncSendEnd, _socket);
                if (asyncResults != null)
                {
                    asyncResults.Add(asyncResult);
                }
            }
        }

        void OnAsyncSendEnd (IAsyncResult result)
        {
            Socket socket = (Socket)result.AsyncState;
            socket.EndSendTo(result);
        }

        /// <summary>
        /// Updates the heart beat : for server, drop lost connection. 
        /// For Client : sends heart beat bytes.
        /// </summary>
        public void UpdateHeartBeat()
        {
            //For server, check timeout connections
            if(isServerRunning)
            {
                droppedConnections.Clear();
                foreach (var key in ConnectionStatusMap.Keys)
                {
                    var conn = ConnectionStatusMap[key];
                    if((Time.time - conn.ConnectionSetupTime) >= HeartBeatConnectionTimeOut && (Time.time - conn.LastHeartBeatTime) >= HeartBeatConnectionTimeOut)
                    {
                        //Drop :
                        //ConnectionStatusMap.Remove(key);
                        droppedConnections.Add(key);
                    }
                }
                //drop the connection timeout keys:
                if(droppedConnections.Count > 0)
                {
                    for(int i=0; i < droppedConnections.Count; i++)
                    {
                        DropConnection(droppedConnections[i]);
                    }
                }
                LastUpdateHeartbeatTime = Time.time;
            }
            //For client, send heart beat bytes:
            else if(isClientConnected)
            {
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                SendAsync(kHeartBeatPacket);
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                LastUpdateHeartbeatTime = Time.time;
            }
        }

        private void DropConnection (IPEndPoint ipEndPoint)
        {
            ConnectionStatusMap.Remove(ipEndPoint);
            for (int i = this.allPeerConnections.Count - 1; i >= 0; i--)
            {
                if (allPeerConnections[i].ipEndpoint == ipEndPoint)
                {
                    allPeerConnections.RemoveAt(i);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                if (_socket != null && (_socket.IsBound || _socket.Connected))
                {
                    _socket.Close();
                    _socket.Dispose();
                }
            }

            disposed = true;
        }
    }

}