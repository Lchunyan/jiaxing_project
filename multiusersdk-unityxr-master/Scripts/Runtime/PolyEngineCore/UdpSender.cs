using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Text;


namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// The UDP sender - a simple UDP sender.
    /// </summary>
    public class UdpSender : MonoBehaviour
    {
        UdpSocketSenderWrapper sender;

        // Start is called before the first frame update
        void Start()
        {
            sender = new UdpSocketSenderWrapper(this);
        }

        private void OnDestroy()
        {
            sender.Dispose();
            sender = null;
        }

        /// <summary>
        /// Send text to specific ip and port
        /// </summary>
        /// <param name="message"></param>
        /// <param name="address"></param>
        /// <param name="port"></param>
        [InspectFunction]
        public void SendText(string address, int port, string message)
        {
            if (string.IsNullOrEmpty(message))
                return;
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            sender.Send(new IPEndPoint(IPAddress.Parse(address), port), buffer, 0, buffer.Length);
            //Debug.LogFormat("Send buffer: {0}", buffer.Length);
        }


        /// <summary>
        /// Send bytes to specific ip and port
        /// </summary>
        /// <param name="message"></param>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public void SendBytes(string address, int port, byte[] message, int offset, int length)
        {
            sender.Send(new IPEndPoint(IPAddress.Parse(address), port), message, 0, message.Length);
        }
    }
}