// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------

using UnityEngine;

namespace Realtime.Messaging
{

    /// <summary>
    /// Configuration file for the Realtime Messenger
    /// </summary>
    [AddComponentMenu("Realtime/Messenger Settings")]
    public class MessengerSettings : ScriptableObject
    {
        #region singleton

        private static MessengerSettings _instance;

        /// <summary>
        /// Access for the Network Manager
        /// </summary>
        public static MessengerSettings Instance
        {
            get
            {
                Init();
                return _instance;
            }
        }

        /// <summary>
        /// Initializes the RealtimeMessanger Services if not initialized.
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            if (_instance != null)
                return;

            _instance = Resources.Load<MessengerSettings>("MessengerSettings");

            if (_instance == null)
            {
                Debug.LogWarning("Messenger Settings Not Found. Please Use The Menu Command 'Realtime/Messenger Settings'");
            }
        }

        #endregion



        /// <summary>
        /// REPLACE WITH UOUR APPLICATION KEY
        /// </summary>
        public string ApplicationKey;

        /// <summary>
        /// OPTIONAL : REPLACE WITH UOUR PRIVATE KEY.
        /// REQUIRED FOR AUTHENTICATION AND PRESENCE
        /// </summary>
        public string PrivateKey;

        /// <summary>
        /// service URL
        /// </summary>
        public string Url = "http://ortc-developers.realtime.co/server/2.1";

        /// <summary>
        /// SERVICE URL IS CLUSTER
        /// </summary>
        public bool IsCluster = true;

        /// <summary>
        /// In Seconds. 1800 = 30 min
        /// </summary>
        public int AuthenticationTime = 1800;

        /// <summary>
        /// Only one connection can use this token since it's private for each user
        /// </summary>
        public bool AuthenticationIsPrivate = true;

        /// <summary>
        /// Resets all settings to default
        /// </summary>
        public void ResetToDefault()
        {
            Url = "http://ortc-developers.realtime.co/server/2.1";
            IsCluster = true;
            AuthenticationIsPrivate = true;
            AuthenticationTime = 1800;
            ApplicationKey = PrivateKey = string.Empty;
        }
    }
}