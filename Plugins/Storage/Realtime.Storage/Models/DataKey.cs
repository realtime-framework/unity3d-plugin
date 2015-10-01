// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------
namespace Realtime.Storage.Models
{
    /// <summary>
    /// Value of each key of the table.
    /// </summary>
    public class DataKey
    {
        // ReSharper disable InconsistentNaming

        /// <summary>
        /// Value of the primary key.
        /// </summary>
        public object primary;
        /// <summary>
        /// Value of the secondary key.
        /// </summary>
        public object secondary;

        /// <summary>
        /// Creates an empty key.
        /// </summary>
        public DataKey() { }

        /// <summary>
        /// Creates a key composed of primary and secondary values.
        /// </summary>
        /// <param name="primary">Value of the primary key.</param>
        /// <param name="secondary">Value of the secondary key.</param>
        public DataKey(object primary, object secondary)
        {
            this.primary = primary;
            this.secondary = secondary;
        }

        /// <summary>
        /// Creates a key composed of primary and secondary values.
        /// </summary>
        /// <param name="primary">Value of the primary key.</param>
        public DataKey(object primary)
        {
            this.primary = primary;
            secondary = null;
        }
    }
}