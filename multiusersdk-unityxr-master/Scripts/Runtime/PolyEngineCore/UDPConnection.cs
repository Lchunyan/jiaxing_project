using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;

namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// UDP connection wrapper.
    /// </summary>
    internal class UDPConnection : I_Connection
    {
        /// <summary>
        /// The ip endpoint of the peer
        /// </summary>
        public IPEndPoint ipEndpoint
        {
            get; private set;
        }

        /// <summary>
        /// The async event arguments.
        /// </summary>
        public SocketAsyncEventArgs asyncEventArgs
        {
            get; private set;
        }
        /// <summary>
        /// Gets the connection time.
        /// </summary>
        /// <value>The connection time.</value>
        public float ConnectionTime
        {
            get; private set;
        }
        /// <summary>
        /// Is any async-sending action currently pend ?
        /// </summary>
        /// <value><c>true</c> if is pending action; otherwise, <c>false</c>.</value>
        public bool IsPendingAsyncSend
        {
            get;private set;
        }
        public Socket socket
        {
            get; private set;
        }

        /// <summary>
        /// Gets the last async send result.
        /// </summary>
        /// <value>The last async send result.</value>
        public System.IAsyncResult lastAsyncSendResult
        {
            get; private set;
        }


        /// <summary>
        /// Unity object context.
        /// </summary>
        public UnityEngine.Object UnityObject
        {
            get; internal set;
        }

        public bool IsTcpConnection => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:PolyEngine.UnityNetworking.UDPConnection"/> class.
        /// </summary>
        /// <param name="remoteEndPoint">Remote end point.</param>
        /// <param name="connectionTime">Connection time.</param>
        public UDPConnection(Socket socket, IPEndPoint remoteEndPoint, float connectionTime)
        {
            this.socket = socket;
            ConnectionTime = connectionTime;
            ipEndpoint = remoteEndPoint;
            asyncEventArgs = new SocketAsyncEventArgs();
            asyncEventArgs.RemoteEndPoint = ipEndpoint;
            asyncEventArgs.Completed += AsyncEventArgs_Completed;;
        }

        public void AsyncSendToRemoteEndPoint (byte[] Buffer, int Offset, int Length)
        {
            System.IAsyncResult iAsyncResult = socket.BeginSendTo(Buffer, Offset, Length, SocketFlags.None, ipEndpoint, AsyncSendCallback, socket);
            if(lastAsyncSendResult != null)
            {
                Debug.LogWarning("Overriding an async-send operation !");
            }
            lastAsyncSendResult = iAsyncResult;
        }

        void AsyncSendCallback (System.IAsyncResult asyncResult)
        {
            //Socket socket = (Socket)asyncResult.AsyncState;
            socket.EndSendTo(asyncResult);
            lastAsyncSendResult = null;
        }


        /// <summary>
        /// On socket async operation complete.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void AsyncEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            //Debug.LogFormat("Action: {0} complete, total bytes transferred : {1}", e.LastOperation, e.BytesTransferred);
        }

    }

}