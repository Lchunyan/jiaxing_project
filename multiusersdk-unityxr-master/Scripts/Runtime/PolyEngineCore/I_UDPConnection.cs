using System.Net;
using System.Net.Sockets;

namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// Interface : UDP Connection.
    /// </summary>
    public interface I_Connection
    {
        /// <summary>
        /// The remote endpoint
        /// </summary>
        /// <value>The ip endpoint.</value>
        IPEndPoint ipEndpoint { get; }

        /// <summary>
        /// Gets the connection setup time.
        /// </summary>
        /// <value>The connection time.</value>
        float ConnectionTime { get; }

        /// <summary>
        /// Unity object context.
        /// </summary>
        UnityEngine.Object UnityObject
        {
            get;
        }

        /// <summary>
        /// If true, this is tcp connection , else it's udp connection
        /// </summary>
        bool IsTcpConnection
        {
            get;
        }

        /// <summary>
        /// The socket.
        /// </summary>
        Socket socket
        {
            get;
        }
    }
}