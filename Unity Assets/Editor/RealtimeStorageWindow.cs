// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Realtime.Editor
{
    public class RealtimeStorageWindow : EditorWindow
    {

        [MenuItem("Tools/Realtime Storage Settings")]
        public static void ShowWindow()
        {
            GetWindowWithRect<RealtimeStorageWindow>(new Rect(0, 0, 640, 400), false, "Storage");
        }

        static bool CreateSettings()
        {
            var instance = Resources.Load<StorageSettingsObject>("StorageSettings");
            if (instance == null)
            {
                Debug.Log("RealtimeStorage Created at Assets/Resources/StorageSettings.asset");

                var inst = CreateInstance<StorageSettingsObject>();

                if (!Directory.Exists(Application.dataPath + "/Resources"))
                    AssetDatabase.CreateFolder("Assets", "Resources");

                AssetDatabase.CreateAsset(inst, "Assets/Resources/StorageSettings.asset");

                AssetDatabase.SaveAssets();
                return String.IsNullOrEmpty(inst.ApplicationKey);
            }
            return String.IsNullOrEmpty(instance.ApplicationKey);
        }

        static StorageSettingsObject Target
        {
            get
            {
                if (StorageSettingsObject.Instance == null)
                    CreateSettings();
                return StorageSettingsObject.Instance;
            }
        }

        void MyAccount()
        {
            Application.OpenURL("https://accounts.realtime.co/subscriptions/");
        }


        void Documentation()
        {
            Application.OpenURL("http://framework.realtime.co/storage/#documentation");
        }


        void Default()
        {
            Target.ResetToDefault();
        }


        void OnGUI()
        {
            var logo = (Texture2D)Resources.Load("icon-realtimeco", typeof(Texture2D));

            GUILayout.BeginHorizontal(GUILayout.MinHeight(64));

            EditorGUILayout.LabelField("Realtime Storage", new GUIStyle
            {
                fontSize = 32,
                padding = new RectOffset(16, 0, 16, 0),
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState
                {
                    textColor = Color.white
                }


            });

            if (logo != null)
            {
                GUILayout.Space(32);
                GUILayout.Label(logo, GUILayout.MinHeight(64));

                EditorGUILayout.Separator();
            }

            GUILayout.EndHorizontal();
            //

            
            GUILayout.Label("Your Application Key");
            Target.ApplicationKey = EditorGUILayout.TextField(Target.ApplicationKey);

            EditorStyles.label.wordWrap = true;
            GUILayout.Label("If you do not know or have an application key please use the Realtime/My Account menu command");
            GUILayout.Space(16);

            GUILayout.Label("Private Key (Optional)");
            Target.PrivateKey = EditorGUILayout.TextField(Target.PrivateKey);
            EditorGUILayout.LabelField("Required for configuring tables from this client.");
            GUILayout.Space(16);
            EditorGUILayout.LabelField("Storage URL");
            Target.StorageUrl = EditorGUILayout.TextField(Target.StorageUrl);
            GUILayout.Space(16);

            //
            GUILayout.BeginHorizontal();
            Target.StorageIsCluster = GUILayout.Toggle(Target.StorageIsCluster, "Is Cluster");
            Target.StorageSSL = GUILayout.Toggle(Target.StorageSSL, "Is Https");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            //         GUILayout.Space(16);
            GUILayout.Label("Messenger URL");
            Target.MessengerUrl = EditorGUILayout.TextField(Target.MessengerUrl);
            GUILayout.Space(16);
            //
            GUILayout.BeginHorizontal();
            Target.MessengerIsCluster = GUILayout.Toggle(Target.MessengerIsCluster, "Is Cluster");
            Target.MessengerSSL = GUILayout.Toggle(Target.MessengerSSL, "Is Https");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            //         GUILayout.Space(16);
            //
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Reset to Default"))
            {
                Default();
            }
            EditorGUILayout.Separator();

            if (GUILayout.Button("Documentation"))
            {
                Documentation();
            }
            if (GUILayout.Button("My Account"))
            {
                MyAccount();
            }
            GUILayout.EndHorizontal();
            //

            EditorUtility.SetDirty(Target);
        }

    }
}