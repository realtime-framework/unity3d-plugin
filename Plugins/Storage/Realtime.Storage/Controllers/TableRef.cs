// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Foundation.Tasks;
using Realtime.LITJson;
using Realtime.Storage.DataAccess;
using Realtime.Storage.Models;

namespace Realtime.Storage.Controllers
{

    /// <summary>
    /// TableRef is a Extends DataAccess with Update Notification via the Messenger.
    /// All Data Acquired via the TableRef will be updated.
    /// </summary>
    public class TableRef<T> where T : class
    {
        /// <summary>
        /// Table Metadata
        /// </summary>
        public TableMetadata TableMeta { get; set; }
        /// <summary>
        /// Item Metadata
        /// </summary>
        public ItemMetadata ItemMeta { get; private set; }
        /// <summary>
        /// Name of the Table
        /// </summary>
        public string TableName { get; private set; }
        readonly string _channel = "rtcs_";
        private readonly StorageRepository _repository;
        private readonly StorageController _storage;

        // IOS AOT error for generics here
        readonly Dictionary<StorageEventType, Dictionary<StorageNotificationType, List<Action<ItemSnapshot>>>> _notifications;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="storage"></param>
        public TableRef(StorageController storage)
        {
            _storage = storage;
            _repository = storage.Repository;
            ItemMeta = _repository.GetMetadata<T>();
            TableName = ItemMeta.Table;
            _channel += ItemMeta.Table;

            _notifications = new Dictionary<StorageEventType, Dictionary<StorageNotificationType, List<Action<ItemSnapshot>>>>();

            var putEvents = new Dictionary<StorageNotificationType, List<Action<ItemSnapshot>>>();
            putEvents.Add(StorageNotificationType.ON, new List<Action<ItemSnapshot>>());
            putEvents.Add(StorageNotificationType.ONCE, new List<Action<ItemSnapshot>>());
            putEvents.Add(StorageNotificationType.DELETE, new List<Action<ItemSnapshot>>());
            _notifications.Add(StorageEventType.PUT, putEvents);
            var updateEvents = new Dictionary<StorageNotificationType, List<Action<ItemSnapshot>>> {
                {StorageNotificationType.ON, new List<Action<ItemSnapshot>>()},
                {StorageNotificationType.ONCE, new List<Action<ItemSnapshot>>()}, 
                {StorageNotificationType.DELETE, new List<Action<ItemSnapshot>>()}
            };
            _notifications.Add(StorageEventType.UPDATE, new Dictionary<StorageNotificationType, List<Action<ItemSnapshot>>>(updateEvents));
            var deleteEvents = new Dictionary<StorageNotificationType, List<Action<ItemSnapshot>>>
            {
                {StorageNotificationType.ON, new List<Action<ItemSnapshot>>()},
                {StorageNotificationType.ONCE, new List<Action<ItemSnapshot>>()}, 
                {StorageNotificationType.DELETE, new List<Action<ItemSnapshot>>()}
            };
            _notifications.Add(StorageEventType.DELETE, new Dictionary<StorageNotificationType, List<Action<ItemSnapshot>>>(deleteEvents));
        }

        #region Storage

        /// <summary>
        /// Delete this table.
        /// </summary>
        /// <returns>This table reference.</returns>
        public UnityTask<StorageResponse<TableMetadata>> Del()
        {
            return _repository.DeleteTable(TableName);
        }

        /// <summary>
        /// Adds a new item to the table.
        /// </summary>
        /// <typeparam name="T">The type of the item to add.</typeparam>
        /// <param name="item">The item to add.</param>
        /// <returns>This table reference.</returns>
        public UnityTask<StorageResponse<ItemSnapshot>> Push(T item)
        {
            return _repository.Update(item).ConvertTo(task =>
            {
                return task.IsFaulted
                    ? new StorageResponse<ItemSnapshot>(task.Result.error)
                    : new StorageResponse<ItemSnapshot>(new ItemSnapshot(task.Result.data));
            });
        }

        /// <summary>
        /// Creates the table referred by this reference.
        /// </summary>
        /// <param name="key">The definition of the key schema for this table. Must contain at least a primary key.</param>
        /// <param name="provisionType">Type of provisioning (number of read/write operations per second).</param>
        /// <param name="provisionLoad">Option of provision load (how the number of operations are divided between the read and write throughput).</param>
        /// <returns>This table reference.</returns>
        public UnityTask<StorageResponse<TableMetadata>> Create(TableKey key, ProvisionType provisionType, ProvisionLoad provisionLoad)
        {
            return _repository.CreateTable(new TableMetadata
            {
                provisionLoad = provisionLoad,
                provisionType = provisionType,
                name = TableName,
                key = key,
            });
        }

        /// <summary>
        /// Creates the table referred by this reference with a customized throughput.
        /// </summary>
        /// <param name="key">The definition of the key schema for this table. Must contain at least a primary key.</param>
        /// <param name="throughput">The custom provision to apply.</param>
        /// <returns>This table reference.</returns>
        public UnityTask<StorageResponse<TableMetadata>> Create(TableKey key, TableThroughput throughput)
        {
            return _repository.CreateTable(new TableMetadata
            {
                throughput = throughput,
                provisionLoad = ProvisionLoad.Balanced,
                provisionType = ProvisionType.Custom,
                name = TableName,
                key = key,
            });
        }

        /// <summary>
        /// Updates the provisioning of the table.
        /// </summary>
        /// <param name="provisionType">Type of provisioning (number of read/write operations per second).</param>
        /// <param name="provisionLoad">Option of provision load (how the number of operations are divided between the read and write throughput).</param>
        /// <returns>This table reference.</returns>
        public UnityTask<StorageResponse<TableMetadata>> Update(ProvisionType provisionType, ProvisionLoad provisionLoad)
        {
            return _repository.UpdateTable(new TableMetadata
            {
                provisionLoad = provisionLoad,
                provisionType = provisionType,
                name = TableName,
            });
        }

        /// <summary>
        /// Updates the provisioning of the table.
        /// </summary>
        /// <param name="throughput">The custom provision to apply.</param>
        /// <returns>This table reference.</returns>
        public UnityTask<StorageResponse<TableMetadata>> Update(TableThroughput throughput)
        {
            return _repository.UpdateTable(new TableMetadata
            {
                throughput = throughput,
                provisionLoad = ProvisionLoad.Balanced,
                provisionType = ProvisionType.Custom,
                name = TableName,
            });
        }

        /// <summary>
        /// Creates a new item reference.
        /// </summary>
        /// <typeparam name="T">The type of the item to add.</typeparam>
        /// <param name="item">The item to add.</param>
        /// <returns>This table reference.</returns>
        public ItemRef<T> Item(T item)
        {
            return new ItemRef<T>(item, _storage, TableMeta);
        }

        /// <summary>
        /// Retrieve a list of items based on the filters (if any) applied.
        /// </summary>
        /// <typeparam name="T">The type of the items to retrieve.</typeparam>
        /// <param name="request">describes the selection action</param>
        /// <returns>This table reference.</returns>
        public UnityTask<StorageResponse<IEnumerable<ItemSnapshot>>> GetItems(ItemQueryRequest<T> request)
        {

            return _repository.Query(request).ConvertTo(task =>
            {
                return task.IsFaulted 
                    ? new StorageResponse<IEnumerable<ItemSnapshot>>(task.Result.error)
                    : new StorageResponse<IEnumerable<ItemSnapshot>>(task.Result.data.items.Select(o => new ItemSnapshot(o, _storage, TableMeta)));
            });
        }

        #endregion

        #region Notifications

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TK"></typeparam>
        class TableNotificationMessage<TK>
        {
            // ReSharper disable InconsistentNaming
            // ReSharper disable UnusedAutoPropertyAccessor.Local
            public string type { get; set; }
            public TK data { get; set; }
            public StorageEventType eventType
            {
                get
                {
                    return (StorageEventType)Enum.Parse(typeof(StorageEventType), type, true);
                }
            }
        }

        void NotifyListeners(StorageNotificationType notificationType, IDictionary<StorageNotificationType, List<Action<ItemSnapshot>>> notification, ItemSnapshot itemSnapshot)
        {
            if (notification.ContainsKey(notificationType))
            {
                var events = notification[notificationType];
                foreach (var evt in events)
                {
                    evt.Invoke(itemSnapshot);
                }

                if (notificationType == StorageNotificationType.ONCE)
                    notification[notificationType].Clear();
            }
        }

        void Subscribe(string primaryKey)
        {
            // Note :
            // Anonymous methods can cause errors when removing notification
            var ch = primaryKey == String.Empty ? _channel : _channel + ":" + primaryKey;
            _storage.AddNotification(ch, OnMessage);
        }

        void OnMessage(string m)
        {
            var changedItem = JsonMapper.ToObject<TableNotificationMessage<T>>(m);

            // notify listeners
            if (_notifications.ContainsKey(changedItem.eventType))
            {
                var itemSnapshot = new ItemSnapshot(changedItem.data, _storage, TableMeta);
                var notification = _notifications[changedItem.eventType];

                NotifyListeners(StorageNotificationType.ON, notification, itemSnapshot);
                NotifyListeners(StorageNotificationType.ONCE, notification, itemSnapshot);
                NotifyListeners(StorageNotificationType.DELETE, notification, itemSnapshot);
            }
        }

        /// <summary>
        /// Attach a listener to run every time the eventType occurs on item(s) with the provided primary key.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="eventType">The type of the event to listen for.</param>
        /// <param name="primaryKeyValue">The primary key of the item(s). Everytime a change occurs to the item(s), the handler is called.</param>
        /// <param name="handler">The function to run whenever the notification is received. If the event type is "put", it will immediately trigger a "getItems" to get the initial data and run the callback with each item snapshot as argument.</param>
        /// <returns>This table reference.</returns>
        public TableRef<T> On(StorageEventType eventType, string primaryKeyValue, Action<ItemSnapshot> handler)
        {
            _notifications[eventType][StorageNotificationType.ON].Add(handler);
            Subscribe(primaryKeyValue);
            return this;
        }

        /// <summary>
        /// Attach a listener to run every time the eventType occurs.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="eventType">The type of the event to listen for.</param>
        /// <param name="handler">The function to run whenever the notification is received. If the event type is "put", it will immediately trigger a "getItems" to get the initial data and run the callback with each item snapshot as argument.</param>
        /// <returns>This table reference.</returns>
        public TableRef<T> On(StorageEventType eventType, Action<ItemSnapshot> handler)
        {
            On(eventType, String.Empty, handler);
            return this;
        }

        /// <summary>
        /// Attach a listener to run only once, when the eventType occurs on item(s) with the provided primary key.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="eventType">The type of the event to listen for.</param>
        /// <param name="primaryKeyValue">The primary key of the item(s). When a change occurs to the item(s), the handler is called once.</param>
        /// <param name="handler">The function invoked, only once, when the notification is received. If the event type is "put", it will immediately trigger a "getItems" to get the initial data and run the callback with each item snapshot as argument.</param>
        /// <returns>This table reference.</returns>
        public TableRef<T> Once(StorageEventType eventType, string primaryKeyValue, Action<ItemSnapshot> handler)
        {
            _notifications[eventType][StorageNotificationType.ONCE].Add(handler);
            Subscribe(primaryKeyValue);
            return this;
        }

        /// <summary>
        /// Attach a listener to run only once when the eventType occurs.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="eventType">The type of the event to listen for.</param>
        /// <param name="handler">The function invoked, only once, when the notification is received. If the event type is "put", it will immediately trigger a "getItems" to get the initial data and run the callback with each item snapshot as argument.</param>
        /// <returns>This table reference.</returns>
        public TableRef<T> Once(StorageEventType eventType, Action<ItemSnapshot> handler)
        {
            Once(eventType, String.Empty, handler);
            return this;
        }

        /// <summary>
        /// Remove a listener(s).
        /// </summary>
        /// <param name="eventType">The type of the event.</param>
        /// <param name="primaryKeyValue">The primary key of the item(s)</param>
        /// <returns>This table reference.</returns>
        public TableRef<T> Off(StorageEventType eventType, string primaryKeyValue)
        {
            return this;
        }

        /// <summary>
        /// Remove a listener(s).
        /// </summary>
        /// <param name="eventType">The type of the event.</param>
        /// <returns>This table reference.</returns>
        public TableRef<T> Off(StorageEventType eventType)
        {
            Off(eventType, String.Empty);
            return this;
        }

        /// <summary>
        /// Remove all listeners.
        /// </summary>
        /// <returns></returns>
        public TableRef<T> Off()
        {
            Off(StorageEventType.DELETE);
            Off(StorageEventType.PUT);
            Off(StorageEventType.UPDATE);

            return this;
        }

        #endregion
    }
}