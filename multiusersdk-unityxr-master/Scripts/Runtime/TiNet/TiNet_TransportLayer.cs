using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// TiNet_TransportLayer.cs.
    /// </summary>
    public partial class TiNet
    {
        static I_TiNetTransportLayer externalTransportLayer = null;

        public static int PreferredPacketBufferSize
        {
            get
            {
                return externalTransportLayer != null ? externalTransportLayer.PacketBufferSize : 1024 * 3;
            }
        }

        /// <summary>
        /// Register external transport layer.
        /// Must be call before TiNet starts.
        /// </summary>
        /// <param name="_transportLayer"></param>
        public static void RegisterExternalTransport(I_TiNetTransportLayer _transportLayer)
        {
            externalTransportLayer = _transportLayer;
            externalTransportLayer.OnDiscoverNewNode -= ExternalTransportLayer_OnDiscoverNewNode;
            externalTransportLayer.OnDiscoverNewNode += ExternalTransportLayer_OnDiscoverNewNode;

            externalTransportLayer.OnConnectingToNode -= ExternalTransportLayer_OnConnectingToNode;
            externalTransportLayer.OnConnectingToNode += ExternalTransportLayer_OnConnectingToNode;

            externalTransportLayer.OnConnectedToNode -= ExternalTransportLayer_OnConnectedToNode;
            externalTransportLayer.OnConnectedToNode += ExternalTransportLayer_OnConnectedToNode;


            externalTransportLayer.OnDisconnect -= ExternalTransportLayer_OnDisconnect;
            externalTransportLayer.OnDisconnect += ExternalTransportLayer_OnDisconnect;

            externalTransportLayer.OnMessage -= ExternalTransportLayer_OnMessage;
            externalTransportLayer.OnMessage += ExternalTransportLayer_OnMessage;

            Debug.LogFormat("External transport layer: {0} is registered.", _transportLayer.GetType());
        }

        private static void ExternalTransportLayer_OnMessage(TiNetMessage msg, I_TiNetNode node)
        {
            TiNet.instance._ExternalTransportLayer_OnMessage(msg, node);
        }


        /// <summary>
        /// 处理 msg 对应的回调处理方法.
        /// </summary>
        /// <param name="msg"></param>
        private void _ExternalTransportLayer_OnMessage(TiNetMessage msg, I_TiNetNode node)
        {
            if (!TiNetUtility.GetMessageCode(msg.GetType(), out short code))
            {
                Debug.LogFormat("Unknown message handler of type : {0}", msg.GetType());
                return;
            }

            //Handle internal preserved message :
            IPEndPoint endPoint = (node != null && node.EndPoint_Reliable != null) ? node.EndPoint_Reliable :
                (node != null && node.EndPoint_Unreliable != null ? node.EndPoint_Unreliable : default(IPEndPoint));
            if (code == 1)
            {
                TiNetDiscovery.OnRemoteIDMessage(msg, endPoint);
            }
            //Preserved msg : text msg
            else if (code == 2)
            {
                TiNetMessageReceiver.OnSimpleTextMessage(msg, endPoint);
            }
            //Preserved msg : data msg
            else if (code == 3)
            {
                TiNetMessageReceiver.OnDataHandler(msg, endPoint);
            }
            //Preserved msg : query info msg
            else if (code == 4)
            {
                TiNetMessageReceiver.OnQueryMessageHandler(msg, endPoint);
            }
            //Preserved msg : send file msg
            else if (code == 5)
            {
                TiNetMessageReceiver.OnSendFileMessage(msg, endPoint);
            }
            //Ban ip message
            else if (code == 7)
            {
                TiNetMessageReceiver.OnBandIPMessageHandler(msg, endPoint);
            }
            else if (TiMessageHandlers02.TryGetValue(code, out System.Action<TiNetMessage, I_TiNetNode> action) && action != null)
            {
                try
                {
                    action?.Invoke(msg, node);
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private static void ExternalTransportLayer_OnConnectingToNode(I_TiNetNode node)
        {
            TiNetNode tinetNode = node as TiNetNode;
            if (tinetNode == null)
            {
                Debug.LogErrorFormat("I_TiNetNode : {0} is not a TiNetNode !", node.GetType().Name);
                return;
            }
            tinetNode.state = TiNetNodeState.Connecting;
        }

        /// <summary>
        /// On transport layer discover a new node.
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        private static I_TiNetNode ExternalTransportLayer_OnDiscoverNewNode(TiNetNodeDescriptor descriptor)
        {
            var tinet = TiNet.instance;
            return tinet._ExternalTransportLayer_OnDiscoverNewNode(descriptor);
        }


        private I_TiNetNode _ExternalTransportLayer_OnDiscoverNewNode(TiNetNodeDescriptor descriptor)
        {
            if (descriptor.TiNetNodeID == this.thisNode.nodeID)
            {
                return thisNode;
            }

            //get remote node:
            bool get = this.GetTiNetNode(descriptor.TiNetMachineName, out I_TiNetNode node);

            //如果是新的节点，则创建实例:
            if (!get)
            {
                TiNetNode newNode = new TiNetNode();
                newNode.nodeName = descriptor.TiNetMachineName;
                newNode.nodeID = descriptor.TiNetNodeID;
                newNode.isLocalNode = false;
                newNode.address = descriptor.RemoteAddress;
                newNode.reliablePort = descriptor.ReliablePort;
                newNode.unreliablePort = descriptor.UnreliablePort;
                newNode.CustomTag = descriptor.NodeTag;
                newNode.nodeStartTime = descriptor.NetworkStartTime;
                nodeMap.AddOrSetDictionary(newNode.nodeName, newNode);//添加进列表
                m_nodes.AddUnduplicate(newNode);//添加进列表
                newNode.state = TiNetNodeState.NotConnected;//未连接
                newNode.EndPoint_Reliable = new IPEndPoint(descriptor.RemoteAddress, descriptor.ReliablePort);//end point: reliable
                newNode.EndPoint_Unreliable = new IPEndPoint(descriptor.RemoteAddress, descriptor.UnreliablePort); //end point: unreliable
                newNode.FilterKeyword = descriptor.FilterKeyword;

                node = newNode;
            }

            return node;
        }


        private static void ExternalTransportLayer_OnConnectedToNode(I_TiNetNode node)
        {
            TiNet.instance._ExternalTransportLayer_OnConnectedToNode(node);
        }

        private I_TiNetNode _ExternalTransportLayer_OnConnectedToNode(I_TiNetNode node)
        {
            TiNetNode n = node as TiNetNode;
            n.state = TiNetNodeState.Connected;
            try
            {
                OnNodeConnected?.Invoke(node);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return node;
        }

        private static void ExternalTransportLayer_OnDisconnect(I_TiNetNode obj)
        {
            TiNet.instance._ExternalTransportLayer_OnDisconnect(obj);
        }

        private I_TiNetNode _ExternalTransportLayer_OnDisconnect(I_TiNetNode node)
        {
            TiNetNode n = node as TiNetNode;
            n.state = TiNetNodeState.NotConnected;
            try
            {
                TiNet.OnNodeDisconnected?.Invoke(node);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return node;
        }

        public static void UnregisterExternalTransport()
        {
            externalTransportLayer.OnDiscoverNewNode -= ExternalTransportLayer_OnDiscoverNewNode;
            externalTransportLayer.OnConnectingToNode -= ExternalTransportLayer_OnConnectingToNode;
            externalTransportLayer.OnConnectedToNode -= ExternalTransportLayer_OnConnectedToNode;
            externalTransportLayer.OnDisconnect -= ExternalTransportLayer_OnDisconnect; ;
            externalTransportLayer.OnMessage -= ExternalTransportLayer_OnMessage;
            externalTransportLayer = null;
            Debug.Log("External transport layer is unregistered.");
        }
    }
}