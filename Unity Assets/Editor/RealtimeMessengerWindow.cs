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
    public class RealtimeMessengersWindow : EditorWindow
    {

        [MenuItem("Tools/Realtime Messenger Settings")]
        public static void ShowWindow()
        {
            GetWindowWithRect<RealtimeMessengersWindow>(new Rect(0, 0, 640, 550), false, "Messenger");
        }


        static bool CreateSettings()
        {
            var instance = Resources.Load<MessengerSettingsObject>("MessengerSettings");
            if (instance == null)
            {
                Debug.Log("RealtimeNetwork Created at Assets/Resources/MessengerSettings.asset");

                var inst = CreateInstance<MessengerSettingsObject>();

                if (!Directory.Exists(Application.dataPath + "/Resources"))
                    AssetDatabase.CreateFolder("Assets", "Resources");

                AssetDatabase.CreateAsset(inst, "Assets/Resources/MessengerSettings.asset");

                AssetDatabase.SaveAssets();
                return String.IsNullOrEmpty(inst.ApplicationKey);
            }
            return String.IsNullOrEmpty(instance.ApplicationKey);
        }

        static MessengerSettingsObject Target
        {
            get
            {
                if (MessengerSettingsObject.Instance == null)
                    CreateSettings();
                return MessengerSettingsObject.Instance;
            }
        }

        void MyAccount()
        {
            Application.OpenURL("https://accounts.realtime.co/subscriptions/");
        }


        void Documentation()
        {
            Application.OpenURL("http://framework.realtime.co/messaging/#documentation");
        }

        void Default()
        {
            Target.ResetToDefault();
        }

        void OnGUI()
        {
            var logo = (Texture2D)Resources.Load("icon-realtimeco", typeof(Texture2D));

            GUILayout.BeginHorizontal(GUILayout.MinHeight(64));

            GUILayout.Label("Realtime Messenger", new GUIStyle
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

                GUILayout.Space(16);
            }

            GUILayout.EndHorizontal();
            //
            //
            GUILayout.Label("Your Application Key");
            Target.ApplicationKey = EditorGUILayout.TextField(Target.ApplicationKey);
            EditorStyles.label.wordWrap = true;
            GUILayout.Label("If you do not know or have an application key please use the Realtime/My Account menu command");
            GUILayout.Space(16);

            GUILayout.Label("Private Key (Optional)");
            Target.PrivateKey = EditorGUILayout.TextField(Target.PrivateKey);
            GUILayout.Label("Private key is required if you want to enable / disable presence or authorize clients locally.");
            GUILayout.Label("It is recommended that you do not do this, but use setup a web server to authorize clients.");
            GUILayout.Space(16);

            GUILayout.Label("Service URL");
            Target.Url = EditorGUILayout.TextField(Target.Url);
            GUILayout.Space(16);
            Target.IsCluster = GUILayout.Toggle(Target.IsCluster, "Is Cluster");
            GUILayout.Space(16);

            GUILayout.Label(string.Format("Authentication Time ({0} Minutes)", Target.AuthenticationTime / 60));
            Target.AuthenticationTime = Mathf.RoundToInt(GUILayout.HorizontalSlider(Target.AuthenticationTime, 60, 1800));
            GUILayout.Space(16);

            Target.AuthenticationIsPrivate = GUILayout.Toggle(Target.AuthenticationIsPrivate, "Is Private");
            GUILayout.Label("Limits authentication to one per client.");
            GUILayout.Space(16);
            //
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset to Default"))
            {
                Default();
            }
            if (GUILayout.Button("Documentation"))
            {
                Documentation();
            }
            if (GUILayout.Button("My Account"))
            {
                MyAccount();
            }
            EditorGUILayout.EndHorizontal();
            //
            EditorUtility.SetDirty(Target);
        }
    }
}