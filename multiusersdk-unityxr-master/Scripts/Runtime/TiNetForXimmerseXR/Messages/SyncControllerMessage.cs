using UnityEngine;
using Ximmerse.XR.UnityNetworking;

namespace Ximmerse.XR
{
    [Message(MessageCode.kSyncController)]
    public class SyncControllerMessage : TiNetMessage
    {
        public int OwnerID;

        public long TimeTicks;

        public bool LeftTracked, RightTracked;

        public Vector3 WorldPositionL, WorldPositionR;

        public Quaternion WorldRotationL, WorldRotationR;

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

        public override void OnDeserialize()
        {
            OwnerID = ReadInt();
            TimeTicks = ReadLong();
            LeftTracked = ReadBool();
            RightTracked = ReadBool();
            WorldPositionL = ReadVector3();
            WorldPositionR = ReadVector3();
            WorldRotationL = ReadQuaternion();
            WorldRotationR = ReadQuaternion();
        }

        public override void OnSerialize()
        {
            WriteInt(OwnerID);
            WriteLong(TimeTicks);
            WriteBool(LeftTracked);
            WriteBool(RightTracked);
            WriteVector3(WorldPositionL);
            WriteVector3(WorldPositionR);
            WriteQuaternion(WorldRotationL);
            WriteQuaternion(WorldRotationR);
        }
    }



    [Message(MessageCode.kSyncMainCamera)]
    public class SyncMainCameraMessage : TiNetMessage
    {
        public int OwnerID;

        public long TimeTicks;

        public bool Tracked;

        public Vector3 WorldPosition;

        public Quaternion WorldRotation;

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

        public override void OnDeserialize()
        {
            OwnerID = ReadInt();
            TimeTicks = ReadLong();
            Tracked = ReadBool();
            WorldPosition = ReadVector3();
            WorldRotation = ReadQuaternion();
        }

        public override void OnSerialize()
        {
            WriteInt(OwnerID);
            WriteLong(TimeTicks);
            WriteBool(Tracked);
            WriteVector3(WorldPosition);
            WriteQuaternion(WorldRotation);
        }
    }
}