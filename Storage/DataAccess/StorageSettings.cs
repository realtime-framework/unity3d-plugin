// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------
using UnityEngine;

namespace Realtime.Storage.DataAccess
{

    /// <summary>
    /// Settings for the Storage API
    /// </summary>
    public class StorageSettings : ScriptableObject
    {
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

        #region singleton

        private static StorageSettings _instance;

        /// <summary>
        /// Access for the Network Manager
        /// </summary>
        public static StorageSettings Instance
        {
            get
            {
                Init();
                return _instance;
            }
        }

        /// <summary>
        /// Initializes the StorageSettings Services if not initialized.
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            if (_instance != null)
                return;

            _instance = Resources.Load<StorageSettings>("StorageSettings");

            if (_instance == null)
            {
                Debug.LogWarning("Storage Settings Not Found. Please Use The Menu Command 'Realtime/Storage Settings'");
            }
        }

        #endregion


        /// <summary>
        /// Resets all settings to default
        /// </summary>
        public void ResetToDefault()
        {
            StorageUrl = "storage-balancer.realtime.co";
            StorageIsCluster = true;
            StorageSSL = false;
            MessengerUrl = "ortc-storage.realtime.co";
            MessengerIsCluster = true;
            MessengerSSL = false;

        }
    }
}