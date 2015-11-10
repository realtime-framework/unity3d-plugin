// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------
namespace Realtime.Storage.Models
{

    /// <summary>
    /// Available types of provisioning (number of read/write operations per second).
    /// </summary>
    public enum StorageProvisionType
    {
        /// <summary>
        /// Id of the Light provision type (26 operations per second).
        /// </summary>
        Light = 1,
        /// <summary>
        /// Id of the Medium  provision type (50 operations per second).
        /// </summary>
        Medium = 2,
        /// <summary>
        /// Id of the Intermediate provision type (100 operations per second).
        /// </summary>
        Intermediate = 3,
        /// <summary>
        /// Id of the Heavy provision type (200 operations per second).
        /// </summary>
        Heavy = 4,
        /// <summary>
        /// Id of the Custom provision type (customized read and write throughput).
        /// </summary>
        Custom = 5
    };

    /// <summary>
    /// Available options of provision load (how the number of operations are divided between the read and write throughput).
    /// </summary>
    public enum StorageProvisionLoad
    {
        /// <summary>
        /// Id of the Read provision load (Assign more read capacity than write capacity).
        /// </summary>
        Read = 1,
        /// <summary>
        /// Id of the Write provision load (Assign more write capacity than read capacity).
        /// </summary>
        Write = 2,
        /// <summary>
        /// Id of the Balanced provision load (Assign similar read an write capacity).
        /// </summary>
        Balanced = 3,
        /// <summary>
        /// Id of the Custom provision load.
        /// </summary>
        Custom = 4
    };

    /// <summary>
    /// Available types of events that can be subscribed at table and item level.
    /// </summary>
    public enum StorageEventType
    {
        /// <summary>
        /// Create
        /// </summary>
        PUT,
        /// <summary>
        /// Update
        /// </summary>
        UPDATE,
        /// <summary>
        /// Delete
        /// </summary>
        DELETE
    }

    /// <summary>
    /// Available types of notifications that can be invoked at table and item level.
    /// </summary>
    internal enum StorageNotificationType
    {
        /// <summary>
        /// Notified every time
        /// </summary>
        ON,
        /// <summary>
        /// Notified only once
        /// </summary>
        ONCE,
        /// <summary>
        /// Delete Notice
        /// </summary>
        DELETE
    }

}