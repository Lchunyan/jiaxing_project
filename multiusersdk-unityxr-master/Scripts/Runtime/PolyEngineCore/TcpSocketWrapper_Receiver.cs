using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading.Tasks;
using System.Threading;
using Ximmerse.XR.Asyncoroutine;
using System.IO;

namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// TcpSocketWrapper_Receiver.cs - handle receive looping 
    /// </summary>
    internal partial class TcpSocketWrapper
    {
        /// <summary>
        /// 内部类：用于标记接收状态。
        /// </summary>
        class AsyncRecvState
        {
            public TcpClient client;

            /// <summary>
            /// 异常信息
            /// </summary>
            public Exception exception;

            /// <summary>
            /// 在出现了异常的情况下，是否要考虑断连
            /// </summary>
            public bool shouldDisconnect;

            /// <summary>
            /// 收到的字节数
            /// </summary>
            public int BytesReceived;

            public void Reset()
            {
                exception = null;
                shouldDisconnect = false;
                BytesReceived = 0;
            }
        }

        /// <summary>
        /// 默认的1级缓存数据池长度.
        /// </summary>
        const int kDefaultIncomingBufferSize = 1024 * 750;//750KB for incoming buffer

        /// <summary>
        /// 数据临时池，每次socket收到的数据先存在这个池中。
        /// 这是socket 通信的1级缓存.
        /// </summary>
        byte[] IncomingBuffer;

        /// <summary>
        /// 2级缓存， 1级缓存收到的数据存放在这里.
        /// </summary>
        internal List<byte> IncomingBytesList = new List<byte>();

        AsyncRecvState recvState;


        public int BufferSize_Incoming
        {
            get; private set;
        }

        /// <summary>
        /// 默认的接收超时， 3秒
        /// </summary>
        const float kDefaultReceiveTimeout = 3;

        /// <summary>
        /// 接收超时.
        /// </summary>
        public float ReceiveTimeout;

        CancellationTokenSource cancel_incoming;

        IAsyncResult iAsyncResult_Recv;

        /// <summary>
        /// 收到的Byte[]数据缓存列表.
        /// </summary>
        List<byte[]> ReceivedMessage = new List<byte[]>();

        //public int TotalRecvBytes = 0;


        /// <summary>
        /// 一次性写出所有的数据
        /// </summary>
        public void FlushRawRecvData(List<byte[]> list)
        {
            lock (mutex_recv_buffer)
            {
                list.AddRange(ReceivedMessage);
                ReceivedMessage.Clear();
                RecvMessageCount = 0;
            }
        }

        /// <summary>
        /// Mutex : access recv buffer.
        /// </summary>
        System.Object mutex_recv_buffer = new object();

        /// <summary>
        /// 记录上一次接收信息的时间.
        /// </summary>
        DateTime LastRecvTime;

        /// <summary>
        /// 收到的消息计数.
        /// </summary>
        public int RecvMessageCount
        {
            get; private set;
        }

        /// <summary>
        /// 初始化读取部分。
        /// </summary>
        internal void InitRecv(int IncomingBufferSize = kDefaultIncomingBufferSize, float _ReceiveTimeout = kDefaultReceiveTimeout)
        {
            if (IncomingBufferSize <= kMessageHeader.Length * 10)
            {
                throw new Exception("IncomingBufferSize too small : " + IncomingBufferSize);
            }
            IncomingBuffer = new byte[IncomingBufferSize];
            BufferSize_Incoming = IncomingBufferSize;
            ReceiveTimeout = _ReceiveTimeout;
            recvState = new AsyncRecvState();
            cancel_incoming = new CancellationTokenSource();
            RecvMessageCount = 0;
            //异步:
            Task.Run(ReceiveLoop);
            LastRecvTime = DateTime.Now;
            //_ = ReceiveLoop(cancel_incoming.Token);
        }
        /// <summary>
        /// 循环读取,此方法在子线程中调用。
        /// </summary>
        private void ReceiveLoop()
        {
            if (m_client == null || m_client.Client == null)
            {
                throw new Exception("TCPClient socket == NULL");
            }

            while (!_disposed)
            {
                if (m_client == null || m_client.Client == null || _disposed || !m_client.Client.Connected)
                {
                    break;
                }

                bool error = false;
                bool isDataDirty = false;//是否收到了新的数据， 导致本地1级缓存变脏
                try
                {
                    recvState.Reset();
                    recvState.client = m_client;

                    //每次接收都从0开始
                    int recvBytes = m_client.Client.Receive(IncomingBuffer, 0, BufferSize_Incoming, SocketFlags.None, out SocketError errorCode);
                    //更新接收时间,用于做PING判断.
                    LastRecvTime = DateTime.Now;
                    //TotalRecvBytes += recvBytes;
                    bool isHeartBeatMessage = PEUtils.CompareArray(IncomingBuffer, 0, kDummyMessage, 0, kDummyMessage.Length);
                    if (isHeartBeatMessage)
                    {
#if UNITY_EDITOR
                        //Debug.Log("Recv Heartbeat");
#endif
                    }
                    else
                    {
#if UNITY_EDITOR
                        //Debug.LogFormat("Recv packet length: {0}, socket error code: {1} @time: {2}", recvBytes, errorCode, LastRecvTime.ToString());
#endif
                    }

                    //对于非心跳包数据,
                    //将socket收到的原始数据，推入2级缓存
                    if(!isHeartBeatMessage)
                    {
                        if (recvBytes > 0)
                        {
                            //添加进 Buffer:
                            IncomingBytesList.AddRange(IncomingBuffer, 0, recvBytes);
                            isDataDirty = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    error = true;
                }

                if (isDataDirty && !error)
                {
                    bool keepParsing = true;
                    while (keepParsing)
                    {
                        //当收到了数据以后， 启动数据分析
                        //如果使用原始socket通信模式，则不需要分析数据流，直接从收到的原始数据流输出。
                        //否则需要解析数据流，拆分数据包:
                        if (!UsingRawSocket)
                        {
                            //对消息做结构化的解析
                            bool parsed = ParseData(IncomingBytesList, out byte[] data);
                            //Debug.LogFormat("Parse incoming bytes list: {0}", parsed);
                            if (parsed)
                            {
                                //   Debug.LogFormat("Get complete data in size: {0}", data.Length);
                                //将解析出来的数据放入全局列表中:
                                //对于为Null的数据包 ，代表是一个Ping 数据，直接抛弃
                                if (data != null)
                                {
                                    lock (mutex_recv_buffer)
                                    {
                                        ReceivedMessage.Add(data);
                                        RecvMessageCount = ReceivedMessage.Count;
                                    }
                                }

                                if (IncomingBytesList.Count > 0)
                                {
                                    keepParsing = true;
                                }
                            }
                            else
                            {
                                keepParsing = false;
                            }
                        }
                        else
                        {
                            //使用 raw socket 通信模式:
                            lock (mutex_recv_buffer)
                            {
                                byte[] data = IncomingBytesList.ToArray();
                                ReceivedMessage.Add(data);
                                RecvMessageCount = ReceivedMessage.Count;
                                IncomingBytesList.Clear();//在Raw socket模式下，接受完毕后要清空消息缓存.
                            }
                            keepParsing = false;//直接输出所有的数据， 不需要多次解析.
                        }
                    }

                }

                //await new WaitForNextFrame();
                Thread.Sleep(1);
            }


        }

        /// <summary>
        /// 对收到的 raw buffer data 数据做一次扫描。如果获取了有效数据, 则
        /// </summary>
        /// <param name="ByteList"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static bool ParseData(List<byte> ByteList, out byte[] data)
        {
            var messageHeader = kMessageHeader;
            int msgHeaderLength = messageHeader.Length;
            int index_HeaderData = -1;//数据中的消息头的索引
            int index_DataLength = -1;//数据中的数据长度索引.
            int index_DataContent = -1;//数据长度.
            int totalCount = ByteList.Count;

            //解析出的数据长度.
            int dataLength = -1;

            //如果parseComplete, 代表收到了完整的包.
            bool parseComplete = false;

            try
            {
                for (int i = 0; i < ByteList.Count - msgHeaderLength; i++)
                {
                    //查找出 kMessageHeader:
                    if (PEUtils.CompareArray(messageHeader, 0, ByteList, i, msgHeaderLength))
                    {
                        index_HeaderData = i;
                        index_DataLength = index_HeaderData + msgHeaderLength;//数据长度index起点
                        dataLength = PEUtils.ReadInt(ByteList, index_DataLength);
                        index_DataContent = index_DataLength + 4;
                        //收到的数据长度 > 全包长度，说明数据还没有收全.
                        if ((index_DataContent + dataLength) > totalCount)
                        {
                            //Debug.LogFormat("等待全包 : {0}/{1}", dataLength, totalCount);
                            parseComplete = false;
                        }
                        else
                        {
                            parseComplete = true;
                        }
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            if (parseComplete)
            {
                //对于有实体消息（也就是消息长度> 0 )， copy出数据:
                if (dataLength > 0)
                {
                    data = new byte[dataLength];
                    //从 header 到结束的总数据字节长度.
                    int dataRangeLength = msgHeaderLength + 4 + dataLength;//原始数据的总长度 : header + 4(data length)
                    int index = 0;
                    for (int i = index_DataContent; i < index_DataContent + dataLength; i++)
                    {
                        data[index++] = ByteList[i];
                    }
                    ByteList.RemoveRange(index_HeaderData, dataRangeLength);
                }
                else
                {
                    //对于无实体消息， 直接删除消息头+长度字节
                    data = null;
                    ByteList.RemoveRange(index_HeaderData, msgHeaderLength + 4);
                }

                return true;
            }
            else
            {
                data = null;
                return false;
            }
        }
    }
}
