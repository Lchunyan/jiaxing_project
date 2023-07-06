using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using Ximmerse.XR.Reflection;
using System;
using System.Net;
using System.Reflection;
using System.Linq;
using Ximmerse.XR.Asyncoroutine;
using Ximmerse.XR.UnityNetworking.InternalMessages;


namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// Ti-Net, p2p solution provider.
    /// </summary>
    public partial class TiNet : MonoBehaviour, TiNetMessageHandler
    {
        private const string kWarningMsg01 = "Missing handler of message code : {0} ";
        private const string kLogMsg01 = "Ti-Node begin broadcasting : {0}";
        private const string kLogMsg02 = "Register TiMessage01 callback : {0}.{1} on msg code: {2}";
        private const string kLogMsg03 = "On Ti-Node dropped: {0}";
        private const string kLogMsg04 = "[TiNet] start : {0}";
        private const string kLogMsg05 = "Register TiMessage02 callback : {0}.{1} on msg code: {2}";
        private const string kLogMsg06 = "Recv packet : {0} from address: {1}";
        private const string kLogMsg07 = "Adds Ti-Node:{0}, ID:{1} result: {2}";
        private const string kLogMsg08 = "Start Tcp client to : {0}:{1}";

        private const string kErrorMsg01 = "Msg code: {0} has been registered already: {1}";
        private const string kErrorMsg02 = "Msg code: {0} has been registered already: {1}";
        private const string kErrorMsg03 = "Msg code :{0} handler unfound !";
        private const string kErrorMsg04 = "TiNet initialize with error";
        private const string kErrorMsg05 = "TiNet start network failed.";
        private const string kErrorMsg06 = "TCP Server failed to started !";
        static TiNet instance;

        public static bool HasInstance
        {
            get
            {
                return instance != null;
            }
        }

        public static TiNet Instance
        {
            get
            {
                if (!instance)
                {
                    instance = FindObjectOfType<TiNet>();
                }
                return instance;
            }
        }

        /// <summary>
        /// If auto start == true, the TiNet node will automatically starts networking.
        /// If false, call "StartNetwork" to start TiNet.
        /// </summary>
        public bool AutoStart = true;

        /// <summary>
        /// Custom tag of the ti node.
        /// </summary>
        [Tooltip("Custom tag of the TiNode")]
        public string CustomTag = string.Empty;

        /// <summary>
        /// Port for network broadcasting discovery.
        /// </summary>
        public int PortBroadcast = 10056;

        /// <summary>
        /// Port for reliable (TCP) message
        /// </summary>
        public int PortReliable = 10098;

        /// <summary>
        /// Port for unreliable (UDP) message
        /// </summary>
        public int PortUnreliable = 10088;

        /// <summary>
        /// strict mode.
        /// </summary>
        [SerializeField, Tooltip(" Strict mode 为 true : tinet node 之间使用  heartbeat 机制维持状态， 如果tcp sent error， 会导致 tinet node 切断连接。\r\n如果为false， tinet node之间没有 heart beat通信包， 如果tcp sent error， 不会切断连接。")]
        bool m_Strictmode = true;

        /// <summary>
        /// Strict mode 为 true : tinet node 之间使用  heartbeat 机制维持状态， 如果tcp sent error， 会导致 tinet node 切断连接。
        /// 如果为false， tinet node之间没有 heart beat通信包， 如果tcp sent error， 不会切断连接。
        /// </summary>
        public bool StrictMode
        {
            get => m_Strictmode;
            set
            {
                m_Strictmode = value;
            }
        }

        /// <summary>
        /// Verbose debug mode.
        /// </summary>
        public bool DebugMode = false;

        /// <summary>
        /// If true, detach tinet from network when application paused is invoked.
        /// </summary>
        [Tooltip("If true, detach tinet from network when application paused is invoked.")]
        public bool ShouldDetachNetworkOnAppPaused = false;

        public enum ConnectionFilterMode
        {
            /// <summary>
            /// 没有过滤器。只要有接受到广播就自动连接。
            /// </summary>
            NoFilter,

            /// <summary>
            /// 使用过滤器， 只有接受到对应广播关键字的节点才建立连接。
            /// </summary>
            UseFilterWord,
        }

        /// <summary>
        /// 过滤机制。
        /// </summary>
        public ConnectionFilterMode FilterMode = ConnectionFilterMode.NoFilter;

        /// <summary>
        /// 过滤关键字。如果是英文，不区分大小写。
        /// </summary>
        public string FilterWord = string.Empty;

        [SerializeField, Tooltip("是否广播局域网探索信息")]
        bool m_BroadcastDiscovery = true;

        /// <summary>
        /// 是否广播局域网探索信息。
        /// </summary>
        public bool BroadcastDiscovery
        {
            get
            {
                return m_BroadcastDiscovery;
            }
            set
            {
                m_BroadcastDiscovery = value;
            }
        }


        [SerializeField, Tooltip("是否自动根据监听加入网络。")]
        bool m_AutoJointNetwork = true;

        /// <summary>
        /// 是否自动根据监听加入网络。
        /// </summary>
        public bool AutoJointNetwork
        {
            get
            {
                return m_AutoJointNetwork;
            }
            set
            {
                m_AutoJointNetwork = value;
            }
        }

        /// <summary>
        /// UDP 广播监听器。
        /// </summary>
        UdpSocketListenerWrapper broadcastListener = null;

        /// <summary>
        /// UDP 广播器。
        /// </summary>
        UdpSocketSenderWrapper broadcastSender = null;

        /// <summary>
        /// UDP 发送器。
        /// </summary>
        UdpSocketSenderWrapper udpSender;

        /// <summary>
        /// Unreliable message UDP 监听器。
        /// </summary>
        UdpSocketListenerWrapper udpListener = null;

        /// <summary>
        /// The local tcp server.
        /// </summary>
        TCPServer tcpServer;

        /// <summary>
        /// Received packet for caching.
        /// </summary>
        List<TiNetworkPacket> PacketListCache = new List<TiNetworkPacket>();

        CancellationTokenSource cancellation;

        /// <summary>
        /// Ti-Net message handlers 01.
        /// Key = message code.
        /// Value = Message handler action.
        /// </summary>
        Dictionary<short, Action<TiNetMessage, IPEndPoint>> TiMessageHandlers01 = new Dictionary<short, Action<TiNetMessage, IPEndPoint>>();

        /// <summary>
        /// Ti-Net message handlers 02.
        /// Key = message code.
        /// Value = Message handler action.
        /// </summary>
        Dictionary<short, Action<TiNetMessage, I_TiNetNode>> TiMessageHandlers02 = new Dictionary<short, Action<TiNetMessage, I_TiNetNode>>();

        /// <summary>
        /// The machine name of the net node.
        /// </summary>
        string MachineName = string.Empty;

        int m_TiNetNodeID = -1;

        /// <summary>
        /// Ti-Net node ID.
        /// </summary>
        [InspectFunction]
        public int TiNetIDNodeID
        {
            get => m_TiNetNodeID; private set => m_TiNetNodeID = value;
        }

        /// <summary>
        /// The TiNet name of the current machine node.
        /// </summary>
        [InspectFunction]
        public string TiNetIDNodeName
        {
            get => MachineName;
            set
            {
                MachineName = value;
            }
        }

        /// <summary>
        /// 和这个 TiNet 相连的网络节点
        /// </summary>
        [SerializeField, DisableEditing]
        internal List<TiNetNode> m_nodes = new List<TiNetNode>();

        /// <summary>
        /// Unband Node List.
        /// </summary>
        List<TiNetNode> unbandNodes = new List<TiNetNode>();

        /// <summary>
        /// NodeName - TiNetNode 节点字典。
        /// Key = Node name,
        /// Value = Node reference.
        /// </summary>
        Dictionary<string, TiNetNode> nodeMap = new Dictionary<string, TiNetNode>();

        /// <summary>
        /// 此列表字段是Tinet收到的远端文件数据的封装器.
        /// 每次收到一个 packet index == 0 的 Transfer DataMessage , 此列表中就会插入一个数据对象。
        /// </summary>
        List<TiNetFileInfo> ReceiveFileInfos = new List<TiNetFileInfo>();

        TiNetFileInfo GetTiNetFileInfo(string FileName)
        {
            for (int i = 0; i < ReceiveFileInfos.Count; i++)
            {
                TiNetFileInfo fileInfo = ReceiveFileInfos[i];
                if (fileInfo.FileName.Equals(FileName))
                {
                    return fileInfo;
                }
            }
            return default(TiNetFileInfo);
        }

        bool m_IsNetworkStarted;

        bool restartNetworkingWhenAppresume = false;

        /// <summary>
        /// Is the TiNet ready?
        /// </summary>
        [InspectFunction]
        public bool IsNetworkStarted
        {
            get => externalTransportLayer != null ? externalTransportLayer.IsNetworkActive : m_IsNetworkStarted;
        }

        [SerializeField, DisableEditing]
        TiNetNode thisNode;

        /// <summary>
        /// Gets the current tinet node .
        /// If network is not started, the node is null.
        /// </summary>
        public I_TiNetNode ThisNode
        {
            get
            {
                return thisNode;
            }
        }

        /// <summary>
        /// 在尝试添加一个 tinet node 之前的筛选委托， 供上层应用使用。
        /// param 2 = 对方tinet node 的 custom tag.
        /// param 3 = 对方IP.
        /// return : 如果为false, 则tinet在收到对方的身份信息的时候， 不会添加此节点。
        /// </summary>
        public static event Func<string, IPAddress, bool> OnTryToAddNode;

        /// <summary>
        /// Event : On TiNet node is connected.
        /// </summary>
        public static event Action<I_TiNetNode> OnNodeConnected;

        /// <summary>
        /// Event : on TiNet node is disconnected.
        /// </summary>
        public static event Action<I_TiNetNode> OnNodeDisconnected;

        /// <summary>
        /// Event : on TiNet is start.
        /// </summary>
        public static event Action OnTiNetStart;

        /// <summary>
        /// Event : on TiNet is stop.
        /// </summary>
        public static event Action OnTiNetStop;

        /// <summary>
        /// Event : on text message.
        /// </summary>
        public static event Action<string, IPEndPoint> OnTextMessage;

        /// <summary>
        /// Event : on data message.
        /// Parameter 1 = data buffer.
        /// Parameter 2 = data length. Maximum 5KB
        /// </summary>
        public static event Action<byte[], int, IPEndPoint> OnDataMessage;

        /// <summary>
        /// Event : on query info message (ack) , 来自远端的回复
        /// </summary>
        internal static event Action OnQueryAckMessage;

        /// <summary>
        /// Event : on tinet receives a file from remote peer node.
        /// </summary>
        public static event Action<I_FileInfo> OnReceiveFile;

        /// <summary>
        /// Event : on a remote node is discovered by receiving its broadcasting info.
        ///
        /// Param 1 = remote node's ip;
        ///
        /// Param 2 = remote node's custom tag
        ///
        /// Param 3 = remote node's ID
        /// 
        /// </summary>
        public static event Action<IPEndPoint, string, int> OnNodeDiscovered;

        /// <summary>
        /// Event : on a remote node is discovered by receiving its broadcasting info.
        ///
        /// Param 1 = remote node's ip;
        ///
        /// Param 2 = remote node's custom tag
        ///
        /// Param 3 = remote node's ID
        /// 
        /// </summary>
        public static event Action<IPEndPoint, string, int> OnUnbandNodeDiscovered;

        /// <summary>
        /// Real start time of the TiNet instance.
        /// If the TiNet is not start, this value is null.
        /// </summary>
        public DateTime? NetworkStartTime
        {
            get; private set;
        }

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
            try
            {
                InitializeTiNet();
            }
            catch (System.Exception e)
            {
                Debug.LogError(kErrorMsg04);
                Debug.LogException(e);
                Debug.LogError("TiNet is disabled due to initialization error.");
            }
        }

        void InitializeTiNet()
        {
            SetupPoolList();
            SocketLLMgr.CreateInstance().DebugMode = this.DebugMode;
            //初始化  TiNet 唯一 ID:
            MachineName = SystemInfo.deviceUniqueIdentifier;
            TiNetIDNodeID = (int)this.MachineName.GetPersisentHash();

            TiNetworkMessagePool.Init();
            TiNetworkPacketPool.Init();

            List<Type> messageHandlerType = new List<Type>();
            PEReflectionUtility.SearchForChildrenTypes(typeof(TiNetMessageHandler), messageHandlerType);

            //初始化  message handlers:
            TiMessageHandlers01.Clear();
            TiMessageHandlers02.Clear();
            for (int i = 0, messageHandlerTypeCount = messageHandlerType.Count; i < messageHandlerTypeCount; i++)
            {
                var handlerType = messageHandlerType[i];
                MethodInfo[] methods = handlerType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                for (int j = 0, methodsLength = methods.Length; j < methodsLength; j++)
                {
                    var method = methods[j];
                    var TiNetMsgCallbackAttr = method.GetCustomAttribute<TiNetMessageCallbackAttribute>();
                    if (TiNetMsgCallbackAttr != null)
                    {
                        Type returnType = method.ReturnType;
                        var parameters = method.GetParameters();
                        short msgCode = TiNetMsgCallbackAttr.MessageCode;
                        if (returnType == typeof(void) && parameters.Length == 2 && typeof(TiNetMessage).IsAssignableFrom(parameters[0].ParameterType)
                            && parameters[1].ParameterType == typeof(IPEndPoint))
                        {
                            if (this.DebugMode)
                            {
                                Debug.LogFormat(kLogMsg02, handlerType.Name, method.Name, msgCode);
                            }
                            if (TiMessageHandlers01.ContainsKey(msgCode))
                            {
                                Debug.LogErrorFormat(kErrorMsg01, msgCode, TiMessageHandlers01[msgCode]);
                            }
                            else
                            {
                                Action<TiNetMessage, IPEndPoint> action = (Action<TiNetMessage, IPEndPoint>)method.CreateDelegate(typeof(Action<TiNetMessage, IPEndPoint>));
                                TiMessageHandlers01.Add(msgCode, action);
                            }
                        }
                        if (returnType == typeof(void) && parameters.Length == 2 && typeof(TiNetMessage).IsAssignableFrom(parameters[0].ParameterType)
                           && parameters[1].ParameterType == typeof(I_TiNetNode))
                        {
                            if (DebugMode)
                                Debug.LogFormat(kLogMsg05, handlerType.Name, method.Name, msgCode);
                            if (TiMessageHandlers02.ContainsKey(msgCode))
                            {
                                Debug.LogErrorFormat(kErrorMsg02, msgCode, TiMessageHandlers02[msgCode]);
                            }
                            else
                            {
                                Action<TiNetMessage, I_TiNetNode> action = (Action<TiNetMessage, I_TiNetNode>)method.CreateDelegate(typeof(Action<TiNetMessage, I_TiNetNode>));
                                TiMessageHandlers02.Add(msgCode, action);
                            }
                        }
                    }
                }
            }

            //Initialize transport layer:
            if (externalTransportLayer != null)
            {
                externalTransportLayer.Initialize(new TiNetNodeDescriptor()
                {
                    AutoJointNetwork = this.AutoJointNetwork,
                    BroadcastID = this.BroadcastDiscovery,
                    BroadcastPort = this.PortBroadcast,
                    ReliablePort = this.PortReliable,
                    UnreliablePort = this.PortUnreliable,
                    DetachNetworkOnAppPause = this.ShouldDetachNetworkOnAppPaused,
                    filterMode = this.FilterMode,
                    FilterKeyword = this.FilterWord,
                    NodeTag = this.CustomTag,
                    TiNetMachineName = this.MachineName,
                    TiNetNodeID = this.m_TiNetNodeID,
                });
            }
        }

        /// <summary>
        /// TCP server 接听到来自远端的连接的时候的回调。
        /// </summary>
        /// <param name="connection"></param>
        private void TcpServer_OnServerConnected(I_Connection connection)
        {
            OnTCPServerConnected_CheckIsTiNetNodeConnection(connection);//检查是否tinet node connection
        }

        /// <summary>
        /// On tcp server flush data packet list.
        /// </summary>
        /// <param name="packets"></param>
        private void TcpServer_OnFlushData(IEnumerable<NetworkDataPacket> packets)
        {
            HandleIncomingMessages(packets);
        }

        /// <summary>
        /// 处理来自远端的消息。
        /// </summary>
        /// <param name="packets"></param>
        private void HandleIncomingMessages(IEnumerable<NetworkDataPacket> packets)
        {
            foreach (NetworkDataPacket packet in packets)
            {
                try
                {
                    short msgCode = PEUtils.ReadShort(packet.Data);
                    TiNetMessage msg = TiNetworkMessagePool.GetMessage(msgCode);
                    if (this.DebugMode)
                    {
                        Debug.LogFormat("On Tcp server packet of code : {0} at type: {1} from : {2}", msgCode, msg.GetType().Name, packet.endPoint.ToString());
                    }
                    msg.Deserialize(packet.Data, 2);//starts from index 2 (first 2 = msg code)

                    //Preserved message:
                    if (msgCode == 1)
                    {
                        TiNetDiscovery.OnRemoteIDMessage(msg, packet.endPoint);
                    }
                    //Preserved msg : text msg
                    else if (msgCode == 2)
                    {
                        TiNetMessageReceiver.OnSimpleTextMessage(msg, packet.endPoint);
                    }
                    //Preserved msg : data msg
                    else if (msgCode == 3)
                    {
                        TiNetMessageReceiver.OnDataHandler(msg, packet.endPoint);
                    }
                    //Preserved msg : query info msg
                    else if (msgCode == 4)
                    {
                        TiNetMessageReceiver.OnQueryMessageHandler(msg, packet.endPoint);
                    }
                    //Preserved msg : send file msg
                    else if (msgCode == 5)
                    {
                        TiNetMessageReceiver.OnSendFileMessage(msg, packet.endPoint);
                    }
                    else if (msgCode == 7)
                    {
                        TiNetMessageReceiver.OnBandIPMessageHandler(msg, packet.endPoint);
                    }
                    //Custom message:
                    else
                    {
                        bool handlerFound = false;
                        if (TiMessageHandlers01.TryGetValue(msgCode, out Action<TiNetMessage, IPEndPoint> action) && action != null)
                        {
                            action?.Invoke(msg, packet.endPoint);
                            handlerFound = true;
                        }

                        if (TiMessageHandlers02.TryGetValue(msgCode, out Action<TiNetMessage, I_TiNetNode> _action2) && _action2 != null)
                        {
                            if (GetTiNetNode(packet.endPoint.Address, out I_TiNetNode _node) && _node != null)
                            {
                                _action2?.Invoke(msg, _node);
                                handlerFound = true;
                            }
                            else
                            {
                                _node = new _InternalTiNetNode()
                                {
                                    Address = packet.endPoint.Address,
                                };
                                _action2?.Invoke(msg, _node);
                                handlerFound = true;
                            }
                        }

                        if (!handlerFound)
                        {
                            Debug.LogWarningFormat(kWarningMsg01, msgCode);
                            Debug.LogWarningFormat("Message exists: {0}, {1}", TiMessageHandlers01.ContainsKey(msgCode), TiMessageHandlers02.ContainsKey(msgCode));
                        }
                    }
                    //回收消息.
                    TiNetworkMessagePool.ReturnMessage(msgCode, msg);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        /// <summary>
        /// Tcp server disconnect callback.
        /// </summary>
        /// <param name="connection"></param>
        private void TcpServer_OnServerDisconnect(I_Connection connection)
        {
            Debug.LogFormat(kLogMsg03, connection.ipEndpoint.ToString());
            for (int i = m_nodes.Count - 1; i >= 0; i--)
            {
                TiNetNode node = this.m_nodes[i];
                if (node.address.GetHashCode() == connection.ipEndpoint.Address.GetHashCode())
                {
                    //先 remove list element:
                    node.state = TiNetNodeState.NotConnected;
                }
            }
        }

        private void OnDestroy()
        {
            if (IsNetworkStarted)
            {
                StopNetworking();
            }
        }

        private void Start()
        {
            if (AutoStart)
            {
                StartNetworking();
            }
        }

        private void Update()
        {
            var cachedPackets = this.PacketListCache;
            cachedPackets.Clear();
            if (broadcastListener != null)
            {
                if (/*broadcastListener.IsBound && */broadcastListener.PopReceivePacket(cachedPackets) > 0)
                {
                    for (int i = 0, packetCount = cachedPackets.Count; i < packetCount; i++)
                    {
                        var packet = cachedPackets[i];
                        //Debug.LogFormat("Recv packet : {0} from address : {1}", packet.DataLength, packet.endPoint.ToString());
                        OnUDPPacket(packet);
                    }
                }
            }


            cachedPackets.Clear();

            if (udpListener != null)
            {
                if (this.udpListener.IsBound && udpListener.PopReceivePacket(cachedPackets) > 0)
                {
                    for (int i = 0, packetCount = cachedPackets.Count; i < packetCount; i++)
                    {
                        var packet = cachedPackets[i];
                        if (DebugMode)
                        {
                            Debug.LogFormat(kLogMsg06, packet.Data.Length, packet.endPoint);
                        }
                        OnUDPPacket(packet);
                    }
                }
            }


            //处理心跳包:
            if (m_nodes.Count > 0)
            {
                for (int i = 0; i < m_nodes.Count; i++)
                {
                    TiNetNode n = this.m_nodes[i];
                    if (n.state == TiNetNodeState.Connected)
                    {
                        //每 1000 毫秒发送一个心跳包, 如果节点在这1000毫秒间隔内有过通信， 则不发送心跳。
                        if (!n.previousSentMessageTime.HasValue || (DateTime.Now - n.previousSentMessageTime.Value).TotalMilliseconds >= 1000)
                        {
                            PingNode(n);//pings the node
                        }
                    }
                }
            }

            //Debug.LogFormat("[TiNet] Pool packet count: {0}", TiNetworkPacketPool.packets.Count);
        }

        /// <summary>
        /// Sends a heart beat message to target node.
        /// </summary>
        /// <param name="node"></param>
        public void PingNode(I_TiNetNode node)
        {
            if (node.State != TiNetNodeState.Connected || !(node is TiNetNode))
            {
                return;//node is not ready.
            }
            var n = node as TiNetNode;
            byte[] _data = TcpSocketWrapper.kDummyMessage;
            //添加进 Tcp Socket wrapper 队列:
            TcpSocketWrapper.sPushOutgoingMessage(n.tcpSocket, _data, 0, _data.Length, false,
                callback: (userData, socketErr, sent) =>
                {
                    if (this.DebugMode)
                    {
                        Debug.LogFormat("Heart beat message result: {0}", socketErr);
                    }
                    if (socketErr != System.Net.Sockets.SocketError.Success)
                    {
                        this.OnSentError(userData as TiNetNode, socketErr, sent);
                    }
                }, UserData: n
                );
            n.previousSentMessageTime = DateTime.Now;//记录上一次发送时间。
        }

        /// <summary>
        /// Stop networking when app is paused.
        /// restart networking when app is startup.
        /// </summary>
        /// <param name="pause"></param>
        void OnApplicationPause(bool pause)
        {
            Debug.LogFormat("[TiNet] Application Pause: {0}", pause);
            if (!ShouldDetachNetworkOnAppPaused)
            {
                return;
            }
            //pause:
            if (pause)
            {
                if (this.m_IsNetworkStarted)
                {
                    restartNetworkingWhenAppresume = true;
                    this.StopNetworking();
                }
            }
            //resume:
            else
            {
                if (restartNetworkingWhenAppresume)
                {
                    this.StartNetworking();
                }
            }
        }


        private void OnNetworkStart()
        {
            this.NetworkStartTime = DateTime.Now;
            //Setup this tinet node:
            TiNetNode _thisNode = new TiNetNode();
            PENetworkUtils.GetLocalIP(out _thisNode.address);
            _thisNode.nodeName = this.MachineName;
            _thisNode.isLocalNode = true;
            _thisNode.customTag = this.CustomTag;
            _thisNode.nodeID = this.TiNetIDNodeID;
            _thisNode.reliablePort = this.PortReliable;
            _thisNode.unreliablePort = this.PortUnreliable;
            _thisNode.nodeStartTime = NetworkStartTime.Value.Ticks;
            _thisNode.tcpClientComponent = null;//this node 没有 tcp client
            _thisNode.UserData = null;
            _thisNode.EndPoint_Reliable = null;
            _thisNode.EndPoint_Unreliable = null;
            _thisNode.FilterKeyword = this.FilterWord;
            this.thisNode = _thisNode;
        }

        /// <summary>
        /// Starts TiNet networking .
        /// </summary>
        [InspectFunction]
#pragma warning disable RECS0165 // 异步方法应返回 Task，而不应返回 void
        public async void StartNetworking()
#pragma warning restore RECS0165 // 异步方法应返回 Task，而不应返回 void
        {


            if (externalTransportLayer != null)
            {
                if (externalTransportLayer.StartNetwork())
                {
                    m_IsNetworkStarted = true;
                    OnNetworkStart();
                }
                return;
            }

            //初始化 TCP/UDP Socket:
            if (broadcastListener == null)
                broadcastListener = new UdpSocketListenerWrapper(gameObject);

            if (udpSender == null)
                udpSender = new UdpSocketSenderWrapper(gameObject, 1024 * 512);//socket's send buffer size : 512KB

            if (broadcastSender == null)
                broadcastSender = new UdpSocketSenderWrapper(gameObject);

            if (udpListener == null)
                udpListener = new UdpSocketListenerWrapper(gameObject);

            cancellation = new CancellationTokenSource();

            tcpServer = this.gameObject.GetOrAddComponent<TCPServer>();
            tcpServer.HeartBeat = false;//在tinet的update中，统一处理针对每个节点的心跳包

            tcpServer.OnServerConnected -= TcpServer_OnServerConnected;
            tcpServer.OnServerDisconnect -= TcpServer_OnServerDisconnect;
            tcpServer.OnFlushData -= TcpServer_OnFlushData;

            tcpServer.OnServerConnected += TcpServer_OnServerConnected;
            tcpServer.OnServerDisconnect += TcpServer_OnServerDisconnect;
            tcpServer.OnFlushData += TcpServer_OnFlushData;

            try
            {
                Debug.LogFormat(kLogMsg01, this.MachineName);
                //启动 broadcaster 监听器:
                broadcastListener.BindToBroadcastAddress(PortBroadcast);
                //启动 UDP 监听器:
                udpListener.BindToBroadcastAddress(PortUnreliable);
                //启动 tcp server:
                tcpServer.ServerPort = this.PortReliable;


                bool serverStarted = tcpServer.StartServer();
                if (!serverStarted)
                {
                    //如果TCPserver 失败:
                    await new WaitForGameTime(1);//等待1s，再次启动
                    serverStarted = tcpServer.StartServer();
                    //如果连续两次失败:
                    if (!serverStarted)
                    {
                        Debug.LogError(kErrorMsg06);
                    }
                }


                if (udpSender == null)
                {
                    udpSender = new UdpSocketSenderWrapper(gameObject, 1024 * 512);
                }
                OnNetworkStart();


                //启动 Discovery Coroutine:
                StartCoroutine(BroadcastDiscoveryMessage());



                m_IsNetworkStarted = true;

                //mark network start time (real system time)

                OnTiNetStart?.Invoke();

                Debug.LogFormat(kLogMsg04, this.MachineName);
            }
            catch (System.Exception exc)
            {
                Debug.LogErrorFormat("[TiNet] exception on start networking: {0}", exc.Message);
                Debug.LogError(kErrorMsg05, this.gameObject);
                Debug.LogException(exc);
            }
        }

        /// <summary>
        /// Stops TiNet netwokring .
        /// 停止tinet的网络， 此方法会暂停TCP server，切断所有的tinet node 的连接。
        /// 但不会清除 node 缓存数据。
        /// </summary>
        [InspectFunction]
        public void StopNetworking()
        {
            if (externalTransportLayer != null)
            {
                externalTransportLayer.StopNetwork();

                //Mark node state : disconnected 
                for (int i = 0; i < m_nodes.Count; i++)
                {
                    TiNetNode n = m_nodes[i];
                    n.state = TiNetNodeState.NotConnected;
                }

                TiNet.OnTiNetStop?.Invoke();

                return;
            }
            try
            {
                //停止 broadcaster 监听器:
                broadcastListener.Unbind();

                //停止 UDP 监听器:
                udpListener.Unbind();

                this.udpSender.Dispose();
                udpSender = null;


                //停止 Discovery Coroutine:
                StopAllCoroutines();

                m_IsNetworkStarted = false;

                NetworkStartTime = null;


                foreach (var n in m_nodes)
                {
                    if (n.state == TiNetNodeState.Connected && n.tcpSocket != null)
                    {
                        n.tcpSocket.Disconnect(false);
                    }
                    if (n.tcpClientComponent)
                    {
                        n.tcpClientComponent.gameObject.Destroy();//销毁 TCP client 组件
                    }

                    n.state = TiNetNodeState.NotConnected;
                }

                //停止 tcp server:
                tcpServer.StopServer();

                tcpServer.OnServerConnected -= TcpServer_OnServerConnected;
                tcpServer.OnServerDisconnect -= TcpServer_OnServerDisconnect;
                tcpServer.OnFlushData -= TcpServer_OnFlushData;
                this.tcpServer.Destroy();//销毁 tcp server

                ///Event : on TiNet stop
                OnTiNetStop?.Invoke();

                //在调用了事件之后， 再清空本地引用。
                thisNode = null;//clear this node

                Debug.LogFormat("[TiNet] stops.");
            }
            catch (Exception exc)
            {
                Debug.LogErrorFormat("[TiNet] exception on stop networking: {0}", exc.Message);
                Debug.LogException(exc);
            }
        }


        /// <summary>
        /// Unband Node.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="isSendMsg"></param>
        public void Unband(IPAddress address, bool isSendMsg = true)
        {
            TiNetNode node = m_nodes.Find(n => n.address.Equals(address));
            if (node != null)
            {
                if (isSendMsg) SendBandIPMessage(node, false);
                unbandNodes.Add(node);
                m_nodes.Remove(node);
                node.tcpClientComponent.StopClient();
            }
        }

        /// <summary>
        /// Band Node.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="isSendMsg"></param>
        public void Band(IPAddress address, bool isSendMsg = true)
        {
            TiNetNode node = unbandNodes.Find(n => n.address.Equals(address));
            if (node != null)
            {
                if (isSendMsg) SendBandIPMessage(node, true);
                m_nodes.Add(node);
                unbandNodes.Remove(node);
                node.tcpClientComponent.StartClient();
            }
        }

        /// <summary>
        /// Send band ip message.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="isBand"></param>
        private void SendBandIPMessage(TiNetNode node, bool isBand)
        {
            BandIPMessage msg = TiNetworkMessagePool.GetMessage<BandIPMessage>();
            msg.isBand = isBand;
            Broadcast(msg);
        }

        /// <summary>
        /// Callback : On UDP packet.
        /// </summary>
        /// <param name="DataPacket"></param>
        private void OnUDPPacket(TiNetworkPacket DataPacket)
        {
            short msgCode = PEUtils.ReadShort(DataPacket.Data, 0);
            TiNetMessage msg = TiNetworkMessagePool.GetMessage(msgCode);
            try
            {
                msg.Deserialize(DataPacket.Data, 2);
                if (DebugMode)
                {
                    Debug.LogFormat("Get mapped msg type: {0} of code: {1} from :{2}", msg.GetType(), msgCode, DataPacket.endPoint);
                }

                //Preserved msg : discovery msg
                if (msgCode == 1)
                {
                    TiNetDiscovery.OnRemoteIDMessage(msg, DataPacket.endPoint);//internal msg : discovery
                }
                //Preserved msg : text msg
                else if (msgCode == 2)
                {
                    TiNetMessageReceiver.OnSimpleTextMessage(msg, DataPacket.endPoint);
                }
                //Preserved msg : data msg
                else if (msgCode == 3)
                {
                    TiNetMessageReceiver.OnDataHandler(msg, DataPacket.endPoint);
                }
                //Preserved msg : query info msg
                else if (msgCode == 4)
                {
                    TiNetMessageReceiver.OnQueryMessageHandler(msg, DataPacket.endPoint);
                }
                else if (msgCode == 7)
                {
                    TiNetMessageReceiver.OnBandIPMessageHandler(msg, DataPacket.endPoint);
                }
                else
                {
                    if (TiMessageHandlers01.TryGetValue(msgCode, out Action<TiNetMessage, IPEndPoint> action) && action != null)
                    {
                        action(msg, DataPacket.endPoint);
                    }
                    else if (TiMessageHandlers02.TryGetValue(msgCode, out Action<TiNetMessage, I_TiNetNode> action2) && action2 != null)
                    {
                        if (GetTiNetNode(DataPacket.endPoint.Address, out I_TiNetNode node))
                        {
                            action2(msg, node);
                        }
                    }
                    else
                    {
                        if (DebugMode)
                            Debug.LogErrorFormat(kErrorMsg03, msgCode);
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.LogException(exc);
            }
            finally
            {
                //TiNetworkMessage 回池:
                TiNetworkMessagePool.ReturnMessage(msgCode, msg);
                DataPacket.Return();
            }
        }

        /// <summary>
        /// 尝试与一个 Address : UDP port 的主机通讯， 并建立连接。
        /// </summary>
        /// <param name="IPAddres"></param>
        /// <param name="DiscoveryPort">对方Tinet的网络发现节点。</param>
        /// <param name="wantsMutualConnection">如果为true, 则建立相互连接。</param>
        /// <returns></returns>
        public void TryConnectsTo(string IPAddres, int DiscoveryPort, bool wantsMutualConnection)
        {
            var _address = IPAddress.Parse(IPAddres);
            QueryNodeInfoMessage queryMsg = TiNetworkMessagePool.GetMessage<QueryNodeInfoMessage>();
            queryMsg.messageFlag = QueryNodeInfoMessage.MessageFlag.Ask;
            queryMsg.UnreliablePort = this.PortUnreliable;
            queryMsg.ReliablePort = this.PortReliable;
            queryMsg.Address = this.tcpServer.ServerAddress;
            queryMsg.NodeID = this.TiNetIDNodeID;
            queryMsg.NodeName = this.TiNetIDNodeName;
            queryMsg.CustomTag = this.CustomTag;
            queryMsg.NodeStartTime = this.NetworkStartTime.HasValue ? NetworkStartTime.Value.Ticks : 0;
            queryMsg.mutualConnection = wantsMutualConnection;

            byte[] buffer = new byte[128];
            PEUtils.WriteShort(0x0004, buffer);
            queryMsg.TotalPassBufferLength = 2;
            int bufferSize = queryMsg.Serialize(buffer, 2);
            this.udpSender.Send(new IPEndPoint(_address, DiscoveryPort), buffer, 0, bufferSize);

            //等待回复:
            Debug.Log("LANDiscoveryMessage is sent");

        }






        /// <summary>
        /// 广播局域网发现信息。
        /// </summary>
        /// <returns></returns>
        IEnumerator BroadcastDiscoveryMessage()
        {
            var wait1s = new WaitForSeconds(1f);
            while (true)
            {

                if (m_BroadcastDiscovery)
                {
                    Ximmerse.XR.UnityNetworking.InternalMessages.LANDiscoveryMessage msg = TiNetworkMessagePool.GetMessage<Ximmerse.XR.UnityNetworking.InternalMessages.LANDiscoveryMessage>();
                    msg.NodeName = MachineName;
                    msg.NodeID = this.TiNetIDNodeID;
                    msg.ReliablePort = this.PortReliable;
                    msg.UnreliablePort = this.PortUnreliable;
                    msg.MyRole = 0;
                    msg.CustomTag = this.CustomTag;
                    msg.NodeStartTime = this.NetworkStartTime.Value.Ticks;
                    if (this.FilterMode == ConnectionFilterMode.UseFilterWord)
                    {
                        msg.Keyword = this.FilterWord;
                    }
                    else
                    {
                        msg.Keyword = string.Empty;
                    }
                    //Send(msg, null, this.PortBroadcast);
                    Broadcast(msg);
                }
                yield return wait1s;
            }
        }

        /// <summary>
        /// Gets TiNet node.
        /// </summary>
        /// <param name="NodeName"></param>
        /// <param name="Node"></param>
        /// <returns></returns>
        public bool GetTiNetNode(string NodeName, out I_TiNetNode Node)
        {
            if (nodeMap.TryGetValue(NodeName, out TiNetNode _Node))
            {
                Node = _Node;
                return true;
            }
            else
            {
                Node = default(I_TiNetNode);
                return false;
            }
        }

        /// <summary>
        /// Gets TiNet node.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="Node"></param>
        /// <returns></returns>
        public bool GetTiNetNode(IPAddress address, out I_TiNetNode Node)
        {
            for (int i = 0, m_nodesCount = m_nodes.Count; i < m_nodesCount; i++)
            {
                var node = m_nodes[i];
                if (node.address.GetHashCode() == address.GetHashCode())
                {
                    Node = node;
                    return true;
                }
            }
            Node = null;
            return false;
        }

        /// <summary>
        /// Gets all net nodes.
        /// </summary>
        /// <param name="Nodes"></param>
        /// <returns></returns>
        public int GetAllNodes(List<I_TiNetNode> Nodes)
        {
            Nodes.Clear();
            for (int i = 0, maxm_nodesCount = this.m_nodes.Count; i < maxm_nodesCount; i++)
            {
                var node = this.m_nodes[i];
                Nodes.Add(node);
            }
            return Nodes.Count;
        }

        /// <summary>
        /// Gets node count.
        /// </summary>
        [InspectFunction]
        public int NodeCount
        {
            get
            {
                return Instance.m_nodes.Count;
            }
        }


        class TiNetMessageReceiver //: TiNetMessageHandler //internal msg code using hard reference link
        {
            /// <summary>
            /// Handler : on simple text message
            /// </summary>
            /// <param name="message"></param>
            /// <param name="endPoint"></param>
            [TiNetMessageCallbackAttribute(messageCode: 0x0002)]
            public static void OnSimpleTextMessage(TiNetMessage message, IPEndPoint endPoint)
            {
                if (endPoint != null && GetUnbandNode(endPoint.Address) != null)
                {
                    return;
                }
                SimpleTextMessage simpleTextMsg = (SimpleTextMessage)message;
                try
                {
                    OnTextMessage?.Invoke(simpleTextMsg.Text, endPoint);
                    if (TiNet.instance.DebugMode)
                    {
                        Debug.LogFormat("On text message: {0} from : {1}", simpleTextMsg.Text, endPoint != null ? endPoint.ToString() : "");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            [TiNetMessageCallbackAttribute(messageCode: 0x0003)]
            public static void OnDataHandler(TiNetMessage message, IPEndPoint endPoint)
            {
                if (endPoint != null && GetUnbandNode(endPoint.Address) != null)
                {
                    return;
                }
                SimpleBytesMessage simpleBytesMsg = (SimpleBytesMessage)message;
                try
                {
                    OnDataMessage?.Invoke(simpleBytesMsg.buffer, simpleBytesMsg.length, endPoint);
                    if (TiNet.instance.DebugMode)
                    {
                        Debug.LogFormat("On data message , byte count: {0} from : {1}", simpleBytesMsg.length, endPoint != null ? endPoint.ToString() : "");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            [TiNetMessageCallbackAttribute(messageCode: 0x0004)]
            public static void OnQueryMessageHandler(TiNetMessage message, IPEndPoint endPoint)
            {
                Debug.LogWarning("QueryMessage is deprecated, don't depends on it");
                //                if (GetUnbandNode(endPoint.Address) != null)
                //                {
                //                    return;
                //                }
                //                QueryNodeInfoMessage queryInfoMsg = (QueryNodeInfoMessage)message;
                //                try
                //                {
                //                    //对 ask message, 回复一条ack message:
                //                    if (queryInfoMsg.messageFlag == QueryNodeInfoMessage.MessageFlag.Ask)
                //                    {
                //                        QueryNodeInfoMessage ack = TiNetUtility.GetMessage<QueryNodeInfoMessage>();
                //                        ack.messageFlag = QueryNodeInfoMessage.MessageFlag.Ack;
                //                        ack.ReliablePort = instance.PortReliable;
                //                        ack.UnreliablePort = instance.PortUnreliable;
                //                        ack.NodeName = instance.TiNetIDNodeName;
                //                        ack.NodeID = instance.TiNetIDNodeID;
                //                        ack.Address = instance.tcpServer.ServerAddress;
                //                        ack.CustomTag = instance.CustomTag;
                //                        ack.NodeStartTime = instance.NetworkStartTime.Value.Ticks;

                //                        var packet = TiNetworkPacketPool.GetNetworkPacket();
                //                        byte[] buffer = new byte[256];
                //                        PEUtils.WriteShort(0x0004, buffer, 0);//write: message code
                //                        int messageBufferLength = ack.Serialize(buffer, 2);
                //                        IPEndPoint iPEndPoint_udp = new IPEndPoint(endPoint.Address, queryInfoMsg.UnreliablePort);
                //                        instance.udpSender.Send(iPEndPoint_udp, buffer, 0, 2 + messageBufferLength);
                //                        Debug.LogFormat("Return a query ack message to:{0}:{1}", iPEndPoint_udp.Address, iPEndPoint_udp.Port);
                //                        if (queryInfoMsg.mutualConnection)
                //                        {
                //                            Instance.TryConnectsTo(endPoint.Address.ToString(), Instance.PortBroadcast, false);
                //                        }
                //                        buffer = null;
                //                    }
                //                    else //ack, 收到来自远端的回复
                //                    {
                //                        Debug.LogFormat("Receives query ack : {0}", queryInfoMsg.ToString());
                //                        OnQueryAckMessage?.Invoke();

                //                        TiNetNode node = new TiNetNode();
                //                        node.nodeName = queryInfoMsg.NodeName;
                //                        node.nodeID = queryInfoMsg.NodeID;
                //                        node.isLocalNode = false;
                //                        node.address = endPoint.Address;
                //                        node.reliablePort = queryInfoMsg.ReliablePort;
                //                        node.unreliablePort = queryInfoMsg.UnreliablePort;
                //                        node.CustomTag = queryInfoMsg.CustomTag;
                //                        node.nodeStartTime = queryInfoMsg.NodeStartTime;

                //#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                //                        Instance.TryAddNode(node);
                //#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                //                        //node.address = message.aadd
                //                        //Instance.nodes.Add(node);
                //                    }
                //                }
                //                catch (Exception e)
                //                {
                //                    Debug.LogException(e);
                //                }
            }

            [TiNetMessageCallbackAttribute(messageCode: 0x0005)]
            public static void OnSendFileMessage_Handler2(TiNetMessage message, I_TiNetNode node)
            {
                OnSendFileMessage(message, node.EndPoint_Reliable);
            }

            /// <summary>
            /// 收到了来自远端的文件传输信息。
            /// </summary>
            /// <param name="message"></param>
            /// <param name="endPoint"></param>
            [TiNetMessageCallbackAttribute(messageCode: 0x0005)]
            public static void OnSendFileMessage(TiNetMessage message, IPEndPoint endPoint)
            {
                if (endPoint != null && GetUnbandNode(endPoint.Address) != null)
                {
                    return;
                }
                TransferDataMessage transferDataMsg = (TransferDataMessage)message;
                try
                {
                    var fileName = transferDataMsg.name;
                    var packetIndex = transferDataMsg.packetIndex;
                    var totalPacketCount = transferDataMsg.packetTotalCount;
                    var dataBuffer = transferDataMsg.buffer;
                    var dataBufferLength = transferDataMsg.length;
                    //将 transferDataMsg 中的数据,copy到文件信息中:
                    TiNetFileInfo fileInfo = instance.GetTiNetFileInfo(fileName);
                    //如果是第一个packet, 创建一个 file info 对象:
                    if (fileInfo == null)
                    {
                        fileInfo = new TiNetFileInfo();
                        fileInfo.FileName = fileName;
                        instance.ReceiveFileInfos.Add(fileInfo);
                    }
                    //填充文件信息对象:
                    //   fileInfo.Content.AddRange(dataBuffer, 0, dataBufferLength);
                    fileInfo.AddFilePacket(packetIndex, dataBuffer);
                    if (Instance.DebugMode)
                    {
                        Debug.LogFormat("Receive file packet: {0}/{1}, packet length: {2}, total receive bytes: {3}, packet count:{4}", packetIndex, totalPacketCount, dataBufferLength, fileInfo.TotalBytes, fileInfo.FilePackets.Count);
                    }
                    if (fileInfo.FilePackets.Count == totalPacketCount)//最后一个数据包了
                    {
                        fileInfo.DoneTransport();
                        if(Instance.DebugMode)
                           Debug.LogFormat("收到文件 : {0} / {1}bytes", fileInfo.fileName, fileInfo.Content.Count);
                        fileInfo.IsReady = true;
                        //#if UNITY_EDITOR
                        //                        //在editor下将文件保存到 Assets/../ReceivedFiles 路径。
                        //                        string editor_write_path = System.IO.Path.Combine(
                        //                                System.IO.Directory.GetParent(Application.dataPath).FullName, fileInfo.FileName);
                        //                        System.IO.File.WriteAllBytes(
                        //                            editor_write_path,
                        //                            fileInfo.Content.ToArray());
                        //                        Debug.LogFormat("Write file : {0}", editor_write_path);
                        //#endif
                        //发出事件供上层应用调用，保存文件:
                        OnReceiveFile?.Invoke(fileInfo);
                        //发出消息后，删除文件对象。
                        instance.ReceiveFileInfos.Remove(fileInfo);
                        //释放文件对象:
                        ((IDisposable)fileInfo).Dispose();
                    }
                    //else if (instance.DebugMode)
                    //{
                    //    Debug.LogFormat("收到文件 : {0}, {1}/{2}个数据包, 文件长度: {3}", fileInfo.fileName, transferDataMsg.packetIndex, transferDataMsg.packetTotalCount, fileInfo.Content.Count);
                    //}
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            [TiNetMessageCallbackAttribute(messageCode: 0x0007)]
            public static void OnBandIPMessageHandler(TiNetMessage message, IPEndPoint endPoint)
            {
                BandIPMessage bandIPMessage = (BandIPMessage)message;
                try
                {
                    if (bandIPMessage.isBand)
                    {
                        TiNet.instance.Band(endPoint.Address, false);
                    }
                    else
                    {
                        TiNet.instance.Unband(endPoint.Address, false);
                    }
                    if (TiNet.instance.DebugMode)
                    {
                        Debug.LogFormat("isBand : {1} from : {1}", bandIPMessage.isBand, endPoint.ToString());
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        /// <summary>
        /// 获取UnbandNode
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private static TiNetNode GetUnbandNode(IPAddress address)
        {
            TiNetNode node = TiNet.instance.unbandNodes.Find(n => n.address.Equals(address));
            return node;
        }
    }
}
