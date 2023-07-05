using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading.Tasks;
using System.Threading;
using Ximmerse.XR.Asyncoroutine;

namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// Tcp socket wrapper for operating on socket stream.
    /// </summary>
    internal partial class TcpSocketWrapper : IDisposable, I_Connection
    {
        /// <summary>
        /// ID
        /// </summary>
        public int ID
        {
            get; private set;
        }

        TcpClient m_client;

        /// <summary>
        /// Gets the wrapped tcp client.
        /// </summary>
        public TcpClient Client
        {
            get
            {
                return m_client;
            }
        }

        /// <summary>
        /// TcpClient 的远端地址
        /// </summary>
        public string RemoteAddress
        {
            get; private set;
        }

        /// <summary>
        /// tcp client 的远端端口.
        /// </summary>
        public int RemotePort
        {
            get; private set;
        }

        public IPEndPoint ipEndpoint
        {
            get;private set;
        }

        /// <summary>
        /// 建立连接的Unity时间
        /// </summary>
        public float ConnectionTime
        {
            get; private set;
        }

        /// <summary>
        /// 通信包的消息头.
        /// </summary>
        public static byte[] kMessageHeader = {
              0x2C, 0x7A, 0x99, 0x1B, 0x9F,
        };

        /// <summary>
        /// Dummy message for heart beat ping
        /// </summary>
        public static byte[] kDummyMessage = {
              0x55, 0x8B, 0x70, 0x21, 0xFF,
        };

        bool _disposed;

        /// <summary>
        /// 超时断连时间 - 默认6秒。
        /// 在这个时间内，如没有收到任何信息， 则连接自动断开.
        /// </summary>
        public float DropConnectionTimeout = 9;

        /// <summary>
        /// a unity engine object as a context to engine operation, like log, coroutine, etc.
        /// </summary>
        public UnityEngine.Object UnityObject
        {
            get; private set;
        }

        /// <summary>
        /// 使用原始 Socket 通信， 不会附加 MessageHeader， 不会使用结构化的通信方式。
        /// </summary>
        public bool UsingRawSocket = false;

        /// <summary>
        /// Is tcp connection
        /// </summary>
        public bool IsTcpConnection => true;

        public Socket socket
        {
            get => m_client != null ? m_client.Client : null;
        }

        /// <summary>
        /// Constructor - make sure the client is connected when you construct it !
        /// Default outgoing buffer sizee = 500kB
        /// </summary>
        /// <param name="client"></param>
        public TcpSocketWrapper(TcpClient client, int ID, float unityTime, UnityEngine.Object unityContext, bool heartBeat = true)
        {
            m_client = client;
            this.ID = ID;
            IPEndPoint remoteIP = (IPEndPoint)client.Client.RemoteEndPoint;
            RemoteAddress = remoteIP.Address.ToString();
            RemotePort = remoteIP.Port;
            ConnectionTime = unityTime;
            UnityObject = unityContext;
            this.ipEndpoint = remoteIP;
            LastRecvTime = DateTime.Now;
            this.doHeartBeat = heartBeat;

            InitOutgoing();
            InitRecv();
            InitConnectionDaemon();
        }

        /// <summary>
        /// Constructor - make sure the client is connected when you construct it !
        /// Customize outgoing buffer sizee 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="OutgoingBufferSize"></param>
        public TcpSocketWrapper(TcpClient client, int ID, float unityTime, int OutgoingBufferSize, int SendTime, int IncomingBufferSize, UnityEngine.Object unityContext)
        {
            m_client = client;
            this.ID = ID;
            IPEndPoint remoteIP = (IPEndPoint)client.Client.RemoteEndPoint;
            RemoteAddress = remoteIP.Address.ToString();
            RemotePort = remoteIP.Port;
            ConnectionTime = unityTime;
            UnityObject = unityContext;
            this.ipEndpoint = remoteIP;

            //Debug.LogFormat("Address : {0}, port: {1}", RemoteAddress, RemotePort);
            InitOutgoing(OutgoingBufferSize, SendTime);
            InitRecv(IncomingBufferSize);
            InitConnectionDaemon();
        }

        ~TcpSocketWrapper()
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
                    //Cancel outgoing thread:
                    if (m_client != null)
                      this.m_client.Dispose();
                }
                catch (Exception exc)
                {
                    Debug.LogException(exc, UnityObject);
                }
            }
            _disposed = true;
        }
    }
}
