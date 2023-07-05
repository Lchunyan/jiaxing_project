using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// Tcp Connection 
    /// </summary>
    public class TcpConnection
    {
        /// <summary>
        /// 在服务器上的Server，由服务器初始化ID,客户端没有服务器的ID
        /// </summary>
        public int ServerID { get; private set; }

        /// <summary>
        /// 链接的客户端的端口
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// 链接的客户端的ip
        /// </summary>
        public string ClientIP { get; private set; }

        /// <summary>
        /// 链接的客户端的TcpClient
        /// </summary>
        public Socket ConnectionSocket;

        public IPEndPoint RemoteIPEndPoint;


        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="Accept"></param>
        /// <param name="AcceptID"></param>
        public TcpConnection(Socket Accept, int AcceptID)
        {
            ServerID = AcceptID;
            ConnectionSocket = Accept;
            RemoteIPEndPoint = (IPEndPoint)Accept.RemoteEndPoint;
            ClientIP = RemoteIPEndPoint.Address.ToString();
            Port = RemoteIPEndPoint.Port;

        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="Accept"></param>
        /// <param name="AcceptID"></param>
        public TcpConnection(Socket Accept)
        {
            ConnectionSocket = Accept;
            RemoteIPEndPoint = (IPEndPoint)Accept.RemoteEndPoint;
            ClientIP = RemoteIPEndPoint.Address.ToString();
            Port = RemoteIPEndPoint.Port;

        }
    }

}