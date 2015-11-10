// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------
using System;

namespace Realtime.Storage.Models
{
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// Possible table status.
    /// </summary>
    public enum ProvisionType
    {
        /// <summary>
        /// (26 operations per second)
        /// </summary>
        Light =1,
        /// <summary>
        /// (50 operations per second)
        /// </summary>
        Medium,
        /// <summary>
        /// (100 operations per second)  
        /// </summary>
        Intermediate,
        /// <summary>
        ///  (200 operations per second)
        /// </summary>
        Heavy,
        /// <summary>
        /// Set manually
        /// </summary>
        Custom
    }

    /// <summary>
    /// Possible table status.
    /// </summary>
    public enum ProvisionLoad
    {
        /// <summary>
        /// (Assign more read capacity than write capacity.) 
        /// </summary>
        Read =1,
        /// <summary>
        /// (Assign similar read an write capacity.)
        /// </summary>
        Balanced,
        /// <summary>
        /// (Assign more write capacity than read capacity.
        /// </summary>
        Write,
        /// <summary>
        /// Set manually
        /// </summary>
        Custom,
    }

    /// <summary>
    /// Information regarding a table structure
    /// </summary>
    public class TableMetadata
    {
        /// <summary>
        /// Possible table status.
        /// </summary>
        public enum Status { 
            /// <summary>
            /// Table is 
            /// </summary>
            ACTIVE,
            /// <summary>
            /// Table is being created. While in this state, it's not possible to perform operations over its items. 
            /// </summary>
            CREATING,
            /// <summary>
            /// Table throughput is being updated but item manipulation is still possible.
            /// </summary>
            UPDATING, 
            /// <summary>
            /// Table is being deleted.
            /// </summary>
            DELETING 
        }

        /// <summary>
        /// The table name
        /// </summary>
        public string table
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Name of the table.
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Key structure of the table.
        /// </summary>
        public TableKey key { get; set; }
        /// <summary>
        /// Id of the provision type.
        /// </summary>
        public ProvisionType provisionType { get; set; }
        /// <summary>
        /// Id of the provision load.
        /// </summary>
        public ProvisionLoad provisionLoad { get; set; }
        /// <summary>
        /// Throughput of the table.
        /// </summary>
        public TableThroughput throughput { get; set; }
        /// <summary>
        /// Current table status
        /// </summary>
        public Status status { get; set; }
        /// <summary>
        /// Date of creation.
        /// </summary>
        public DateTime creationDate { get; set; }
        /// <summary>
        /// Size of the table.
        /// </summary>
        public int size { get; set; }
        /// <summary>
        /// Number of items in the table. Updated every 6 hours.
        /// </summary>
        public int itemCount { get; set; }
    }
}