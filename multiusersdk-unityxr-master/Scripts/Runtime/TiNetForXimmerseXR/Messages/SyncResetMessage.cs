using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ximmerse.XR.UnityNetworking;

namespace Ximmerse.XR
{
    [Message(MessageCode.kSyncReset)]
    public class SyncResetMessage : TiNetMessage
    {
        public List<int> ResetID = new List<int>();

        /// <summary>
        /// Writes to network
        /// </summary>
        public override void OnSerialize()
        {
            this.WriteIntList(ResetID);
        }
        /// <summary>
        /// Reads from network
        /// </summary>
        public override void OnDeserialize()
        {
            this.ReadInts(ResetID);
        }
    }
}