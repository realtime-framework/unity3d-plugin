// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------
using System;

namespace Realtime.Storage.Models
{
    /// <summary>
    /// Specifies which of the items class attributes are part of the key schema.
    /// </summary>
    /// <remarks>
    /// Attach these to your data model classes
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public class StorageKeyAttribute : Attribute
    {
        /// <summary>
        /// Name of the table
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// Primary Key is used for Item Lookup
        /// </summary>
        public string Primary { get; set; }

        /// <summary>
        /// Secondary Key is used for Ordered Queries. 
        /// </summary>
        public string Secondary { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="primaryKey"></param>
        public StorageKeyAttribute(string tableName, string primaryKey)
        {
            Table = tableName;
            Primary = primaryKey;
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="primaryKey"></param>
        /// <param name="secondaryKey"></param>
        public StorageKeyAttribute(string tableName, string primaryKey, string secondaryKey)
        {
            Table = tableName;
            Primary = primaryKey;
            Secondary = secondaryKey;
        }
    }
}