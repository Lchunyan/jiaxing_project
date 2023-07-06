namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// Simple text message.
    /// Use this message for lightweight text message transmission.
    /// </summary>
    [MessageAttribute(messageCode: 0x0002)]
    internal class SimpleTextMessage : TiNetMessage
    {
        public string Text;

        public SimpleTextMessage()
        {
        }

        public override void OnDeserialize()
        {
            Text = this.ReadString();
        }

        public override void OnSerialize()
        {
            this.WriteString(Text);
        }
    }
}