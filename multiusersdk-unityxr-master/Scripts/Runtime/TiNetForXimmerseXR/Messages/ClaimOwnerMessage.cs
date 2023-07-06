using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ximmerse.XR.UnityNetworking;

namespace Ximmerse.XR
{
    [Message(MessageCode.kClaimOwner)]
    public class ClaimOwnerMessage : TiNetMessage
    {
        /// <summary>
        /// The owner Ti-Node ID
        /// </summary>
        public int OwnerID;

        /// <summary>
        /// The Network scene ID.
        /// </summary>
        public int NetworkID;

        /// <summary>
        /// 如果是true，为设置owner，
        /// 如果为false， 为反设置owner.
        /// </summary>
        public bool ClaimOwner;

        /// <summary>
        /// 当 Cliam owner = true的时候， 记录宣布所有权的时间戳。
        /// 这项设置用于解决当两个用户同时刻宣布主权的冲突问题。
        /// </summary>
        public long ClaimOwnerTime;

        public override void OnDeserialize()
        {
            OwnerID = ReadInt();
            NetworkID = ReadInt();
            ClaimOwner = ReadBool();
            ClaimOwnerTime = ReadLong();
        }

        public override void OnSerialize()
        {
            WriteInt(OwnerID);
            WriteInt(NetworkID);
            WriteBool(ClaimOwner);
            WriteLong(ClaimOwnerTime);
        }
    }
}