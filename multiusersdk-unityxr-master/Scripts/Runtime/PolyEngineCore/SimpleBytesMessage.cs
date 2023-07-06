using System;

namespace Ximmerse.XR.UnityNetworking
{
    [MessageAttribute(messageCode: 0x0003)]
    internal class SimpleBytesMessage : TiNetMessage
    {
        public byte[] buffer = new byte[1024];//default 1kb buffer

        /// <summary>
        /// The write length;
        /// </summary>
        public int length;

        public SimpleBytesMessage()
        {
        }

        public override void OnDeserialize()
        {
            length = this.ReadBytes(buffer);
        }

        public override void OnSerialize()
        {
            this.WriteBytes(buffer, 0, length);
        }
    }
}