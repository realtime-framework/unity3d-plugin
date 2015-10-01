// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------

#if !UNITY_WSA && !UNITY_EDITOR
using Realtime.Storage.DataAccess;
#endif
using UnityEngine;

namespace Realtime
{

    /// <summary>
    /// Default Settings for the Storage API
    /// </summary>
    [AddComponentMenu("Realtime/Storage Settings")]
    public class StorageSettingsObject : ScriptableObject
    {
        #region singleton

        private static StorageSettingsObject _instance;

        /// <summary>
        /// Access for the Network Manager
        /// </summary>
        public static StorageSettingsObject Instance
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
        /// Instantiates the RealtimeStorageSettings from Resources
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        public static void InitService()
        {
            if (_instance != null)
                return;

            _instance = Resources.Load<StorageSettingsObject>("StorageSettings");

            if (_instance == null)
            {
                Debug.LogWarning("StorageSettings Not Found. Please Use The Menu Command 'Realtime/Storage Settings'");
            }
        }

        public void OnEnable()
        {
#if !UNITY_WSA && !UNITY_EDITOR
            StorageSettings.ApplicationKey = ApplicationKey;
            StorageSettings.PrivateKey = PrivateKey;
            StorageSettings.StorageUrl = StorageUrl;
            StorageSettings.StorageIsCluster = StorageIsCluster;
            StorageSettings.StorageSSL = StorageSSL;
            StorageSettings.MessengerUrl = MessengerUrl;
            StorageSettings.MessengerIsCluster = MessengerIsCluster;
            StorageSettings.MessengerSSL = MessengerSSL;
#endif
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
        public string StorageUrl = "storage-balancer.realtime.co";

        /// <summary>
        /// SERVICE URL IS CLUSTER
        /// </summary>
        public bool StorageIsCluster = true;

        /// <summary>
        /// SERVICE URL IS HTTPS
        /// </summary>
        public bool StorageSSL = false;

        /// <summary>
        /// service URL
        /// </summary>
        public string MessengerUrl = "ortc-storage.realtime.co";

        /// <summary>
        /// SERVICE URL IS CLUSTER
        /// </summary>
        public bool MessengerIsCluster = true;

        /// <summary>
        /// SERVICE URL IS HTTPS
        /// </summary>
        public bool MessengerSSL = false;

        /// <summary>
        /// Sets all Settings to default
        /// </summary>
        public void ResetToDefault(){

            MessengerUrl = "ortc-storage.realtime.co";
            StorageUrl = "storage-balancer.realtime.co";
            MessengerIsCluster =  StorageIsCluster = true;
            StorageSSL = MessengerSSL = false;
            ApplicationKey = PrivateKey = string.Empty;
        }

    }
}