// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------
using System.Collections.Generic;

namespace Realtime.Storage.Models
{
    /// <summary>
    /// Metadata for a StorageRef
    /// </summary>
    public class StorageMetadata
    {
        readonly Dictionary<string, TableMetadata> _tables = new Dictionary<string, TableMetadata>();

        /// <summary>
        /// adds the table metadata to the internal list
        /// </summary>
        /// <param name="tableMetadata"></param>
       public void Add(TableMetadata tableMetadata)
        {
            if (!_tables.ContainsKey(tableMetadata.name))
            {
                _tables.Add(tableMetadata.name, tableMetadata);
            }
        }

        /// <summary>
        /// gets the table metadata for the table
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public TableMetadata Get(string table)
        {
            TableMetadata metadata;
            _tables.TryGetValue(table, out metadata);

            return metadata;
        }
    }
}