// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------
using System;
using System.Collections.Generic;
using Foundation.Tasks;
using Realtime.LITJson;
using Realtime.Storage.Models;

namespace Realtime.Storage.Controllers
{
    /// <summary>
    /// Definition of an item reference.
    /// </summary>
    /// <typeparam name="T">Type of the item.</typeparam>
    public class ItemRef<T> where T : class
    {
        /// <summary>
        /// Object the reference is manipulating.
        /// </summary>
        public T Item { get; set; }

        readonly TableMetadata _metadata;
        private readonly ItemMetadata _itemMeta;
        readonly StorageController _storageContext;
        // IOS AOT error for generics here
        readonly IDictionary<StorageEventType, Dictionary<StorageNotificationType, List<Action<ItemSnapshot>>>> _notifications;

        private readonly string _channel = "rtcs_";

        internal ItemRef(T item, StorageController storageContext, TableMetadata metadata)
        {
            Item = item;
            _metadata = metadata;
            _storageContext = storageContext;
            _itemMeta = _storageContext.Repository.GetMetadata<T>();
            _channel += metadata.name + ":";

            var dataKey = _itemMeta.GetKey(item);
            _channel += dataKey.primary;

            if (dataKey.secondary != null)
            {
                _channel += "_" + dataKey.secondary;
            }

            _notifications = new Dictionary<StorageEventType, Dictionary<StorageNotificationType, List<Action<ItemSnapshot>>>>();

            var putEvents = new Dictionary<StorageNotificationType, List<Action<ItemSnapshot>>> 
            {
            {StorageNotificationType.ON, new List<Action<ItemSnapshot>>()},
            {StorageNotificationType.ONCE, new List<Action<ItemSnapshot>>()},
            {StorageNotificationType.DELETE, new List<Action<ItemSnapshot>>()}
            };
            _notifications.Add(StorageEventType.PUT, putEvents);
            var updateEvents = new Dictionary<StorageNotificationType, List<Action<ItemSnapshot>>>
            {
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
        /// Get the value of this item reference.
        /// </summary>
        /// <returns>This item reference.</returns>
        public UnityTask<StorageResponse<ItemSnapshot>> Get()
        {
            return _storageContext.Repository.Get<T>(_itemMeta.GetKey(Item)).ConvertTo(task =>
            {
                if (task.IsFaulted)
                    return new StorageResponse<ItemSnapshot>(task.Result.error);

                Item = task.Result.data;

                return new StorageResponse<ItemSnapshot>(new ItemSnapshot(task.Result.data, _storageContext, _metadata));
            });
        }

        /// <summary>
        /// Updates the value of this item reference.
        /// </summary>
        /// <returns>This item reference.</returns>
        public UnityTask<StorageResponse<ItemSnapshot>> Set()
        {
            return _storageContext.Repository.Update(Item).ConvertTo(task =>
            {
                if (task.IsFaulted)
                    return new StorageResponse<ItemSnapshot>(task.Result.error);

                Item = task.Result.data;

                return new StorageResponse<ItemSnapshot>(new ItemSnapshot(task.Result.data, _storageContext, _metadata));
            });
        }

        /// <summary>
        /// Delete the value of this item reference.
        /// </summary>
        /// <returns>This item reference.</returns>
        public UnityTask<StorageResponse<ItemSnapshot>> Del()
        {
            return _storageContext.Repository.Delete(Item).ConvertTo(task =>
            {
                if (task.IsFaulted)
                    return new StorageResponse<ItemSnapshot>(task.Result.error);

                Item = task.Result.data;

                return new StorageResponse<ItemSnapshot>(new ItemSnapshot(task.Result.data, _storageContext, _metadata));
            });
        }

        /// <summary>
        /// Increments a given attribute of an item. If the attribute doesn't exist, it is set to zero before the operation.
        /// </summary>
        /// <param name="property">The name of the item's attribute.</param>
        /// <param name="value">The number to add.</param>
        /// <returns>This item reference.</returns>
        public UnityTask<StorageResponse<ItemSnapshot>> Incr(string property, int value)
        {
            return _storageContext.Repository.Incr(Item, property, value).ConvertTo(task =>
            {

                if (task.IsFaulted)
                    return new StorageResponse<ItemSnapshot>(task.Result.error);

                Item = task.Result.data;

                return new StorageResponse<ItemSnapshot>(new ItemSnapshot(task.Result.data, _storageContext, _metadata));
            });
        }

        /// <summary>
        /// Increments a given attribute of an item. If the attribute doesn't exist, it is set to zero before the operation.
        /// </summary>
        /// <param name="property">The name of the item's attribute.</param>
        /// <returns>This item reference.</returns>
        public UnityTask<StorageResponse<ItemSnapshot>> Incr(string property)
        {
            return _storageContext.Repository.Incr(Item, property).ConvertTo(task =>
            {
                if (task.IsFaulted)
                    return new StorageResponse<ItemSnapshot>(task.Result.error);

                Item = task.Result.data;

                return new StorageResponse<ItemSnapshot>(new ItemSnapshot(task.Result.data, _storageContext, _metadata));
            });
        }

        /// <summary>
        /// Decrements the value of an items attribute. If the attribute doesn't exist, it is set to zero before the operation.
        /// </summary>
        /// <param name="property">The name of the item's attribute.</param>
        /// <returns>This item reference.</returns>
        public UnityTask<StorageResponse<ItemSnapshot>> Decr(string property)
        {
            return _storageContext.Repository.Decr(Item, property).ConvertTo(task =>
            {
                if (task.IsFaulted)
                    return new StorageResponse<ItemSnapshot>(task.Result.error);
                
                Item = task.Result.data;

                return new StorageResponse<ItemSnapshot>(new ItemSnapshot(task.Result.data, _storageContext, _metadata));
            });
        }

        /// <summary>
        /// Decrements the value of an items attribute. If the attribute doesn't exist, it is set to zero before the operation.
        /// </summary>
        /// <param name="property">The name of the item's attribute.</param>
        /// <param name="value">The number to subtract.</param>
        /// <returns>This item reference.</returns>
        public UnityTask<StorageResponse<ItemSnapshot>> Decr(string property, int value)
        {
            return _storageContext.Repository.Decr(Item, property, value).ConvertTo(task =>
            {
                if (task.IsFaulted)
                    return new StorageResponse<ItemSnapshot>(task.Result.error);

                Item = task.Result.data;

                return new StorageResponse<ItemSnapshot>(new ItemSnapshot(task.Result.data, _storageContext, _metadata));
            });
        }

        #endregion

        #region Notifications

        private class ItemNotification<TK>
        {
            // ReSharper disable InconsistentNaming
            // ReSharper disable UnusedAutoPropertyAccessor.Local
            // ReSharper disable once MemberCanBePrivate.Local
            public string type { get; set; }
            public TK data { get; set; }

            public StorageEventType eventType
            {
                get
                {
                    return (StorageEventType)Enum.Parse(typeof(StorageEventType), type, true);
                }
            }
            // ReSharper restore InconsistentNaming
            // ReSharper restore UnusedAutoPropertyAccessor.Local
        }

        private void NotifyListeners(StorageNotificationType notificationType, IDictionary<StorageNotificationType, List<Action<ItemSnapshot>>> notification, ItemSnapshot itemSnapshot)
        {
            if (notification.ContainsKey(notificationType))
            {
                var events = notification[notificationType];
                foreach (var evt in events)
                    evt(itemSnapshot);

                if (notificationType == StorageNotificationType.ONCE)
                    notification[notificationType].Clear();
            }
        }

        private void Subscribe()
        {
            // Note :
            // Anonymous methods can cause errors when removing notification
            _storageContext.AddNotification(_channel, OnMessage);
        }

        void OnMessage(string m)
        {
            var changedItem = JsonMapper.ToObject<ItemNotification<T>>(m);

            // notify listeners
            if (_notifications.ContainsKey(changedItem.eventType))
            {
                var itemSnapshot = new ItemSnapshot(changedItem.data, _storageContext, _metadata);
                var notification = _notifications[changedItem.eventType];

                NotifyListeners(StorageNotificationType.ON, notification, itemSnapshot);
                NotifyListeners(StorageNotificationType.ONCE, notification, itemSnapshot);
                NotifyListeners(StorageNotificationType.DELETE, notification, itemSnapshot);
            }
        }

        /// <summary>
        /// Attach a listener to run every time the eventType occurs.
        /// </summary>
        /// <param name="eventType">The type of the event to listen. Possible values: put, update, delete.</param>
        /// <param name="handler">The function to run whenever the notification is received. If the event type is "put", it will immediately trigger a "getItems" to get the initial data and run the callback with each item snapshot as argument.</param>
        /// <param name="onError">Response if an error was returned from the server.</param>
        /// <returns>This item reference.</returns>
        public ItemRef<T> On(StorageEventType eventType, Action<ItemSnapshot> handler, Action<StorageError> onError = null)
        {
            // TODO : Why ?
            // if (eventType == Storage.EventType.PUT)
            //   this.Get(handler, onError);

            _notifications[eventType][StorageNotificationType.ON].Add(handler);

            Subscribe();
            return this;
        }

        /// <summary>
        /// Attach a listener to run only once the event type occurs.
        /// </summary>
        /// <param name="eventType">The type of the event to listen. Possible values: put, update, delete.</param>
        /// <param name="handler">The function invoked, only once, when the notification is received. If the event type is "put", it will immediately trigger a "getItems" to get the initial data and run the callback with each item snapshot as argument.</param>
        /// <param name="onError">Response if an error was returned from the server.</param>
        /// <returns>This item reference.</returns>
        public ItemRef<T> Once(StorageEventType eventType, Action<ItemSnapshot> handler, Action<StorageError> onError = null)
        {
            // TODO : Why ?
            // if (eventType == Storage.EventType.PUT)
            //    this.Get(handler, onError);

            _notifications[eventType][StorageNotificationType.ONCE].Add(handler);

            Subscribe();
            return this;
        }

        /// <summary>
        /// Remove all event handlers by its event type.
        /// </summary>
        /// <param name="eventType">The type of the event to remove. Possible values: put, update, delete. If not specified, it will remove all listeners of this reference.</param>
        /// <returns>This item reference.</returns>
        public ItemRef<T> Off(StorageEventType eventType)
        {
            _notifications[eventType][StorageNotificationType.ON].Clear();
            _notifications[eventType][StorageNotificationType.ONCE].Clear();
            _notifications[eventType][StorageNotificationType.DELETE].Clear();

            return this;
        }

        /// <summary>
        /// Remove all event handlers.
        /// </summary>
        /// <returns>This item reference.</returns>
        public ItemRef<T> Off()
        {
            Off(StorageEventType.DELETE);
            Off(StorageEventType.PUT);
            Off(StorageEventType.UPDATE);

            return this;
        }

        #endregion
    }
}