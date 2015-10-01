// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------

using Realtime.Messaging;
using UnityEngine;

namespace Realtime
{

    /// <summary>
    /// client configuration
    /// </summary>
    [AddComponentMenu("Realtime/Messenger Settings")]
    public class MessengerSettingsObject : ScriptableObject
    {
        #region singleton

        private static MessengerSettingsObject _instance;

        /// <summary>
        /// Access for the Network Manager
        /// </summary>
        public static MessengerSettingsObject Instance
        {
            get
            {
                InitService();
                if (_instance)
                    _instance.OnEnable();
                return _instance;
            }
        }
        
        /// <summary>
        /// Initializes the RealtimeMessanger Services if not initialized.
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        public static void InitService()
        {
            if(_instance != null)
                return;

            _instance = Resources.Load<MessengerSettingsObject>("MessengerSettings");

            if (_instance == null)
            {
                Debug.LogWarning("Messenger Settings Not Found. Please Use The Menu Command 'Realtime/Messenger Settings'");
            }
        }

        public void OnEnable()
        {
            MessengerSettings.ApplicationKey = ApplicationKey;
            MessengerSettings.PrivateKey = PrivateKey;
            MessengerSettings.Url = Url;
            MessengerSettings.IsCluster = IsCluster;
            MessengerSettings.IsCluster = IsCluster;
            MessengerSettings.IsCluster = IsCluster;
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


