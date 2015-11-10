// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------
namespace Realtime.Storage.Models
{
    /// <summary>
    /// Listing of tables returned from the GetTables Repository Action
    /// </summary>
    public class TableList
    {
        // ReSharper disable InconsistentNaming

        /// <summary>
        /// table names
        /// </summary>
        public string[] tables { get; set; }
        /// <summary>
        /// table for paging
        /// </summary>
        public string stopTable { get; set; }
    }
}