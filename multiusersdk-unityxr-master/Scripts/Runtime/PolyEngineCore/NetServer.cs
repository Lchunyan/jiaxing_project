using UnityEngine;
using System.Net;
using System;
using System.Collections.Generic;

namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// Net server.
    /// </summary>
    public abstract class NetServer : NetworkChannel
    {
        /// <summary>
        /// When a connection is disconnected.
        /// </summary>
        public event Action<I_Connection> OnServerDisconnect;

        /// <summary>
        /// When a new connection is connected.
        /// </summary>
        public event Action<I_Connection> OnServerConnected;

        [SerializeField]
        string m_ServerAddress = "127.0.0.1";

        /// <summary>
        /// Server address
        /// </summary>
        public string ServerAddress
        {
            get
            {
                return m_ServerAddress;
            }
            set
            {
                m_ServerAddress = value;
            }
        }

        [SerializeField]
        int m_ServerPort = 10032;

        /// <summary>
        /// Server port
        /// </summary>
        public int ServerPort
        {
            get
            {
                return m_ServerPort;
            }
            set
            {
                m_ServerPort = value;
            }
        }

        [SerializeField, MinValue(1)]
        int m_MaxConnection = 99;

        /// <summary>
        /// Max connection
        /// </summary>
        public int MaxConnection
        {
            get
            {
                return m_MaxConnection;
            }
            set
            {
                m_MaxConnection = value;
            }
        }


        /// <summary>
        /// Gets the alive connection count.
        /// </summary>
        public abstract int ConnectionCount
        {
            get;
        }

        /// <summary>
        /// Is server running?
        /// </summary>
        public abstract bool IsServerRunning
        {
            get;
        }


        /// <summary>
        /// is the server accepting new client.
        /// </summary>
        public abstract bool AcceptNewClient
        {
            get;
        }

        [SerializeField, Tooltip ("Auto start net server when Monobehaviour Start()")]
        bool m_AutoStart = false;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:UPDServer"/> auto start UDP server.
        /// </summary>
        /// <value><c>true</c> if auto start; otherwise, <c>false</c>.</value>
        public bool AutoStart { get { return m_AutoStart; } set { m_AutoStart = value; } }

        /// <summary>
        /// Starts server
        /// </summary>
        public abstract bool StartServer();

        /// <summary>
        /// Stops server.
        /// </summary>
        /// <returns></returns>
        public abstract void StopServer();

        /// <summary>
        /// Sends message to connection, starts from offset , in length.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="Message"></param>
        /// <param name="Offset"></param>
        /// <param name="Length"></param>
        public abstract void SendTo(I_Connection connection, byte[] Message, int Offset, int Length);

        /// <summary>
        /// Broadcast message to all connected clients.
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="length"></param>
        /// <param name="offset"></param>
        public abstract void BroadcastAsync(byte[] Message, int length, int offset);



        /// <summary>
        /// Invoke on server connected by new client
        /// </summary>
        protected void InvokeOnServerConnected(I_Connection conn)
        {
            try
            {
                OnServerConnected?.Invoke(conn);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }


        /// <summary>
        /// Invoke on server disconnect  client
        /// </summary>
        protected void InvokeOnServerDisconnect(I_Connection conn)
        {
            try
            {
                OnServerDisconnect?.Invoke(conn);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
