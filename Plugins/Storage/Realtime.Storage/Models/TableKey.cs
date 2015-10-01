// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------
namespace Realtime.Storage.Models
{
    /// <summary>
    /// Table key schema.
    /// </summary>
    public class TableKey
    {
        // ReSharper disable InconsistentNaming

        /// <summary>
        /// Structure of the primary key.
        /// </summary>
        public Key primary { get; set; }

        /// <summary>
        /// Structure of the secondary key.
        /// </summary>
        public Key secondary { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public TableKey() { }

        /// <summary>
        /// Creates a key schema composed of a primary key.
        /// </summary>
        /// <param name="_primary">Structure of the primary key.</param>
        public TableKey(Key _primary)
        {
            primary = _primary;
        }

        /// <summary>
        /// Creates a key schema composed of a primary and secondary key.
        /// </summary>
        /// <param name="_primary">Structure of the primary key.</param>
        /// <param name="_secondary">Structure of the secondary key.</param>
        public TableKey(Key _primary, Key _secondary)
        {
            primary = _primary;
            secondary = _secondary;
        }
    }
}