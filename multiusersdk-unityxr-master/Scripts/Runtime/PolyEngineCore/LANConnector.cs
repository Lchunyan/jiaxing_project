using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using Ximmerse.XR.Asyncoroutine;
namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// LAN connector : auto connects udp client to udp server, by broadcaster content.
    /// </summary>
    public class LANConnector : MonoBehaviour
    {
        /// <summary>
        /// Restart target IP.
        /// </summary>
        public enum RestartTargetIP
        {
            /// <summary>
            /// Use random IP when reconnection
            /// </summary>
            Random = 0,

            /// <summary>
            /// Wait for the previous connected IP for [WaitIPTargetTime]
            /// </summary>
            WaitForPreviousIP,

            /// <summary>
            /// Skip the previous connected IP for [WaitIPTargetTime]
            /// </summary>
            SkipPreiousIP,

            /// <summary>
            /// Wait for the specific IP for [WaitIPTargetTime]
            /// </summary>
            WaitForSpecificIP,
        }

        /// <summary>
        /// The net client to auto connect.
        /// </summary>
        public NetClient client;

        public BroadcastListener listener;

        public bool AutoStart = true;

        /// <summary>
        /// if true, suppress reconnection.
        /// </summary>
        bool SuppressReconnect;

        /// <summary>
        /// if true, auto reconnect when disconnected.
        /// </summary>
        [Tooltip("if true, auto reconnect when disconnected.")]
        public bool AutoReconnect = true;

        [Tooltip("Reconnect IP config")]
        public RestartTargetIP m_reconnectIP = RestartTargetIP.Random;

        /// <summary>
        /// Config which IP to choose when reconnect
        /// </summary>
        public RestartTargetIP ReconnectIPConfig
        {
            get => m_reconnectIP;
            set => m_reconnectIP = value;
        }

        [SerializeField]
        float m_WaitIPTargetTime = 5;

        /// <summary>
        /// Wait for target IP time when reconnection.
        /// </summary>
        public float WaitIPTargetTime { get => m_WaitIPTargetTime; set => m_WaitIPTargetTime = value; }

        /// <summary>
        /// Is the connector connecting to server ?
        /// </summary>
        bool isConnecting = false;

        /// <summary>
        /// The previous connected IP.
        /// </summary>
        string previousConnectIP = string.Empty;

        /// <summary>
        /// The specific IP.
        /// </summary>
        public string specificIP = string.Empty;

        /// <summary>
        /// Start connection time.
        /// </summary>
        long startsConnectionTime = 0;

        // Start is called before the first frame update
        void Start()
        {
            if (AutoStart)
            {
                StartAutoConnection();
                client.OnClientStateChanged += onClientStateChanged;
            }
        }

        /// <summary>
        /// Starts the auto connection.
        /// 调用此方法启动自动连接功能.
        /// </summary>
        [InspectFunction]
        public void StartAutoConnection()
        {
            if (!isConnecting)
            {
                isConnecting = true;
                if (!listener.IsListening)
                {
                    listener.StartListening();
                }
                listener.OnListenMessage += Listener_OnListenMessage;
                startsConnectionTime = DateTime.Now.Ticks;
            }
            else
            {
                Debug.Log("LAN Connector is already working to connect.");
            }
        }

        /// <summary>
        /// Stops connector 。
        /// 停止自动连接尝试.
        /// </summary>
        [InspectFunction]
        public void Stop(bool successfullyConnect)
        {
            isConnecting = false;
            if (listener.IsListening)
            {
                listener.StopListening();
            }
            listener.OnListenMessage -= Listener_OnListenMessage;
        }

        void Listener_OnListenMessage(string IP, byte[] Content, int ContentLength)
        {
            try
            {
                bool _acceptThisIP = false;
                bool isFirstConnection = string.IsNullOrEmpty(previousConnectIP);//is the first connection time ?
                //if previous ip is null:
                if (string.IsNullOrEmpty(previousConnectIP) && this.m_reconnectIP != RestartTargetIP.WaitForSpecificIP)
                {
                    _acceptThisIP = true;
                }

                //if (string.IsNullOrEmpty(previousConnectIP) && this.m_reconnectIP != RestartTargetIP.WaitForSpecificIP)
                //{
                //    _acceptThisIP = true;
                //}
                //else
                //{
                else
                {
                    switch (this.m_reconnectIP)
                    {
                        case RestartTargetIP.Random:
                            _acceptThisIP = true;
                            break;
                        case RestartTargetIP.WaitForPreviousIP:
                            //如果之前的 IP 是空，说明第一次连接
                            if (isFirstConnection || IP == previousConnectIP)
                            {
                                _acceptThisIP = true;
                            }
                            else
                            {
                                double waitTime = new TimeSpan(DateTime.Now.Ticks - startsConnectionTime).TotalSeconds;
                                if (waitTime >= this.m_WaitIPTargetTime)
                                {
                                    _acceptThisIP = true;
                                }
                                else
                                {
                                    Debug.Log("Not yet - still waiting for previous IP");
                                }
                            }
                            break;
                        case RestartTargetIP.SkipPreiousIP:
                            if (isFirstConnection || IP != previousConnectIP)
                            {
                                _acceptThisIP = true;
                            }
                            else
                            {
                                double waitTime = new TimeSpan(DateTime.Now.Ticks - startsConnectionTime).TotalSeconds;
                                if (waitTime >= this.m_WaitIPTargetTime)
                                {
                                    _acceptThisIP = true;
                                }
                                else
                                {
                                    Debug.Log("Not yet - still waiting for non-previouss other IP");
                                }
                            }
                            break;
                        case RestartTargetIP.WaitForSpecificIP:
                            if (IP == specificIP)
                            {
                                _acceptThisIP = true;
                            }
                            else
                            {
                                Debug.LogFormat ("Not yet - still waiting for specific IP: {0}", specificIP);
                            }
                            break;
                    }
                }

                if (_acceptThisIP)
                {
                    //在接受到以后，停止
                    //int port = PEUtils.ReadInt(Content);
                    ParseRecvBuffer(Content, out int port);
                    ConnectTo(IP, port);
                }

            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Parse the received broadcaster buffer content.
        /// Output broadcast Port
        /// </summary>
        protected virtual void ParseRecvBuffer(byte[] Content, out int Port)
        {
            int port = PEUtils.ReadInt(Content);
            Port = port;
        }

        void onClientStateChanged()
        {
            if (this.client.IsClientDisconnected && AutoReconnect && SuppressReconnect == false)
            {
                //invoke listener re-listening:
                Invoke("StartAutoConnection", 0.5f);
            }
        }

        private void ConnectTo(string IP, int Port)
        {
            Debug.LogFormat("[LANConnector] Connects to : {0}:{1}", IP, Port);
            //在接受到以后，停止
            client.ServerAddress = IP;
            client.ServerPort = Port;
            previousConnectIP = IP;
            client.StartClient();
            Stop(true);
        }

        /// <summary>
        /// Restarts the whole connection process.
        /// 重新开启连接， 可以在参数中指定重新连接的条件。
        /// </summary>
        [InspectFunction]
        public async Task Restart()
        {
            if (!isConnecting)
            {
                SuppressReconnect = true;
                //1. Dispose client socket.
                bool autoReconnectOld = client.AutoReconnect;
                client.AutoReconnect = false;//stops UDP client auto reconnection.
                this.client.StopClient();
                Stop(false);//stopps listener.
                //wait for disconnected
                while (!client.IsClientDisconnected)
                {
                    await new WaitForNextFrame();
                }
                StartAutoConnection();

                //wait for connection complete :
                while (!client.IsClientConnected)
                {
                    await new WaitForNextFrame();
                }
                SuppressReconnect = false;
                client.AutoReconnect = autoReconnectOld;//revert auto reconnect flag
                Debug.Log("Restart and reconnected complete.");
            }
        }
    }
}