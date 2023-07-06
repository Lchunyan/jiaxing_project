using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading.Tasks;
using System.Threading;
using Ximmerse.XR.Asyncoroutine;

namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// TcpSocketWrapper_Daemon.cs
    /// SocketWrapper的守护线程.
    /// </summary>
    internal partial class TcpSocketWrapper
    {

        /// <summary>
        /// 发送ping消息（空消息串)的时间间隔.
        /// 默认1s.
        /// </summary>
        public static float kPingMessageInterval = 1;

        /// <summary>
        /// 记录最后一次auto ping的时间.
        /// </summary>
        DateTime lastAutoPingTime;

        void InitConnectionDaemon()
        {
            timeoutException = false;
            Task.Run(ConnectionDaemon);
        }

        /// <summary>
        /// 如果为 true, 代表已经超时.
        /// </summary>
        internal bool timeoutException
        {
            get; private set;
        }

        /// <summary>
        /// 是否使用心跳包 ?
        /// 对于tcp而言，使用心跳包是判断连接状态的方式。
        /// </summary>
        internal bool doHeartBeat = true;

        /// <summary>
        /// 连接状态检查线程.
        /// 在超时之后标记 timeException 字段.
        /// 确定在 socket 建立了连接之后再调用此方法.
        /// </summary>
        /// <returns></returns>
        private void ConnectionDaemon()
        {
            byte[] dummyMessage = kDummyMessage; //使用 Dummy message 做为心跳包包体报文.
            try
            {
                while (!timeoutException && !_disposed)
                {
                    if (m_client == null || m_client.Client == null)
                    {
                        throw new Exception("Tcp socket is disposed.");
                    }

                    //检查连接超时:
                    //在最多 DropConnectionTimeout 时间内(默认6s）, 如果没有收到任何消息， 则认为已经和remote断连.
                    var lastRecvTimeDiff = DateTime.Now - LastRecvTime;
                    //只有在 UsingRawSocket == false的时候才做心跳消息检查:
                    if (this.doHeartBeat && !UsingRawSocket && lastRecvTimeDiff.TotalSeconds >= DropConnectionTimeout)
                    {
                        //抛出超时异常
                        timeoutException = true;
                        // Debug.LogErrorFormat(UnityObject, "Timeout exception on socket: {0}, total time-span: {1}/{2}, last recv-time: {3}", this.RemoteAddress, lastRecvTimeDiff.TotalSeconds, DropConnectionTimeout, LastRecvTime.ToString());
                    }

                    //心跳包:
                    //如果距离上次发送时间 >= ping 时间间隔,则发送一条空消息.
                    if (doHeartBeat)
                    {
                        if ((DateTime.Now - lastAutoPingTime).TotalSeconds >= (kPingMessageInterval / 2) &&
                      (DateTime.Now - lastSentMsgTime).TotalSeconds >= kPingMessageInterval)
                        {
                            //如果使用原始socket， 不发送心跳包, 否则 auto-ping:
                            if (!this.UsingRawSocket)
                            {
                                PushOutgoingMessage(buffer: dummyMessage, attachMessageHeader: false);
                            }

                            //Debug.Log("Auto ping", UnityObject);
                            lastAutoPingTime = DateTime.Now;
                        }
                    }


                    //await new WaitForNextFrame();
                    //await Task.Delay(1);
                    Thread.Sleep(1);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e, UnityObject);
                timeoutException = true;
            }
        }
    }

}
