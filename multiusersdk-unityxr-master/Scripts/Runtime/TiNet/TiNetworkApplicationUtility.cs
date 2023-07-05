using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Ximmerse.XR
{
    public static class TiNetworkApplicationUtility
    {
        /// <summary>
        /// 获取 GameObject是否被Sync Identity 同步。
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="syncIdentity"></param>
        /// <returns></returns>
        public static bool IsSyncGameObject(this GameObject gameObject, out I_SyncIdentity syncIdentity)
        {
            syncIdentity = null;
            for (int i = 0, SyncIdentityentitiesCount = SyncIdentity.entities.Count; i < SyncIdentityentitiesCount; i++)
            {
                var id = SyncIdentity.entities[i];
                if(id.targetGameObject == gameObject)
                {
                    syncIdentity = id;
                    return true;
                }
            }
            return false;
        }
    }

}