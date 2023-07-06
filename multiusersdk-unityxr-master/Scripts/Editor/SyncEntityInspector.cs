using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Ximmerse.XR.Reflection;
using System.Linq;
using Ximmerse.XR.UnityNetworking;


namespace Ximmerse.XR
{
    [CustomEditor(typeof(SyncIdentity))]
    public class SyncEntityInspector : Editor
    {

        SyncIdentity syncEntity
        {
            get => target as SyncIdentity;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            //对所有的 entities 自动排序:
            if (GUILayout.Button("Auto assign ID"))
            {
                SortAllEntitiesID();
            }
        }

        void SortAllEntitiesID()
        {
            var allEntities = FindObjectsOfType<SyncIdentity>();
            var invalid = new List<SyncIdentity>();
            var valid = new List<SyncIdentity>();
            int minID = 0, maxID = 0;
            foreach (var e in allEntities)
            {
                if (e.GetFieldValue<int>("m_NetworkID") == -1)
                {
                    invalid.Add(e);
                }
                else if (valid.Count(x=>x.NetworkID == e.NetworkID) > 0)
                {
                    invalid.Add(e);
                }
                else
                {
                    valid.Add(e);
                }
            }
            if(invalid.Count > 0)
            {
                if (valid.Count > 0)
                {
                    minID = valid.Min(x => x.NetworkID);
                    maxID = valid.Max(x => x.NetworkID);
                }
                else
                {
                    minID = 0;
                    maxID = 0;
                }

                for (int i = 0, invalidCount = invalid.Count; i < invalidCount; i++)
                {
                    var id = invalid[i];
                    var oldID = id.NetworkID;
                    id.SetFieldValue<int>("m_NetworkID", maxID + i + 1);
                    EditorUtility.SetDirty(id);
                    Debug.LogFormat(id.gameObject, "Set {2} ID: {0}=>{1}", oldID, id.NetworkID, id);
                }
            }
          
        }

        /// <summary>
        /// 为选中的GameObject 创建一个 SyncIdentity, 令其成为网络同步对象。
        /// </summary>
        [MenuItem("GameObject/TiNet/Sync Identity", priority = 0)]
        public static void CreateSyncIdentity(MenuCommand menuCommand)
        {
            var selectT = menuCommand.context as GameObject;
            if (selectT == null)
            {
                return;
            }
            Debug.Log(selectT.name);
            if (selectT)
            {
                GameObject syncGORoot = GameObject.Find("SyncGameObjectRoot");
                if (syncGORoot == null)
                {
                    syncGORoot = new GameObject("SyncGameObjectRoot");
                }


                {
                    GameObject syncGo = new GameObject(string.Format("Sync:{0}", selectT.name), new System.Type[] { typeof(SyncIdentity), typeof(SyncTransform), typeof(SyncGameObjectState) });
                    var syncID = syncGo.GetComponent<SyncIdentity>();
                    syncGo.transform.SetParenAtIdentityPosition(syncGORoot.transform);
                    syncID.targetGameObject = selectT.gameObject;
                    syncID.NetworkID = GetNetworkID();
                }
                var tiNetManager = Object.FindObjectOfType<TiNetManager>();
                if (tiNetManager == null)
                {
                    var tinet = syncGORoot.AddComponent<TiNet>();
                    tiNetManager = syncGORoot.AddComponent<TiNetManager>();
                    tinet.AutoStart = true;
                    Debug.Log("TiNet instance is created.");
                }
            }
        }

        public static int GetNetworkID()
        {
            int MaxID = -1;
            var lst = FindObjectsOfType<SyncIdentity>();
            foreach(var identity in lst)
            {
                MaxID = (MaxID < identity.NetworkID) ? identity.NetworkID : MaxID;
            }
            return MaxID + 1;
        }
    }
}
