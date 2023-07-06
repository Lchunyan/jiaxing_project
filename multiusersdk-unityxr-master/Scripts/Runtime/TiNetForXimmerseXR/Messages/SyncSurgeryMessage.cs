using UnityEngine;
using Ximmerse.XR.UnityNetworking;

namespace Ximmerse.XR
{
    [Message(MessageCode.kSyncSurgery)]
    public class SyncSurgeryMessage : TiNetMessage
    {
        public int SurgeryID;

        public long TimeTicks;

        public float InitTime;

        public bool IsDirectorPlaying;

        /// <summary>
        /// The datetime from TimeTicks
        /// </summary>
        public System.DateTime time
        {
            get
            {
                return new System.DateTime(TimeTicks);
            }
            set
            {
                TimeTicks = value.Ticks;
            }
        }

        /// <summary>
        /// Read data from network.
        /// </summary>
        public override void OnDeserialize()
        {
            SurgeryID = ReadInt();
            TimeTicks = ReadLong();
            InitTime = ReadFloat();
            IsDirectorPlaying = ReadBool();
        }

        /// <summary>
        /// Writes data to network.
        /// </summary>
        public override void OnSerialize()
        {
            WriteInt(SurgeryID);
            WriteLong(TimeTicks);
            WriteFloat(InitTime);
            WriteBool(IsDirectorPlaying);
        }
    }
}