using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Ximmerse.XR.UnityNetworking
{
    [CustomEditor(typeof(TiNet))]
    public class TiNetEditor : Editor
    {

        TiNet tinet { get => this.target as TiNet; }

        string text = string.Empty;

        string Address = "128.0.0.1";
        int DicoveryPort = 9090;
        int TcpPort = 9091;
        int UdpPort = 9092;

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("AutoStart"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Strictmode"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ShouldDetachNetworkOnAppPaused"), new GUIContent("Detach when pause"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CustomTag"));//Custom tag.
            EditorGUILayout.PropertyField(serializedObject.FindProperty("PortBroadcast"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("PortReliable"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("PortUnreliable"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BroadcastDiscovery"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AutoJointNetwork"));


            EditorGUILayout.PropertyField(serializedObject.FindProperty("FilterMode"));
            if(tinet.FilterMode == TiNet.ConnectionFilterMode.UseFilterWord)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("FilterWord"));
            }

            if(Application.IsPlaying (target))
            {
                EditorGUILayout.LabelField(string.Format("Is network started: {0}", tinet.IsNetworkStarted));
                EditorGUILayout.LabelField(string.Format("Connection count: {0}", tinet.NodeCount));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("thisNode"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_nodes"), true);
                EditorGUILayout.Separator();

                if (!tinet.IsNetworkStarted)
                {
                    if(GUILayout.Button ("Start Network"))
                    {
                        tinet.StartNetworking();
                    }
                }
                else
                {
                   

                    if (GUILayout.Button("Stop Network"))
                    {
                        tinet.StopNetworking();
                    }

                    text = EditorGUILayout.TextField(text);
                    if (GUILayout.Button("Send Text (Reliable)"))
                    {
                        tinet.SendTextMessageToAll_Reliable(text);
                    }
                    if (GUILayout.Button("Send Text (Unreliable)"))
                    {
                        tinet.SendTextMessageToAll_Unreliable(text);
                    }
                }
                
                EditorGUILayout.BeginHorizontal();
                Address = EditorGUILayout.TextField(Address);
                //TcpPort = EditorGUILayout.IntField(TcpPort);
                UdpPort = EditorGUILayout.IntField(UdpPort);
                if (GUILayout.Button("Connects To"))
                {
                    tinet.TryConnectsTo(Address, UdpPort, true);
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("DebugMode"));

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}