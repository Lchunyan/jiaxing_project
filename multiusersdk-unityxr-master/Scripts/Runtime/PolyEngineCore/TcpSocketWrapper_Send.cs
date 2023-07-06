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
    /// TcpSocketWrapper_Send.cs - handle send outgoing packet.
    /// </summary>
    internal partial class TcpSocketWrapper
    {

        /// <summary>
        /// Queue for caching outgoing packets.
        /// </summary>
        internal List<TcpDataPacket> oPackets = new List<TcpDataPacket>();

        /// <summary>
        /// Queue for pushing outgoing packets into oPackets queue ,for thread safety.
        /// </summary>
        internal List<TcpDataPacket> oPacketTemp = new List<TcpDataPacket>();

        /// <summary>
        /// Queue for outgoing network packets.
        /// </summary>
        List<TiNetworkPacket> oTiNetworkPackets = new List<TiNetworkPacket>();

        const float kDefaultSendTimeout = 3;

        const int kOutgoingMessageBuffer = 1024 * 500;//500KB for outgoing buffer

        /// <summary>
        /// The previous send msg time.
        /// </summary>
        DateTime lastSentMsgTime;

        byte[] OutgoingBuffer;

        /// <summary>
        /// The outgoing buffer size.
        /// </summary>
        public int BufferSize_Outgoing
        {
            get; private set;
        }

        /// <summary>
        /// Outgoing message's max length.
        /// 能容纳的最大消息长度。
        /// </summary>
        public int MessageMaxSize_Outgoing
        {
            get
            {
                //总长度 - (消息头 和 4位Int32消息长度数据)。
                return BufferSize_Outgoing - kMessageHeader.Length - 4;
            }
        }

        /// <summary>
        /// The outgoing socket exception.
        /// </summary>
        public Exception socketOutgoingException
        {
            get; private set;
        }

        /// <summary>
        /// 初始化发送部分字段。
        /// </summary>
        internal void InitOutgoing(int OutgoingBufferSize = kOutgoingMessageBuffer, float _SendTimeout = kDefaultSendTimeout)
        {
        }

        /// <summary>
        /// 推入外发消息队列 (Text).
        /// </summary>
        internal void PushOutgoingMessage(string text)
        {
            if (m_client != null && m_client.Client != null)
            {
                byte[] textBuffer = System.Text.Encoding.UTF8.GetBytes(text);
                PushOutgoingMessage(textBuffer, true);
            }
        }

        /// <summary>
        /// 将 Buffer 数组通过socket发送。
        /// 此方法允许 Buffer 为空， 如果 buffer 为空代表Ping消息，最终会发送一段 MessageHeader + [DataLength] 长度的消息.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="attachMessageHeader">是否在消息前添加消息头。</param>
        internal void PushOutgoingMessage(byte[] buffer, bool attachMessageHeader = true)
        {
            if (m_client != null && m_client.Client != null)
            {
                if (attachMessageHeader)
                {
                    byte[] outBuffer = new byte[kMessageHeader.Length + 4 + buffer.Length];//message header 长度 + 消息长度 + buffer的长度
                    Array.ConstrainedCopy(kMessageHeader, 0, outBuffer, 0, kMessageHeader.Length);
                    PEUtils.WriteInt(buffer.Length, outBuffer, kMessageHeader.Length);
                    Array.ConstrainedCopy(buffer, 0, outBuffer, kMessageHeader.Length + 4, buffer.Length);
                    SocketLLMgr.PushTcpSendRequest(m_client, outBuffer, 0, outBuffer.Length, OnSent, userData: null);
                }
                else
                {
                    SocketLLMgr.PushTcpSendRequest(m_client, buffer, 0, buffer.Length, OnSent, userData: null);
                }
                lastSentMsgTime = DateTime.Now;//记录最后一次发送时间
            }
        }


        /// <summary>
        /// 推入外发消息队列.
        /// </summary>
        internal void PushOutgoingMessage(byte[] buffer, int offset, int length, bool attachMessageHeader = true)
        {
            if (m_client != null && m_client.Client != null)
            {
                if (attachMessageHeader)
                {
                    byte[] outBuffer = new byte[kMessageHeader.Length + 4 + length];//message header 长度 + 消息长度 + buffer的长度
                    Array.ConstrainedCopy(kMessageHeader, 0, outBuffer, 0, kMessageHeader.Length);
                    PEUtils.WriteInt(length, outBuffer, kMessageHeader.Length);
                    Array.ConstrainedCopy(buffer, offset, outBuffer, kMessageHeader.Length + 4, length);
                    SocketLLMgr.PushTcpSendRequest(m_client, outBuffer, 0, outBuffer.Length, OnSent, userData: null);
                }
                else
                {
                    SocketLLMgr.PushTcpSendRequest(m_client, buffer, 0, length, OnSent, userData: null);
                }
                lastSentMsgTime = DateTime.Now;//记录最后一次发送时间
            }
        }

        /// <summary>
        /// Static 版本的 PushOutgoingMessage
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="attachMessageHeader"></param>
        /// <param name="callback"></param>
        static internal void sPushOutgoingMessage(Socket socket, byte[] buffer, int offset, int length,
            bool attachMessageHeader = true, Action<object, SocketError, int> callback = null, object UserData = null)
        {
            if (socket != null)
            {
                if (attachMessageHeader)
                {
                    byte[] outBuffer = new byte[kMessageHeader.Length + 4 + length];//message header 长度 + 消息长度 + buffer的长度
                    Array.ConstrainedCopy(kMessageHeader, 0, outBuffer, 0, kMessageHeader.Length);
                    PEUtils.WriteInt(length, outBuffer, kMessageHeader.Length);
                    Array.ConstrainedCopy(buffer, offset, outBuffer, kMessageHeader.Length + 4, length);
                    SocketLLMgr.PushTcpSendRequest(socket, outBuffer, 0, outBuffer.Length, callback, UserData);
                }
                else
                {
                    SocketLLMgr.PushTcpSendRequest(socket, buffer, 0, length, callback, UserData);
                }
            }
        }

        /// <summary>
        /// 推入 Tinet Data Packet 到消息队列。
        /// </summary>
        /// <param name="tinetpacket"></param>
        internal void PushOutgoingTinetPacket(TiNetworkPacket tinetpacket)
        {
            if (m_client != null && m_client.Client != null)
            {
                PushOutgoingMessage(tinetpacket.Data, 0, tinetpacket.DataLength);
            }
        }


        /// <summary>
        /// 发送 TCP 消息的回调。
        /// </summary>
        /// <param name="socketError"></param>
        /// <param name="sentSize"></param>
        private void OnSent(System.Object userData, SocketError socketError, int sentSize)
        {
            if (socketError != SocketError.Success)
            {
                Debug.LogFormat("Socket sent error code: {0}, sent bytes = {1}, server = {2}:{3}",
                    socketError, sentSize, this.RemoteAddress, this.RemotePort);
            }
            //如果 sent error == shutdown , 连接已经中断。
            if (socketError == SocketError.Shutdown)
            {
                Debug.LogFormat("TCP client to {0}:{1} is stopped on socket shutdown.", this.RemoteAddress, this.RemotePort);
                this.Dispose();
            }
        }


    }
}
