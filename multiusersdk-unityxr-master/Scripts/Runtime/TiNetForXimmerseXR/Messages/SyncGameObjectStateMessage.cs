using Ximmerse.XR.UnityNetworking;

namespace Ximmerse.XR
{
    [Message(MessageCode.kSyncGameObjectState)]
    public class SyncGameObjectStateMessage : TiNetMessage
    {

        public int NetworkID;

        public bool ActiveSelf;

        public long TimeTicks;

        public override void OnDeserialize()
        {
            NetworkID = ReadInt();
            ActiveSelf = ReadBool();
            TimeTicks = ReadLong();
        }

        public override void OnSerialize()
        {
            WriteInt(NetworkID);
            WriteBool(ActiveSelf);
            WriteLong(TimeTicks);
        }
    }
}