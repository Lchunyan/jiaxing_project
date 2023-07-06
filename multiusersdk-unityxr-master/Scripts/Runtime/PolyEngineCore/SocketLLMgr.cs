using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using Ximmerse.XR.Asyncoroutine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Collections.Concurrent;

namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// Low level socket manager.
    /// </summary>
    public class SocketLLMgr : SingletonMonobehaviour<SocketLLMgr>
    {
        /// <summary>
        /// TCP 发送请求。
        /// </summary>
        public class TcpClientSenderRequest
        {
            /// <summary>
            /// The tcp client socket
            /// </summary>
            public Socket tcpSocket;

            /// <summary>
            /// The byte[] array.
            /// </summary>
            public byte[] array;

            /// <summary>
            /// The write offset to the array
            /// </summary>
            public int offset;

            /// <summary>
            /// The write length
            /// </summary>
            public int length;

            public object userData;

            /// <summary>
            /// 1st param = user data,
            /// 2nd param = socket error code,
            /// 3nd param = send socket byte.
            /// </summary>
            public Action<object, SocketError, int> callback;
        }

        /// <summary>
        /// UDP 发送请求。
        /// </summary>
        public class UdpClientSenderRequest
        {
            /// <summary>
            /// The Udp client
            /// </summary>
            public UdpClient udpClient;

            /// <summary>
            /// The byte[] array.
            /// </summary>
            public byte[] array;

            /// <summary>
            /// The write offset to the array
            /// </summary>
            public int offset;

            /// <summary>
            /// The write length
            /// </summary>
            public int length;

            /// <summary>
            /// The destination end point
            /// </summary>
            public EndPoint endPoint;

            /// <summary>
            /// Callback.
            ///
            /// 1st boolean = is all the data sent?
            /// 2nd int = sent data length.
            /// </summary>
            public Action<bool, int> callback;
        }

        /// <summary>
        /// TCP outgoing queue.
        /// </summary>
        ConcurrentQueue<TcpClientSenderRequest> tcpOutgoingQueue = new ConcurrentQueue<TcpClientSenderRequest>();

        /// <summary>
        /// Udp outgoing queue.
        /// </summary>
        ConcurrentQueue<UdpClientSenderRequest> udpOutgoingQueue = new ConcurrentQueue<UdpClientSenderRequest>();

        /// <summary>
        /// The tcp outgoing thread.
        /// </summary>
        Thread tcpOutgoingThread = null;

        /// <summary>
        /// The udp outgoing thread.
        /// </summary>
        Thread udpOutgoingThread = null;

        /// <summary>
        /// If debug mode is on, debug message is printed.
        /// </summary>
        public bool DebugMode = false;

        public static SocketLLMgr CreateInstance()
        {
            if (HasInstance())
            {
                return Instance;
            }

            var gameObject = new GameObject("Socket Manager", new Type[] { typeof(SocketLLMgr) });
            DontDestroyOnLoad(gameObject);
            return gameObject.GetComponent<SocketLLMgr>();
        }

        /// <summary>
        /// 插入一条 TCP 发送请求。
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="callback"></param>
        public static void PushTcpSendRequest(TcpClient tcpClient, byte[] array, int offset, int length, Action<object, SocketError, int> callback, object userData)
        {
            if (!HasInstance())
            {
                CreateInstance();
            }
            Instance.tcpOutgoingQueue.Enqueue(new TcpClientSenderRequest()
            {
                array = array,
                length = length,
                tcpSocket = tcpClient.Client,
                offset = offset,
                callback = callback,
                userData = userData,
            });
        }

        /// <summary>
        /// 插入一条 TCP 发送请求。
        /// </summary>
        /// <param name="tcpSocket"></param>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="callback"></param>
        public static void PushTcpSendRequest(Socket tcpSocket, byte[] array, int offset, int length, Action<object, SocketError, int> callback, object userData)
        {
            if (!HasInstance())
            {
                CreateInstance();
            }
            Instance.tcpOutgoingQueue.Enqueue(new TcpClientSenderRequest()
            {
                array = array,
                length = length,
                tcpSocket = tcpSocket,
                offset = offset,
                callback = callback,
                userData = userData,
            });
        }

        /// <summary>
        /// 插入一条 UDP 发送请求。
        /// </summary>
        /// <param name="udpClient"></param>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="callback"></param>
        public static void PushUdpSendRequest(UdpClient udpClient, byte[] array, int offset, int length, EndPoint endPoint, Action<bool, int> callback)
        {
            if (!HasInstance())
            {
                CreateInstance();
            }
            Instance.udpOutgoingQueue.Enqueue(new UdpClientSenderRequest()
            {
                array = array,
                length = length,
                udpClient = udpClient,
                offset = offset,
                endPoint = endPoint,
                callback = callback,
            });
        }

        protected override void Awake()
        {
            base.Awake();
            //启动 tcp outgoing线程:
            tcpOutgoingThread = new Thread(TickTcpOutging);
            tcpOutgoingThread.Priority = System.Threading.ThreadPriority.Normal;
            tcpOutgoingThread.Start();
            //启动 udp outgoing线程:
            udpOutgoingThread = new Thread(TickUdpOutgoing);
            udpOutgoingThread.Priority = System.Threading.ThreadPriority.Normal;
            udpOutgoingThread.Start();
        }

        /// <summary>
        /// 处理发送TCP队列。
        /// </summary>
        private void TickTcpOutging()
        {
            while (true)
            {
                try
                {
                    while (tcpOutgoingQueue.TryDequeue(out TcpClientSenderRequest request))
                    {
                        if (request.tcpSocket == null)
                        {
                            continue;
                        }
                        Socket _tcpSock = request.tcpSocket;
                        if(_tcpSock == null)
                        {
                            continue;//skip dispose object
                        }
                        int sent = _tcpSock.Send(request.array, request.offset, request.length, SocketFlags.None, out SocketError errorCode);
                        if (DebugMode)
                        {
                            Debug.LogFormat("Socket LLMgr - TCP Socket sent error - {0}, sent byte = {1}, invoke callback is Null: {2}, callback = {3}", errorCode, sent, request.callback == null, request.callback);
                        }
                        //在unity main thread中触发回调:
                        if (request.callback != null)
                        {
                            Callback_OnSocketSent(request.userData, errorCode, sent, request.callback);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// On socket sent callback
        /// </summary>
        /// <param name="userData"></param>
        /// <param name="socketError"></param>
        /// <param name="sent"></param>
        /// <param name="action"></param>
        async void Callback_OnSocketSent(object userData, SocketError socketError, int sent, Action<object, SocketError, int> action)
        {
            await new WaitForNextFrame();
            try
            {
                action.Invoke(userData, socketError, sent);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// 处理发送 UDP 队列
        /// </summary>
        private void TickUdpOutgoing()
        {
            while (true)
            {
                try
                {
                    while (udpOutgoingQueue.TryDequeue(out UdpClientSenderRequest request))
                    {
                        if (request.udpClient == null || request.udpClient.Client == null)
                        {
                            continue;
                        }
                        Socket _udpSocket = request.udpClient.Client;
                        int sent = _udpSocket.SendTo(request.array, 0, request.length, SocketFlags.None, request.endPoint);
                        //在unity main thread中触发回调:
                        if (request.callback != null)
                        {
                            bool allSent = sent == request.length;
                            //Invoke callback at main thread:
                            Task.Run(async () =>
                            {
                                await new WaitForNextFrame();
                                request.callback?.Invoke(allSent, sent);
                            });
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                Thread.Sleep(1);
            }
        }


        /// <summary>
        /// 打印发送队列的信息。
        /// </summary>
        [InspectFunction]
        public static void DumpQueuePendingStatus()
        {
            if (!HasInstance())
            {
                CreateInstance();
            }
            Debug.LogFormat("Pending TCP outgoing queue: {0}, UDP outgoing queue: {1}", Instance.tcpOutgoingQueue.Count, Instance.udpOutgoingQueue.Count);
        }

    }
}