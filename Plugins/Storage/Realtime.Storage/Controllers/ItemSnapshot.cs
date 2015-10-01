// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------
using Realtime.Storage.Models;

namespace Realtime.Storage.Controllers
{
    /// <summary>
    /// Definition of an item snapshot.
    /// </summary>
    sealed public class ItemSnapshot
    {
        readonly object _item;
        readonly TableMetadata _metadata;
        readonly StorageController _repository;

        internal ItemSnapshot(object item)
        {
            _item = item;
        }

        internal ItemSnapshot(object item, StorageController repository, TableMetadata metadata)
        {
            _item = item;
            _metadata = metadata;
            _repository = repository;
        }

        /// <summary>
        /// Retrieves a new reference for the item represented by the snapshot.
        /// </summary>
        /// <returns>A new item reference.</returns>
        public ItemRef<T> Ref<T>()  where T : class
        {
            return new ItemRef<T>((T)_item, _repository, _metadata);
        }

        /// <summary>
        /// Returns the properties of the item represented by the snapshot..
        /// </summary>
        /// <returns>The value of this snapshot.</returns>
        public object Val()
        {
            return _item;
        }
        /// <summary>
        /// Returns the properties of the item represented by the snapshot..
        /// </summary>
        /// <returns>The value of this snapshot.</returns>
        public T Val<T>() 
        {
            return (T)_item;
        }
    }
}