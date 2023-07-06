using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Net;
using System.IO;
using System.Text;
using Ximmerse.XR.Asyncoroutine;
using System.Collections.Concurrent;
using System.Net.Sockets;


namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// TiNet_OutgoingDaemon.cs
    /// </summary>
    public partial class TiNet
    {

        IPAddress localIP = null;
        IPAddress subnetBroadcastAddr = null;

        //System.Object poolListLock = new object();

        //List<List<TiNetworkPacket>> poolNetworkPacketList = new List<List<TiNetworkPacket>>();
        ConcurrentQueue<List<TiNetworkPacket>> poolNetworkPacketList = new ConcurrentQueue<List<TiNetworkPacket>>();

        void SetupPoolList()
        {
            for (int i = 0; i < 30; i++)
            {
                //poolNetworkPacketList.Add(new List<TiNetworkPacket>());
                poolNetworkPacketList.Enqueue(new List<TiNetworkPacket>());
            }
        }

        /// <summary>
        /// 获取缓存池中的network packet 列表.
        /// </summary>
        /// <returns></returns>
        List<TiNetworkPacket> GetCacheTiNetworkPacketList()
        {
            //lock (poolListLock)
            //{
            //    if (poolNetworkPacketList.Count > 0)
            //    {
            //        var lst = poolNetworkPacketList[poolNetworkPacketList.Count - 1];
            //        lst.Clear();
            //        poolNetworkPacketList.RemoveAt(poolNetworkPacketList.Count - 1);
            //        return lst;
            //    }
            //}

            if (poolNetworkPacketList.TryDequeue(out List<TiNetworkPacket> lst) && lst != null)
            {
                return lst;
            }

            return new List<TiNetworkPacket>();
        }

        void ReturnTiNetworkPacketToPool(List<TiNetworkPacket> list)
        {
            list.Clear();
            //lock (poolListLock)
            //    poolNetworkPacketList.Add(list);
            poolNetworkPacketList.Enqueue(list);
        }

        /// <summary>
        /// UDP broadcast message to LAN
        /// </summary>
        /// <param name="message"></param>
        public void Broadcast(TiNetMessage message)
        {
            if (externalTransportLayer != null)
            {
                externalTransportLayer.BroadcastToLAN(message);
                return;
            }
            if (localIP == null)
            {
                PENetworkUtils.GetLocalIP(out localIP);
                PENetworkUtils.GetSubnetBroadcastAddress(localIP, out subnetBroadcastAddr);
                //byte[] subnetBytes = subnetBroadcastAddr.GetAddressBytes();
                //int i = subnetBytes.Length;
            }
            var packet = TiNetworkPacketPool.GetNetworkPacket();
            //packet.endPoint.Address = IPAddress.Broadcast;
            packet.endPoint.Address = subnetBroadcastAddr;
            packet.endPoint.Port = this.PortBroadcast;
            //缓存池的前2字节 = Message KeyCode:
            short messageCode = TiNetworkMessagePool.GetAttribute(message.GetType()).MessageCode;
            //第二个字节开始 = 报文数据:
            int index = PEUtils.WriteShort(messageCode, packet.Data, 0);
            packet.DataLength = 2 + message.Serialize(packet.Data, index);
            broadcastSender.Broadcast(packet);
            //TiNetMessage 归池
            message.IsSent = true;
            TiNetworkMessagePool.ReturnMessage(messageCode, message);
        }


        /// <summary>
        /// Sends a reliable TiNetwork message by TCP/IP 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        public void SendToAllReliable(TiNetMessage message)
        {
            if (externalTransportLayer != null)
            {
                externalTransportLayer.EnqueueMessage(message, null, true);
                return;
            }
            if (m_nodes.Count == 0)
            {
                return;
            }
            //为每一个connect状态的node，发送消息:
            for (int i = 0; i < m_nodes.Count; i++)
            {
                TiNetNode _node = m_nodes[i];
                SendReliableTo(_node, message);
            }
        }

        /// <summary>
        /// Sends a TiNetwork message by TCP/IP 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node">发送目标节点</param>
        /// <param name="message"></param>
        public void SendReliableTo(I_TiNetNode _node, TiNetMessage message)
        {
            if (externalTransportLayer != null)
            {
                externalTransportLayer.EnqueueMessage(message, _node, true);
                return;
            }
            if (_node.State != TiNetNodeState.Connected || !(_node is TiNetNode))
            {
                return;//node is not ready.
            }
            var n = _node as TiNetNode;
            byte[] _data = new byte[1024 * 5];//default size: 5kb, todo  : use message hint size

            //消息头:
            short messageCode = TiNetworkMessagePool.GetAttribute(message.GetType()).MessageCode;
            int index = PEUtils.WriteShort(messageCode, _data, TcpSocketWrapper.kMessageHeader.Length + 4);//消息头是message header + 包体长度 (4位的int)
            int _length = 2 + message.Serialize(_data, index);//写入消息包体内容

            Array.ConstrainedCopy(TcpSocketWrapper.kMessageHeader, 0, _data, 0, TcpSocketWrapper.kMessageHeader.Length);//copy 消息头
            PEUtils.WriteInt(_length, _data, TcpSocketWrapper.kMessageHeader.Length);//写入消息的长度

            int totalMsgLength = TcpSocketWrapper.kMessageHeader.Length + 4 + _length;
            //添加进 Tcp Socket wrapper 队列:
            TcpSocketWrapper.sPushOutgoingMessage(n.tcpSocket, _data, 0, totalMsgLength, false, callback: (userData, err, sent) =>
            {
                if (err != System.Net.Sockets.SocketError.Success)
                {
                    this.OnSentError(userData as TiNetNode, err, sent);
                }
            }, UserData: n
            );
            n.previousSentMessageTime = DateTime.Now;//记录上一次发送时间。
            message.IsSent = true;
        }

        /// <summary>
        /// Sends a TiNetwork message by UDP
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        public async Task SendToAllUnreliable(TiNetMessage message)
        {
            if (externalTransportLayer != null)
            {
                externalTransportLayer.EnqueueMessage(message, null, false);
                return;
            }
            if (m_nodes.Count == 0)
            {
                return;
            }
            var packet = TiNetworkPacketPool.GetNetworkPacket();
            //缓存池的前2字节 = Message KeyCode:
            short messageCode = TiNetworkMessagePool.GetAttribute(message.GetType()).MessageCode;
            //第二个字节开始 = 报文数据:
            int index = PEUtils.WriteShort(messageCode, packet.Data, 0);
            packet.DataLength = 2 + message.Serialize(packet.Data, index);
            //使用一个 temp list 监控 packets 的发送情况:
            List<TiNetworkPacket> packets = GetCacheTiNetworkPacketList();
            packets.Add(packet);
            for (int i = 0, MaxNodeCount = this.m_nodes.Count; i < MaxNodeCount; i++)
            {
                //TiNetworkPacket 是引用类型,每次都需要更新引用对象:
                if (i != 0)
                {
                    var newPacket = TiNetworkPacketPool.GetNetworkPacket();
                    newPacket.DataLength = packet.DataLength;
                    Array.Copy(packet.Data, 0, newPacket.Data, 0, packet.DataLength);
                    packet = newPacket;
                    packets.Add(newPacket);
                }

                var node = this.m_nodes[i];
                packet.endPoint.Address = node.address;
                packet.endPoint.Port = node.unreliablePort;
                this.udpSender.Send(packet);
            }

            while (packets.Count > 0)
            {
                for (int i = packets.Count - 1; i >= 0; i--)
                {
                    if (packets[i].isProcessed)
                    {
                        packets.RemoveAt(i);
                    }
                }
                await new WaitForNextFrame();
            }
            //return to pool:
            ReturnTiNetworkPacketToPool(packets);
            //TiNetMessage 归池
            message.IsSent = true;
            TiNetworkMessagePool.ReturnMessage(messageCode, message);
        }


        /// <summary>
        /// Sends a TiNetwork message by UDP
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="message"></param>
        public async Task SendUnreliableTo(I_TiNetNode node, TiNetMessage message)
        {
            if (externalTransportLayer != null)
            {
                externalTransportLayer.EnqueueMessage(message, node, false);
                return;
            }
            var packet = TiNetworkPacketPool.GetNetworkPacket();
            //缓存池的前2字节 = Message KeyCode:
            short messageCode = TiNetworkMessagePool.GetAttribute(message.GetType()).MessageCode;
            //第二个字节开始 = 报文数据:
            int index = PEUtils.WriteShort(messageCode, packet.Data, 0);
            packet.DataLength = 2 + message.Serialize(packet.Data, index);
            bool found = false;

            for (int i = 0, maxcount = m_nodes.Count; i < maxcount; i++)
            {
                TiNetNode _node = m_nodes[i];
                if (_node == node)
                {
                    found = true;
                    packet.endPoint.Address = _node.address;
                    packet.endPoint.Port = _node.unreliablePort;
                    this.udpSender.Send(packet);
                    break;
                }
            }

            if (!found)
            {
                Debug.LogWarningFormat("TiNet.SendReliableTo : unknown Ti-Node : {0}", node.ToString());
                //TiNetMessage 归池
                message.IsSent = true;
                TiNetworkMessagePool.ReturnMessage(messageCode, message);
            }
            else//等待packet 处理完毕后释放 TiNetMessage 对象。
            {
                while (!packet.isProcessed)
                {
                    await new WaitForNextFrame();
                }
                //TiNetMessage 归池
                message.IsSent = true;
                TiNetworkMessagePool.ReturnMessage(messageCode, message);
            }
        }

        /// <summary>
        /// Sends a text message to all node, in reliable channel.
        /// </summary>
        /// <param name="text"></param>
        public void SendTextMessageToAll_Reliable(string text)
        {
            SimpleTextMessage simpleTextMessage = TiNetworkMessagePool.GetMessage<SimpleTextMessage>();
            simpleTextMessage.Text = text;
            SendToAllReliable(simpleTextMessage);
        }



        /// <summary>
        /// Sends a text message to target node, in reliable channel.
        /// </summary>
        /// <param name="text"></param>
        public void SendTextMessageToNode_Reliable(string text, I_TiNetNode node)
        {
            SimpleTextMessage simpleTextMessage = TiNetworkMessagePool.GetMessage<SimpleTextMessage>();
            simpleTextMessage.Text = text;
            SendReliableTo(node, simpleTextMessage);
        }

        /// <summary>
        /// Sends a text message to all node, in unreliable channel.
        /// </summary>
        /// <param name="text"></param>
        public async Task SendTextMessageToAll_Unreliable(string text)
        {
            SimpleTextMessage simpleTextMessage = TiNetworkMessagePool.GetMessage<SimpleTextMessage>();
            simpleTextMessage.Text = text;
            await SendToAllUnreliable(simpleTextMessage);
        }



        /// <summary>
        /// Sends a text message to target node, in unreliable channel.
        /// </summary>
        /// <param name="text"></param>
        public async Task SendTextMessageToNode_Unreliable(string text, I_TiNetNode node)
        {
            SimpleTextMessage simpleTextMessage = TiNetworkMessagePool.GetMessage<SimpleTextMessage>();
            simpleTextMessage.Text = text;
            await SendUnreliableTo(node, simpleTextMessage);
        }



        /// <summary>
        /// Sends a byte[] message to all node, in reliable channel.
        /// </summary>
        /// <param name="buffer">Maximum 1KB</param>
        /// <param name="startIndex"></param>
        /// <param name="length">Maximum 1KB</param>
        public void SendDataMessageToAll_Reliable(byte[] buffer, int startIndex, int length)
        {
            SimpleBytesMessage simpleBytesMsg = TiNetworkMessagePool.GetMessage<SimpleBytesMessage>();
            Array.ConstrainedCopy(buffer, startIndex, simpleBytesMsg.buffer, 0, length);
            simpleBytesMsg.length = length;
            SendToAllReliable(simpleBytesMsg);
        }

        /// <summary>
        /// Sends a byte[] message to all node, in reliable channel.
        /// </summary>
        /// <param name="buffer">Maximum 1KB</param>
        /// <param name="startIndex"></param>
        /// <param name="length">Maximum 5KB</param>
        public async Task SendDataMessageToAll_Unreliable(byte[] buffer, int startIndex, int length)
        {
            SimpleBytesMessage simpleBytesMsg = TiNetworkMessagePool.GetMessage<SimpleBytesMessage>();
            Array.ConstrainedCopy(buffer, startIndex, simpleBytesMsg.buffer, 0, length);
            simpleBytesMsg.length = length;
            await SendToAllUnreliable(simpleBytesMsg);
        }


        /// <summary>
        /// Sends a text message to target node, in reliable channel.
        /// </summary>
        /// <param name="buffer">Maximum 1KB</param>
        /// <param name="startIndex"></param>
        /// <param name="length">Maximum 1KB</param>
        public void SendDataMessageToNode_Reliable(byte[] buffer, int startIndex, int length, I_TiNetNode node)
        {
            SimpleBytesMessage simpleBytesMsg = TiNetworkMessagePool.GetMessage<SimpleBytesMessage>();
            Array.ConstrainedCopy(buffer, startIndex, simpleBytesMsg.buffer, 0, length);
            simpleBytesMsg.length = length;
            SendReliableTo(node, simpleBytesMsg);
        }


        /// <summary>
        /// Sends a text message to target node, in reliable channel.
        /// </summary>
        /// <param name="buffer">Maximum 1KB</param>
        /// <param name="startIndex"></param>
        /// <param name="length">Maximum 1KB</param>
        public async Task SendDataMessageToNode_Unreliable(byte[] buffer, int startIndex, int length, I_TiNetNode node)
        {
            SimpleBytesMessage simpleBytesMsg = TiNetworkMessagePool.GetMessage<SimpleBytesMessage>();
            Array.ConstrainedCopy(buffer, startIndex, simpleBytesMsg.buffer, 0, length);
            simpleBytesMsg.length = length;
            await SendUnreliableTo(node, simpleBytesMsg);
        }

        /// <summary>
        /// 以流的形式发送文件到 Node 节点。
        /// </summary>
        /// <param name="FilePath"></param>
        /// <param name="node">接受点。</param>
        /// <param name="FileAlias">文件的自定义名，如果为空，则使用File的本名。FileAlias 可以包含扩展名, 例如 : file_exp.txt</param>
        /// <returns>Sent result.</returns>
        public async Task SendFile(string FilePath, I_TiNetNode node, string FileAlias = "")
        {
            if (!File.Exists(FilePath))
            {
                Debug.LogErrorFormat("{0} not exists !", FilePath);
                return;
            }
            FileStream fs = null;
            try
            {
                fs = File.OpenRead(FilePath);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                if (fs != null)
                    fs.Close();
                return;
            }

            try
            {
                int offset = 0;
                int totalReadCount = 0;
                FileInfo fInfo = new FileInfo(FilePath);
                long fileSize = fInfo.Length;
                int bufferCount = TransferDataMessage.kBufferSize;
                ushort totalPacketCount = (ushort)Mathf.CeilToInt(fileSize / (float)bufferCount);
                ushort totalSentPacketCount = 0;
                if (this.DebugMode)
                {
                    Debug.LogFormat("Total packet required : {0}", totalPacketCount);
                }
                string fileName = string.IsNullOrEmpty(FileAlias) ? Path.GetFileName(FilePath) : FileAlias;
                List<TransferDataMessage> msgs = new List<TransferDataMessage>();
                bool usingExternalTransport = TiNet.externalTransportLayer != null;
                while (true)
                {
                    TransferDataMessage _TransferDataMessage = TiNetUtility.GetMessage<TransferDataMessage>();
                    var data = _TransferDataMessage.buffer;
                    int readCount = fs.Read(data, offset, data.Length);
                    _TransferDataMessage.length = readCount;
                    totalReadCount += readCount;
                    //Debug.LogFormat("Total read count = {0}", totalReadCount);
                    //如果读不到数据了
                    if (readCount == 0)
                    {
                        //不需要此 _TransferDataMessage 对象了，归池:
                        TiNetworkMessagePool.ReturnMessage(0x0005, _TransferDataMessage);
                        Debug.LogFormat("File : {0} read completed, total read count : {1}, total sent packet count : {2}", FilePath, totalReadCount, totalSentPacketCount);
                        break;
                    }
                    else
                    {
                        _TransferDataMessage.name = fileName;
                        _TransferDataMessage.packetIndex = totalSentPacketCount;
                        _TransferDataMessage.packetTotalCount = totalPacketCount;
                        totalSentPacketCount += 1;
                        //发送 data 到 node:
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                        _TransferDataMessage.SendToReliable(node);
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                        if (instance.DebugMode)
                        {
                            Debug.LogFormat("File : {0} sent packet count : {1}/{2}", FilePath, totalSentPacketCount, totalPacketCount);
                        }
                        msgs.Add(_TransferDataMessage);
                    }
                    if(usingExternalTransport)
                    {
                        await new WaitForGameTime(0.02f);//waits a network frame
                    }
                }

                while (msgs.Count > 0)
                {
                    await new WaitForNextFrame();
                    for (int i = msgs.Count - 1; i >= 0; i--)
                    {
                        TransferDataMessage msg = msgs[i];
                        if (msg.IsSent)
                        {
                            TiNetworkMessagePool.ReturnMessage(0x0005, msg);
                            msgs.RemoveAt(i);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            //发送完毕, 关闭文件流。
            try
            {
                if (fs != null)
                    fs.Close();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// 和 SendFile 相近，直接发送byte[]数组。
        /// </summary>
        /// <param name="buffer">数据缓存</param>
        /// <param name="offset">数据起点</param>
        /// <param name="length">数据总长</param>
        /// <param name="node"></param>
        /// <param name="name">数据的名字</param>
        /// <param name="UsingUnreliable">是否使用UDP通道发送. 默认: false, 使用 tcp/ip 通道.</param>
        public async Task SendBulkData(byte[] buffer, int offset, int length, I_TiNetNode node, string name, bool UsingUnreliable = false)
        {
            int chunk = TransferDataMessage.kBufferSize;//chunk : 每个数据包的大小
            int chunkCount = Mathf.CeilToInt(length / (float)chunk);//需要拆分成多少个数据包
            List<TransferDataMessage> msgs = new List<TransferDataMessage>();
            bool usingExternalTransport = TiNet.externalTransportLayer != null;
            for (int i = 0; i < chunkCount; i++)
            {
                int startIndex = offset + i * chunk;
                int endIndex = Mathf.Min((startIndex + chunk), offset + length);
                int _length = endIndex - startIndex;
                TransferDataMessage _TransferDataMessage = TiNetUtility.GetMessage<TransferDataMessage>();
                _TransferDataMessage.name = name;
                _TransferDataMessage.packetIndex = (ushort)i;
                _TransferDataMessage.packetTotalCount = (ushort)chunkCount;
                _TransferDataMessage.length = _length;
                Array.Copy(buffer, startIndex, _TransferDataMessage.buffer, 0, _length);
                //发送 data 到 node:
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                if (UsingUnreliable)
                {
                    _TransferDataMessage.SendToUnreliable(node);
                }
                else
                {
                    _TransferDataMessage.SendToReliable(node);
                }
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                msgs.Add(_TransferDataMessage);
                if (instance.DebugMode)
                {
                    Debug.LogFormat("BulkData : {0} sent packet count : {1}/{2}", name, i, chunkCount);
                }
                if (usingExternalTransport)
                {
                    await new WaitForGameTime(0.02f);//waits a network frame
                }
            }
            while (msgs.Count > 0)
            {
                await new WaitForNextFrame();
                for (int i = msgs.Count - 1; i >= 0; i--)
                {
                    TransferDataMessage msg = msgs[i];
                    if (msg.IsSent)
                    {
                        TiNetworkMessagePool.ReturnMessage(0x0005, msg);
                        msgs.RemoveAt(i);
                    }
                }
            }
        }

    }
}