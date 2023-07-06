namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// Band ip message.
    /// </summary>
    [MessageAttribute(messageCode: 0x0007)]
    internal class BandIPMessage : TiNetMessage
    {
        /// <summary>
        /// 是否Band
        /// </summary>
        public bool isBand;

        public override void OnDeserialize()
        {
            isBand = ReadBool();
        }

        public override void OnSerialize()
        {
            WriteBool(isBand);
        }
    }
}