using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using I_TiNetNode = Ximmerse.XR.UnityNetworking.I_TiNetNode;
using TiNet = Ximmerse.XR.UnityNetworking.TiNet;
using TiNetMessage = Ximmerse.XR.UnityNetworking.TiNetMessage;
namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// Ti-Net utility.
    /// </summary>
    public static class TiNetUtility
    {
        /// <summary>
        /// Broadcast unreliable UDP msg to LAN
        /// </summary>
        /// <param name="msg"></param>
        public static void Broadcast(this TiNetMessage msg)
        {
            TiNet.Instance.Broadcast(msg);
        }

        /// <summary>
        /// Sends reliable message to all nodes in reliable channel
        /// </summary>
        /// <param name="msg"></param>
        public static void SendToAllReliable(this TiNetMessage msg)
        {
            TiNet.Instance.SendToAllReliable(msg);
        }

        /// <summary>
        /// Sends reliable message to the node in reliable channel
        /// </summary>
        /// <param name="msg"></param>
        public static void SendToReliable(this TiNetMessage msg, I_TiNetNode node)
        {
            TiNet.Instance.SendReliableTo(node, msg);
        }

        /// <summary>
        /// Sends reliable message to the node in reliable channel
        /// </summary>
        /// <param name="msg"></param>
        public static async Task SendToUnreliable(this TiNetMessage msg, I_TiNetNode node)
        {
            // await TiNet.Instance.SendUnreliableTo(node, msg);
            await TiNet.Instance.SendUnreliableTo(node, msg);
        }

        /// <summary>
        /// Sends reliable message to all nodes in reliable channel
        /// </summary>
        /// <param name="msg"></param>
        public static async Task SendToAllUnreliable(this TiNetMessage msg)
        {
            // await TiNet.Instance.SendToAllUnreliable(msg);
            await TiNet.Instance.SendToAllUnreliable(msg);
        }



        /// <summary>
        /// Gets a pooled TiNetMessage instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetMessage<T>() where T : TiNetMessage
        {
            return TiNetworkMessagePool.GetMessage<T>();
        }

        /// <summary>
        /// Gets a pooled TiNetMessage instance by the message code.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static TiNetMessage GetMessage(short messageCode)
        {
            var msgInstance = TiNetworkMessagePool.GetMessage(messageCode);
            return msgInstance;
        }

        /// <summary>
        /// Gets the message code of the TiNetMessage Type.
        /// </summary>
        /// <param name="messageType"></param>
        /// <returns></returns>
        public static bool GetMessageCode(System.Type messageType, out short MessageCode)
        {
            var attr = TiNetworkMessagePool.GetAttribute(messageType);
            if (attr != null)
            {
                MessageCode = attr.MessageCode;
                return true;
            }
            else
            {
                MessageCode = -1;
                Debug.LogWarningFormat("Unable to retrieve message code of type: {0}", messageType);
                return false;
            }
        }

        /// <summary>
        /// Return a TiNetMessage instance to pool.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static void ReturnMessage<T>(T msg) where T : TiNetMessage
        {
            if (GetMessageCode(msg.GetType(), out short messageCode))
            {
                TiNetworkMessagePool.ReturnMessage(messageCode, msg);
            }
        }
    }
}