using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// Bulk data transfer message.
    /// </summary>
    [MessageAttribute(messageCode: 0x0005)]
    public class TransferDataMessage : TiNetMessage
    {
        /// <summary>
        /// 文件/对象的名字。
        /// </summary>
        public string name;

        /// <summary>
        /// 包的序列id
        /// </summary>
        public ushort packetIndex;

        /// <summary>
        /// 包总数
        /// </summary>
        public ushort packetTotalCount;

        public static int kBufferSize
        {
            get
            {
                return TiNet.PreferredPacketBufferSize;
            }
        }


        public byte[] buffer = new byte[kBufferSize];

        /// <summary>
        /// The data buffer length;
        /// </summary>
        public int length;




        public TransferDataMessage()
        {
            buffer = new byte[kBufferSize];
        }

        public override void OnDeserialize()
        {
            name = ReadString();
            packetIndex = ReadUShort();
            packetTotalCount = ReadUShort();
            length = ReadBytes(buffer);

            
        }

        public override void OnSerialize()
        {
            WriteString(name);
            WriteUShort(packetIndex);
            WriteUShort(packetTotalCount);
            WriteBytes(buffer, 0, length);

        
        }

        protected override void OnReset()
        {
            name = string.Empty;
            length = 0;
            packetTotalCount = 0;
            packetIndex = 0;
            System.Array.Clear(buffer, 0, buffer.Length);

        }
    }
}