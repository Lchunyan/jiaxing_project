using Ximmerse.XR.UnityNetworking;
using UnityEngine.Playables;
namespace Ximmerse.XR
{
    [Message(MessageCode.kSyncDirector)]
    public class SyncDirectorMessage : TiNetMessage
    {
        /// <summary>
        /// The director owner ID.
        /// </summary>
        public int NetworkID;

        /// <summary>
        /// The director time.
        /// </summary>
        public float DirectorTime;

        /// <summary>
        /// The play state of the director.
        /// </summary>
        public PlayState State;

        /// <summary>
        /// The real time of the director's machine.
        /// </summary>
        public long SystemTime;

        public override void OnDeserialize()
        {
            NetworkID = ReadInt();
            DirectorTime = ReadFloat();
            State = (PlayState)ReadByte();
            SystemTime = ReadLong();
        }

        public override void OnSerialize()
        {
            WriteInt(NetworkID);
            WriteFloat(DirectorTime);
            WriteByte((byte)State);
            WriteLong(SystemTime);
        }
    }
}