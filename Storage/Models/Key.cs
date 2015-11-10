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
    /// Structure of a table key.
    /// </summary>
    public class Key
    {
        // ReSharper disable InconsistentNaming

        /// <summary>
        /// Allowed key types.
        /// </summary>
        public enum DataType { 
            /// <summary>
            /// numeric column
            /// </summary>
            NUMBER, 
            /// <summary>
            /// String column
            /// </summary>
            STRING 
        };

        /// <summary>
        /// Name of the key.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Type of the key.
        /// </summary>
        public DataType dataType { get; set; }

        /// <summary>
        /// Creates a key.
        /// </summary>
        /// <param name="name">Name of the key.</param>
        /// <param name="dataType">Type of the key.</param>
        public Key(string name, DataType dataType)
        {
            this.name = name;
            this.dataType = dataType;
        }

        /// <summary>
        /// Creates a key.
        /// </summary>
        /// <param name="name">Name of the key.</param>
        /// <param name="dataType">Type of the key.</param>
        public Key(string name, string dataType)
        {
            this.name = name;
            this.dataType = (DataType)Enum.Parse(typeof(DataType), dataType.ToUpper());
        }

        /// <summary>
        /// ctor
        /// </summary>
        public Key()
        {
            
        }
    }
}