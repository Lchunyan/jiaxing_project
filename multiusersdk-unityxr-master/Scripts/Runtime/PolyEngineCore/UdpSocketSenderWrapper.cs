using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Ximmerse.XR.Asyncoroutine;
namespace Ximmerse.XR.UnityNetworking
{

    /// <summary>
    /// UdpSocketSenderWrapper.cs
    /// </summary>
    public class UdpSocketSenderWrapper : IDisposable
    {
        /// <summary>
        /// a unity engine object as a context to engine operation, like log, coroutine, etc.
        /// </summary>
        public UnityEngine.Object UnityObject
        {
            get; private set;
        }

        private const string kErrorMsg = "UDP outgoing thread is aborted.";

        bool _disposed = false;

        Socket socket = null;

        /// <summary>
        /// Threading mutex.
        /// </summary>
        System.Object mutex = new object();

        CancellationTokenSource cancellation;

        /// <summary>
        /// The outgoing pending packets.
        /// </summary>
        List<TiNetworkPacket> PendingPackets = new List<TiNetworkPacket>();

        /// <summary>
        /// The in-outgoing progress packets.
        /// </summary>
        List<TiNetworkPacket> OutgoingPackets = new List<TiNetworkPacket>();


        /// <summary>
        /// 创建一个通用的 Socket wrapper.
        /// </summary>
        /// <param name="context"></param>
        public UdpSocketSenderWrapper(UnityEngine.Object context)
        {
            this.UnityObject = context;
            cancellation = new CancellationTokenSource();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //Begin send loop:
            StartSenderThread();
        }

        /// <summary>
        /// 创建一个通用的 Socket wrapper.
        /// </summary>
        /// <param name="context"></param>
        public UdpSocketSenderWrapper(UnityEngine.Object context, int sendBufferSize)
        {
            this.UnityObject = context;
            cancellation = new CancellationTokenSource();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SendBufferSize = sendBufferSize;
            StartSenderThread();
        }

        internal void StartSenderThread()
        {
            //Begin send loop:
            Task.Run(SendLoop);
        }

        ~UdpSocketSenderWrapper()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {

            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                try
                {
                    if (cancellation != null)
                    {
                        cancellation.Cancel();
                    }

                    if (socket != null)
                    {
                        socket.Close();
                    }

                    lock (mutex)
                    {
                        this.PendingPackets.Clear();
                        this.OutgoingPackets.Clear();
                    }

                }
                catch (Exception exc)
                {
                    Debug.LogException(exc, UnityObject);
                }
            }
            _disposed = true;
        }

        /// <summary>
        /// Sends a message.
        /// The method puts a outging message in the outgoing queue.
        /// The new message will be put to last item at the queue.
        /// </summary>
        public void Send(IPEndPoint endPoint, byte[] message, int offset, int messageLength)
        {
            var packet = TiNetworkPacketPool.GetNetworkPacket();
            if (packet.Data.Length < messageLength)
            {
                Debug.LogWarningFormat("Message length: {0} out of initialized buffer size : {1}, Now re-allocating buffer.", messageLength, packet.Data.Length);
                return;
            }
            packet.endPoint.Address = endPoint.Address;
            packet.endPoint.Port = endPoint.Port;
            packet.DataLength = messageLength;
            Array.ConstrainedCopy(message, offset, packet.Data, 0, messageLength);
            lock (mutex)
            {
                PendingPackets.Add(packet);
            }
        }

        /// <summary>
        /// Sends a message.
        /// The method puts a outging message in the outgoing queue.
        /// The new message will be put to last item at the queue.
        /// </summary>
        public void Send(TiNetworkPacket packet)
        {
            lock (mutex)
            {
                PendingPackets.Add(packet);
            }
        }

        /// <summary>
        /// Broadcast bytes to all LAN devices.
        /// </summary>
        /// <param name="packet"></param>
        public void Broadcast(TiNetworkPacket packet)
        {
            if (socket.EnableBroadcast == false)
            {
                socket.EnableBroadcast = true;
            }
            //对于 broadcast,直接发送而不是推入队列:
            try
            {
                int sent = socket.SendTo(packet.Data, 0, packet.DataLength, SocketFlags.None, packet.endPoint);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            packet.Return();
        }

        void SendLoop()
        {
            try
            {
                CancellationToken token = cancellation.Token;
                var pendingQueue = PendingPackets;
                //var outgoingQueue = OutgoingPackets;
                while (true)
                {
                    int pendingCount = 0;
                    lock (mutex)
                    {
                        pendingCount = pendingQueue.Count;
                    }
                    if (pendingCount == 0)
                    {
                        Thread.Sleep(1);
                    }
                    else
                    {
                        lock (mutex)
                        {
                            for (int i = 0; i < pendingQueue.Count; i++)
                            {
                                TiNetworkPacket packet = pendingQueue[i];
                                try
                                {
                                    if (packet.endPoint == null)
                                    {
                                        Debug.LogError("Packet endpoint == NULL");
                                    }
                                    else if (packet.DataLength == 0)
                                    {
                                        Debug.LogError("Packet data length == 0");
                                    }
                                    else
                                    {
                                        int sent = socket.SendTo(packet.Data, 0, packet.DataLength, SocketFlags.None, packet.endPoint);
                                        if (sent != packet.DataLength)
                                        {
                                            Debug.LogWarningFormat("Total sent : {0} != Data length: {1}", sent, packet.DataLength);
                                        }
                                        //Debug.LogFormat("UDP SendLoop() - Total sent byte: {0}/ data length: {1}", sent, packet.DataLength);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Debug.LogErrorFormat("Exception when sending to : {0}", packet.endPoint.ToString());
                                    Debug.LogException(e);
                                }
                                finally
                                {
                                    packet.Return();
                                }
                            }
                            pendingQueue.Clear();
                        }
                    }

                    //If the sender thread is aborted:
                    if (token.IsCancellationRequested)
                    {
                        throw new TaskCanceledException(kErrorMsg);
                    }
                }
                //Debug.Log("UDP socket disposed.");
            }
            catch (TaskCanceledException cancellationExc)
            {
                Debug.LogException(cancellationExc, this.UnityObject);
            }
            catch (System.Exception exc)
            {
                Debug.LogErrorFormat(this.UnityObject.name, "Error in SendLoop() of object: {0}", this.UnityObject);
                Debug.LogException(exc, this.UnityObject);
            }
            finally
            {
                lock (mutex)
                {
                    PendingPackets.Clear();//在发送线程退出的时候， 清理unreliable消息缓存池
                }
            }
        }
    }
}