using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Net;
using System.Reflection;
using System.Linq;
using Ximmerse.XR.Asyncoroutine;


namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// TiNet : discovery and setup connection between each node.
    /// </summary>
    public partial class TiNet : MonoBehaviour
    {

        /// <summary>
        /// 延迟注册连接列表。
        /// 见 OnTCPServerConnected_CheckIsTiNetNodeConnection
        /// </summary>
        List<I_Connection> PendingRegistrationConnections = new List<I_Connection>();

        /// <summary>
        /// 和 remote node 建立连接
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        internal async Task<bool> ConnectsToNode(TiNetNode node)
        {
            //连接规则:
            //由网络中启动时间较早的设备，建立一个tcp client，连接到另一端(启动时间较迟的一端)的tcp server
            //这个规则要求网络中的所有的设备都处于统一时间轴下
            if (node.nodeStartTime > this.NetworkStartTime.Value.Ticks)//本地的时间早于远端，本地client连接到远端的server:
            {
                Debug.LogFormat("本地client连接到远端的server");
                //在子对象中， 建立或者获取连接到 远端 的tcp client 组件:
                Transform childTcpClient = gameObject.transform.Find(node.NodeName);
                TCPClient clientComponent = (childTcpClient && childTcpClient.HasComponent<TCPClient>()) ? childTcpClient.GetComponent<TCPClient>() : gameObject.AddChildGameObject<TCPClient>(node.NodeName);
                node.tcpClientComponent = clientComponent;
                //给TCP client注册消息监听器，处理来自远端tinet node的消息:
                clientComponent.OnFlushData -= onTcpClientFlushRemoteFlushData;
                clientComponent.OnFlushData += onTcpClientFlushRemoteFlushData;

                clientComponent.ServerAddress = node.Address.ToString();
                clientComponent.ServerPort = node.reliablePort;
                clientComponent.AutoReconnect = true;//自动重连
                clientComponent.HeartBeat = false;//在tinet的update中，统一处理针对每个节点的心跳包
                var connTask = clientComponent.StartClient();
                await connTask;//等待Tcp client和对方建立连接.
                switch (connTask.Result)
                {
                    case true:
                        node.tcpSocket = clientComponent.TcpClient.Client;
                        node.state = TiNetNodeState.Connected;//已经连接.
                        OnNodeConnected?.Invoke(node);
                        //在和远端的tcp server建立连接之后，发送一条 
                        break;

                    case false:
                        node.state = TiNetNodeState.NotConnected;//未连接，等待下次通过 TiNetDiscovery.OnRemoteIDMessage 方法进入连接池
                        break;
                }
                return connTask.Result;
            }
            else //本地时间迟于远端, 则不在此处理. 在TCP server接到新的连接的时候处理。见 : OnTCPServerConnected_CheckIsTiNetNodeConnection(I_Connection)
            {
                return true;
            }
        }

        /// <summary>
        /// tcp client收到来自远端的节点的数据。
        /// </summary>
        /// <param name="obj"></param>
        private void onTcpClientFlushRemoteFlushData(IEnumerable<NetworkDataPacket> packets)
        {
            HandleIncomingMessages(packets);
        }

        /// <summary>
        /// Tcp server 接到新的连接。
        /// 检查此连接是否来自于已知的 tinet node
        /// </summary>
        /// <param name="connection"></param>
        private void OnTCPServerConnected_CheckIsTiNetNodeConnection(I_Connection connection)
        {
            bool nodeExists = false;
            foreach (var node in m_nodes)
            {
                if (node.Address.ToString() == connection.ipEndpoint.Address.ToString())
                {
                    node.tcpSocket = connection.socket;
                    node.state = TiNetNodeState.Connected;//当一个远端的已经node和本身建立了tcp 连接的时候，标记此节点的状态为 connected
                    nodeExists = true;
                    Debug.LogFormat("建立和远端的Tcp client的连接。 tcp socket is not null : {0}", node.tcpSocket != null);
                    OnNodeConnected?.Invoke(node);
                    return;
                }
            }
            //如果来自远端的节点还未被添加到本地的 node map 和 node list，
            //这是因为id message广播时间交错导致的. 当己方的tcp server被远方的tcp client连接的时候，己方还未收到来自远方的ID广播消息，就会出现这种情况:
            if (nodeExists == false)
            {
                PendingRegistrationConnections.Add(connection);//将这个连接添加到延迟注册连接列表。
            }
        }

        /// <summary>
        /// 检查来自远端的LAN discovery message是否符合连接标准.
        /// </summary>
        /// <param name="remoteDiscoveryMessage"></param>
        /// <returns></returns>
        private bool CheckPassFilterWork(Ximmerse.XR.UnityNetworking.InternalMessages.LANDiscoveryMessage remoteDiscoveryMessage)
        {
            return this.FilterMode == ConnectionFilterMode.NoFilter || (this.FilterMode == ConnectionFilterMode.UseFilterWord
                 && this.FilterWord.Equals(remoteDiscoveryMessage.Keyword, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 检查ID信息是否来自本节点自身
        /// </summary>
        /// <param name="remoteDiscoveryMessage"></param>
        /// <returns></returns>
        private bool CheckIsSelf(Ximmerse.XR.UnityNetworking.InternalMessages.LANDiscoveryMessage remoteDiscoveryMessage)
        {
            return this.TiNetIDNodeID.Equals(remoteDiscoveryMessage.NodeID);
        }

        /// <summary>
        /// 此ip是否来自延迟注册连接。
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        private bool CheckIsPendingRegistrationConnection(IPEndPoint endPoint, out I_Connection conn)
        {
            for (int i = 0; i < PendingRegistrationConnections.Count; i++)
            {
                I_Connection iConn = PendingRegistrationConnections[i];
                if (iConn.ipEndpoint.Address.Equals(endPoint.Address))
                {
                    conn = iConn;
                    return true;
                }
            }
            conn = null;
            return false;
        }

        /// <summary>
        /// 处理网络发现和节点互联的inner class.
        /// </summary>
        class TiNetDiscovery
        {
            /// <summary>
            /// 收到来自远端的 LAN discovery message.
            /// </summary>
            /// <param name="message"></param>
            /// <param name="endPoint"></param>
            [TiNetMessageCallbackAttribute(messageCode: 0x0001)]
            public static async void OnRemoteIDMessage(TiNetMessage message, IPEndPoint endPoint)
            {
                var tinet = Instance;
                if (!tinet.CommonValidationOnNodeSetup())
                {
                    return;
                }
                Ximmerse.XR.UnityNetworking.InternalMessages.LANDiscoveryMessage remoteIDMsg = (Ximmerse.XR.UnityNetworking.InternalMessages.LANDiscoveryMessage)message;
                if (tinet.CheckIsSelf(remoteIDMsg))//如果消息体来自于本节点自身.
                {
                    return;
                }
                //Debug.Log("On LAN Discovery msg !");
                //如果此节点已经被Ban了:
                if (GetUnbandNode(endPoint.Address) != null)
                {
                    OnUnbandNodeDiscovered?.Invoke(endPoint, remoteIDMsg.CustomTag, remoteIDMsg.NodeID);
                    return;
                }

                //如果不符合过滤关键字规则,则不通过;
                if (!tinet.CheckPassFilterWork(remoteIDMsg))
                {
                    return;
                }

                //如果此节点已经被添加到节点列表了并且正在连接中或者已经连接了， 则不做处理。
                if (tinet.nodeMap.TryGetValue(remoteIDMsg.NodeName, out TiNetNode _node) && _node.State != TiNetNodeState.NotConnected)
                {
                    return;
                }

                //触发一次 OnNodeDiscovered  事件:
                try
                {
                    OnNodeDiscovered?.Invoke(endPoint, remoteIDMsg.CustomTag, remoteIDMsg.NodeID);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                //尝试和节点建立连接:
                {
                    if (tinet.DebugMode)
                    {
                        Debug.LogFormat("尝试和节点:{0}:{1}, name:{2}, id:{3}", endPoint, remoteIDMsg.ReliablePort, remoteIDMsg.NodeName, remoteIDMsg.NodeID);
                    }

                    //如果上层筛选器要求过滤此节点:
                    if (TiNet.OnTryToAddNode != null && !TiNet.OnTryToAddNode(remoteIDMsg.CustomTag, endPoint.Address))
                    {
                        return;
                    }
                    TiNetNode node = _node == null ? new TiNetNode() : _node;
                    node.nodeName = remoteIDMsg.NodeName;
                    node.nodeID = remoteIDMsg.NodeID;
                    node.isLocalNode = false;
                    node.address = endPoint.Address;
                    node.reliablePort = remoteIDMsg.ReliablePort;
                    node.unreliablePort = remoteIDMsg.UnreliablePort;
                    node.CustomTag = remoteIDMsg.CustomTag;
                    node.nodeStartTime = remoteIDMsg.NodeStartTime;
                    tinet.nodeMap.AddOrSetDictionary(node.nodeName, node);//添加进列表
                    tinet.m_nodes.AddUnduplicate(node);//添加进列表
                    node.state = TiNetNodeState.Connecting;//初始状态: 连接中
                    node.EndPoint_Reliable = new IPEndPoint(endPoint.Address, remoteIDMsg.ReliablePort);//end point: reliable
                    node.EndPoint_Unreliable = new IPEndPoint(endPoint.Address, remoteIDMsg.UnreliablePort); //end point: unreliable
                    node.FilterKeyword = remoteIDMsg.Keyword;

                    //如果此节点是来自于延迟注册连接:
                    if (tinet.CheckIsPendingRegistrationConnection(endPoint, out I_Connection iConn))
                    {
                        tinet.PendingRegistrationConnections.Remove(iConn);//从延迟注册列表移除
                        node.state = TiNetNodeState.Connected;//已连接
                        node.tcpSocket = iConn.socket;
                        OnNodeConnected?.Invoke(node);
                    }
                    await Instance.ConnectsToNode(node);//等待和远端节点建立连接
                }
            }
        }

        /// <summary>
        /// 在建立连接之前的普通检测.
        /// </summary>
        /// <returns></returns>
        private bool CommonValidationOnNodeSetup()
        {
            return this.NetworkStartTime.HasValue;
        }


        /// <summary>
        /// Trys to add a Ti-Network node.
        /// 添加一个 TiNetwork 节点。
        /// </summary>
        /// <returns></returns>
        [Obsolete("Deprecated - call ConnectsToNode(TiNetNode node) instead. ")]
        internal async Task<bool> TryAddNode(TiNetNode node)
        {
            return false;
            /*
            try
            {
                //TODO : try tcp connection:
                nodeMap.Add(node.nodeName, node);
                m_nodes.Add(node);

                TCPClient client = this.gameObject.AddChildGameObject<TCPClient>(node.nodeName);
                client.ServerAddress = node.address.ToString();
                client.ServerPort = node.reliablePort;
                client.AutoReconnect = true;
                client.HeartBeat = this.m_Strictmode;//是否打开心跳包的开关

                //在tcp client连上之后，发送一条 LAN Discovery message (以防自身广播失效）
                Action onConnected = null;
#pragma warning disable RECS0165 // 异步方法应返回 Task，而不应返回 void
                onConnected = async () =>
                {
#pragma warning restore RECS0165 // 异步方法应返回 Task，而不应返回 void
                    await new WaitForGameTime(1);//等待1s后发出:
                    LANDiscoveryMessage msg = TiNetworkMessagePool.GetMessage<LANDiscoveryMessage>();
                    msg.NodeName = MachineName;
                    msg.NodeID = this.TiNetIDNodeID;
                    msg.ReliablePort = this.PortReliable;
                    msg.UnreliablePort = this.PortUnreliable;
                    msg.MyRole = 0;
                    msg.CustomTag = this.CustomTag;
                    msg.NodeStartTime = this.NetworkStartTime.Value.Ticks;
                    this.SendReliableTo(node, msg);
                    client.OnClientConnected -= onConnected;
                };
                client.OnClientConnected += onConnected;
                var ConnTask = client.StartClient();
                if (DebugMode)
                    Debug.LogFormat(kLogMsg08, client.ServerAddress, client.ServerPort);

                await ConnTask;

                if (DebugMode)
                    Debug.LogFormat(kLogMsg07, node.nodeName, node.NodeID, ConnTask.Result);
                //failed:
                if (ConnTask.Result == false)
                {
                    nodeMap.Remove(node.nodeName);
                    m_nodes.Remove(node);
                    client.gameObject.Destroy();
                }
                //success:
                else
                {

                    node.tcpClientComponent = client;
                    //add tcp clients:
                    TcpClients.Add(client);
                    TcpNodeClientMap.Add(node, client);
                }
                OnNodeConnected?.Invoke(node);
                return ConnTask.Result;
            }
            catch (Exception exc)
            {
                Debug.LogException(exc);
                return false;
            }
            */
        }


    }
}