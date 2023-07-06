using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;


namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// UdpSocketListenerWrapper.cs
    /// </summary>
    public class UdpSocketListenerWrapper : IDisposable
    {

        public string LocalAddress { get; private set; }
        /// <summary>
        /// Default receive buffer size: 512KB
        /// </summary>
        const int kBufferSize = 1024 * 512;

        byte[] buffer = null;

        bool _disposed = false;

        /// <summary>
        /// a unity engine object as a context to engine operation, like log, coroutine, etc.
        /// </summary>
        public UnityEngine.Object UnityObject
        {
            get; private set;
        }

        Socket socket = null;

        AsyncCallback asyncRecv = null;

        EndPoint receiveEndPoint = new IPEndPoint(IPAddress.Any, 0);

        List<TiNetworkPacket> receivePackets = new List<TiNetworkPacket>();

        /// <summary>
        /// If true, the message from local IP will be ignore.
        /// Default is true.
        /// </summary>
        public bool ignoreMessageFromLocalIP = true;

        /// <summary>
        /// Is the UDP socket bound to port ?
        /// </summary>
        public bool IsBound
        {
            get; private set;
        }

        IPAddress localIP;

        /// <summary>
        /// Threading mutex.
        /// </summary>
        System.Object mutex = new object();

        public UdpSocketListenerWrapper(UnityEngine.Object context)
        {
            this.UnityObject = context;
            asyncRecv = new AsyncCallback(this.OnAsyncReceive);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.ReceiveBufferSize = kBufferSize;
            buffer = new byte[kBufferSize];
        }

        /// <summary>
        /// Binds the UDP socket listener to port at local IP address.
        /// </summary>
        public bool BindToLocalAddress(int ListenerPort)
        {
            if (IsBound)
            {
                Debug.LogErrorFormat(this.UnityObject, "This UDP socket listener is already bound to address:", this.LocalAddress);
                return false;
            }
            try
            {
                //var host = Dns.GetHostEntry(Dns.GetHostName());
                //string localAddress = null;
                //foreach (var ip in host.AddressList)
                //{
                //    if (ip.AddressFamily == AddressFamily.InterNetwork)
                //    {
                //        localAddress = ip.ToString();
                //        break;
                //    }
                //}

                //if (string.IsNullOrEmpty(localAddress))
                //{
                //    Debug.LogErrorFormat(this.UnityObject, "Failed to get local address when binding port: {0}", ListenerPort);
                //    return false;
                //}

                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
                if (!PENetworkUtils.GetLocalIP(out localIP))
                {
                    Debug.LogError("Failed to obtain local IP, broadcaster can't work !");
                    return false;
                }
                LocalAddress = localIP.ToString();
                socket.Bind(new IPEndPoint(localIP, ListenerPort));
                IsBound = true;
                //在listener上启动的循环读输入字节的Daemon方法。
                socket.BeginReceiveFrom(buffer, 0, kBufferSize, SocketFlags.None, ref receiveEndPoint, asyncRecv, state: null);
                Debug.LogFormat(UnityObject, "UDP listener's been bind to address {0}:{1}", LocalAddress, ListenerPort);
                return true;
            }
            catch (System.Exception exc)
            {
                Debug.LogException(exc);
                IsBound = false;
                return false;
            }
        }

        /// <summary>
        /// Binds the UDP socket listener to LAN broadcasting address.
        /// </summary>
        /// <returns></returns>
        public bool BindToBroadcastAddress(int Port)
        {
            if (IsBound)
            {
                Debug.LogErrorFormat(this.UnityObject, "This UDP socket listener is already bound to address:", this.LocalAddress);
                return false;
            }
            try
            {
                if (!PENetworkUtils.GetLocalIP(out localIP))
                {
                    Debug.LogError("Failed to obtain local IP, broadcaster can't work !");
                    return false;
                }
                IPAddress any = IPAddress.Any;
                var broadcastEndPoint = new IPEndPoint(any, Port) as EndPoint;
                socket.Bind(broadcastEndPoint);
                //在listener上启动的循环读输入字节的Daemon方法。
                socket.BeginReceiveFrom(buffer, 0, kBufferSize, SocketFlags.None, ref receiveEndPoint, asyncRecv, state: null);
                IsBound = true;
                Debug.LogFormat(UnityObject, "UDP listener's been bind to address {0}", any.ToString());
                return true;
            }
            catch (Exception exc)
            {
                Debug.LogException(exc);
                IsBound = false;
                return false;
            }
        }

        /// <summary>
        /// 对一个已经绑定了的Socket wrapper解绑定
        /// </summary>
        /// <returns></returns>
        public void Unbind()
        {
            if (IsBound && socket != null)
            {
                try
                {
                    socket.Close();
                    socket.Dispose();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    IsBound = false;
                }
            }
        }

        ~UdpSocketListenerWrapper()
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
                    if (socket != null)
                    {
                        socket.Close();
                    }
                }
                catch (Exception exc)
                {
                    Debug.LogException(exc, UnityObject);
                }
            }
            _disposed = true;
        }

        private void OnAsyncReceive(IAsyncResult asyncResult)
        {
            int byteCount = -1;
            try
            {
                byteCount = socket.EndReceiveFrom(asyncResult, ref receiveEndPoint);
            }
            catch (ThreadAbortException abortExc) //在 dispose UDP thread wrapper 的时候触发此异常
            {
                Debug.LogFormat("UDP wrapper receive thread is aborted.  Quit receiving loop.");
                return;
            }
            catch (ObjectDisposedException disposeExc)//在 dispose UDP thread wrapper 的时候触发此异常
            {
                Debug.LogFormat("UDP wrapper socket is disposed. Quit receiving loop.");
                return;
            }

            IPEndPoint ipEndPoint = (IPEndPoint)receiveEndPoint;

            //Ignore broadcast message from local IP:
            if (ignoreMessageFromLocalIP && localIP != null && ipEndPoint.Address.Equals(this.localIP))
            {
                //在listener上启动的循环读输入字节的Daemon方法。
                socket.BeginReceiveFrom(buffer, 0, kBufferSize, SocketFlags.None, ref receiveEndPoint, asyncRecv, state: null);
                return;
            }

            //Debug.LogFormat("UDP listener read bytes: {0} from end point: {1} ", byteCount, receiveEndPoint.ToString());
            TiNetworkPacket packet = TiNetworkPacketPool.GetNetworkPacket();
            packet.endPoint.Port = ipEndPoint.Port;
            packet.endPoint.Address = ipEndPoint.Address;
            Array.ConstrainedCopy(buffer, 0, packet.Data, 0, byteCount);
            packet.DataLength = byteCount;
            lock (mutex)
            {
                receivePackets.Add(packet);
            }
            //在listener上启动的循环读输入字节的Daemon方法。
            socket.BeginReceiveFrom(buffer, 0, kBufferSize, SocketFlags.None, ref receiveEndPoint, asyncRecv, state: null);
        }

        /// <summary>
        /// Pop received packets to list.
        /// Internal packet list will be clear after called.
        /// </summary>
        /// <param name="packets"></param>
        /// <returns></returns>
        public int PopReceivePacket(List<TiNetworkPacket> packets)
        {
            int count = 0;
            lock (mutex)
            {
                count = this.receivePackets.Count;
                if (count > 0)
                {
                    packets.AddRange(this.receivePackets);
                    receivePackets.Clear();
                }
            }
            return count;
        }
    }
}