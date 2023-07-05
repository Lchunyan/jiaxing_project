using Ximmerse.XR.UnityNetworking;
using UnityEngine;
using TiNetMessgae = Ximmerse.XR.UnityNetworking.TiNetMessage;

namespace Ximmerse.XR
{
    [Message(MessageCode.kSyncTransform)]
    public class SyncTransformMessage : Ximmerse.XR.UnityNetworking.TiNetMessage
    {
        public int OwnerID;

        public int NetworkID;

        public long TimeTicks;

        public Vector3 WorldPosition;

        public Quaternion WorldRotation;

        public Vector3 Scale;

        public bool IsFirstMessage;


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
        /// 从网络读取数据
        /// </summary>
        public override void OnDeserialize()
        {
            OwnerID = ReadInt();
            NetworkID = ReadInt();
            TimeTicks = ReadLong();
            WorldPosition = ReadVector3();
            WorldRotation = ReadQuaternion();
            Scale = ReadVector3();
            IsFirstMessage = ReadBool();

        }

        /// <summary>
        /// 向网络写出数据
        /// </summary>
        public override void OnSerialize()
        {
            WriteInt(OwnerID);
            WriteInt(NetworkID);
            WriteLong(TimeTicks);
            WriteVector3(WorldPosition);
            WriteQuaternion(WorldRotation);
            WriteVector3(Scale);
            WriteBool(IsFirstMessage);

        }
    }
}