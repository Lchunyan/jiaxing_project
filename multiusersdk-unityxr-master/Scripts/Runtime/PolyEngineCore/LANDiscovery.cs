using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// LAN discovery : broadcasting UDP server's port inside LAN
    /// </summary>
    public class LANDiscovery : MonoBehaviour
    {
        public NetServer server;

        public Broadcaster broadcaster;

        public bool AutoStart = true;

        public bool AutoStop = false;

        public int LimitConnectionCount = 1;

        public bool AutoRestart = true;

        public enum State
        {
            NotStarted,

            Started,
        }

        public State state
        {
            get;private set;
        }

        // Start is called before the first frame update
        void Start()
        {
            if (AutoStart)
            {
                StartAutoConnection();
            }
        }

        private void Update()
        {
            if (state == State.NotStarted && this.AutoStart && server.ConnectionCount < LimitConnectionCount && broadcaster.IsBroadcasting == false)
            {
                broadcaster.StartBroadcasting();
            }
        }

        private void OnDestroy()
        {
            server.OnServerConnected -= Server_OnConnected;
        }

        [InspectFunction]
        public void StartAutoConnection()
        {
            if (!broadcaster.IsBroadcasting)
            {
                broadcaster.Looping = true;
                PrepareBroadcasterBuffer();
                broadcaster.StartBroadcasting();

                server.OnServerConnected += Server_OnConnected;
                if(!server.IsServerRunning)
                {
                    server.StartServer();
                }
                state = State.Started;
                Debug.LogFormat("LANDiscovery - start auto connection @port: {0}", server.ServerPort);
            }
            else
            {
                Debug.LogFormat("LANDiscovery - Already running at port : {0}", server.ServerPort);
            }
        }

        void Server_OnConnected(I_Connection obj)
        {
            if (AutoStop && server.ConnectionCount >= LimitConnectionCount)
            {
                Stop();
            }
        }

        /// <summary>
        /// Sets the broadcaster's buffer.
        /// </summary>
        protected virtual void PrepareBroadcasterBuffer()
        {
            broadcaster.Buffer = new byte[4];
            PEUtils.WriteInt(server.ServerPort, broadcaster.Buffer);
        }


        [InspectFunction]
        public void Stop()
        {
            if (broadcaster.IsBroadcasting)
            {
                broadcaster.StopBroadcasting();
            }
            state = State.NotStarted;
        }



    }
}