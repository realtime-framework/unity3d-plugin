// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------
namespace Realtime.Storage.Models
{
    /// <summary>
    /// Describes the usage of a table
    /// </summary>
    public class TableThroughput
    {
        // ReSharper disable InconsistentNaming

        /// <summary>
        /// table reads per second
        /// </summary>
        public int read { get; set; }

        /// <summary>
        /// table writes per second
        /// </summary>
        public int write { get; set; }

        /// <summary>
        /// creates a new table
        /// </summary>
        /// <param name="read"></param>
        /// <param name="write"></param>
        public TableThroughput(int read, int write)
        {
            this.read = read;
            this.write = write;
        }

        /// <summary>
        /// creates a new table
        /// </summary>
        public TableThroughput() : this(1,1)
        {
            
        }
    }
}