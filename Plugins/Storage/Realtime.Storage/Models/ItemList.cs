// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------
namespace Realtime.Storage.Models
{
    /// <summary>
    /// Listing of Items with a stop key for paging
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ItemList<T>
    {
        // ReSharper disable InconsistentNaming

        /// <summary>
        /// found items
        /// </summary>
        public T[] items { get; set; }

        /// <summary>
        /// Stop key for paging
        /// </summary>
        public DataKey stopKey { get; set; }
    }
}