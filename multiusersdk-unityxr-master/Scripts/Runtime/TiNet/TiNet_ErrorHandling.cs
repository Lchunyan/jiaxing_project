using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Linq;
using Ximmerse.XR.Asyncoroutine;


namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// TiNet error handling.
    /// </summary>
    public partial class TiNet : MonoBehaviour
    {
        /// <summary>
        /// Error handling on sent.
        /// 当发送消息出现socket error的情况下， 进入此方法。
        /// </summary>
        /// <param name="node"></param>
        /// <param name="err"></param>
        /// <param name="sentBytes"></param>
        private void OnSentError(TiNetNode node, SocketError err, int sentBytes)
        {
            if (err == SocketError.Success)
            {
                return;
            }
            Debug.LogErrorFormat("On Sent-Socket error : {0} , sent = {1}, node: {2}, set node to [NotConnected] status.", err, sentBytes, node.ToString());
            node.state = TiNetNodeState.NotConnected;
            //再 发出event:
            OnNodeDisconnected?.Invoke(node);
            switch (err)
            {
                case SocketError.Shutdown:
                case SocketError.TimedOut:
                case SocketError.ConnectionAborted:
                case SocketError.ConnectionRefused:
                case SocketError.ConnectionReset:
                case SocketError.NotConnected:
                case SocketError.Interrupted:
                    //todo : handle different socket error type
                    break;
            }

            //处理tcp component:
            if (node.tcpClientComponent)
            {
                node.tcpClientComponent.StopClient();
            }
        }
    }
}