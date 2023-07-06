using UnityEngine;
using System.Net;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// Net client.
    /// </summary>
    public abstract class NetClient : NetworkChannel
    {
        /// <summary>
        /// 当 当前客户端断开的事件
        /// </summary>
        public event Action OnClientDisconnected;

        /// <summary>
        /// 当当前客链接的服务器主动断开的事件
        /// </summary>
        public event Action OnClientConnected;

        /// <summary>
        /// Call when client state is change from connect to connecting / connecting to connected / connected to disconnect
        /// </summary>
        public event Action OnClientStateChanged;

    

        /// <summary>
        /// Is the client connected ?
        /// </summary>
        public abstract bool IsClientConnected
        {
            get;
        }

        /// <summary>
        /// Is the client connecting?
        /// </summary>
        public abstract bool IsClientConnecting
        {
            get;
        }

        /// <summary>
        /// Is the connect disconnected ?
        /// </summary>
        public abstract bool IsClientDisconnected
        {
            get;
        }

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

        [SerializeField]
        bool m_AutoReconnect = false;

        /// <summary>
        /// When being disconnected, should the client auto reconnect to previous address and port.
        /// </summary>
        public bool AutoReconnect
        {
            get
            {
                return m_AutoReconnect;
            }
            set
            {
                m_AutoReconnect = value;
            }
        }

        /// <summary>
        /// Start net client.
        /// </summary>
        public abstract Task<bool> StartClient();

        /// <summary>
        /// Stop net client.
        /// </summary>
        public abstract void StopClient();

        /// <summary>
        /// Sends message to remote.
        /// The client must be connected prior to sending.
        /// </summary>
        /// <param name="Message"></param>
        public abstract void Send(byte[] Message);

        /// <summary>
		/// Invoke on client disconnected.
		/// </summary>
		protected void InvokeOnClientDisconnect()
        {
            try
            {
                OnClientDisconnected?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            try
            {
                OnClientStateChanged?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }


        /// <summary>
        /// Invoke on client connected.
        /// </summary>
        protected void InvokeOnClientConnected()
        {
            try
            {
                OnClientConnected?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            try
            {
                OnClientStateChanged?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }


        /// <summary>
        /// Invoke on client start connecting.
        /// </summary>
        protected void InvokeOnClientConnecting()
        {
            try
            {
                OnClientStateChanged?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}