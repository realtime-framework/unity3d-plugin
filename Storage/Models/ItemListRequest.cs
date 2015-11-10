// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------
using System;
using System.Collections.Generic;

namespace Realtime.Storage.Models
{
    /// <summary>
    /// Request for a listing of items
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ItemListRequest<T> where T : class
    {
        // ReSharper disable InconsistentNaming
        protected internal bool _searchForward = true;
        protected internal readonly List<Filter> _filters = new List<Filter>();
        protected internal readonly List<string> _properties = new List<string>();
        protected internal DataKey _startKey;
        protected internal DataKey _datakey;
        protected internal int _limit = 0;
        
        #region Primary

        /// <summary>
        /// Adds property truncation
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public ItemListRequest<T> WithProperty(string attribute)
        {
            _properties.Add(attribute);
            return this;
        }

        /// <summary>
        /// adds property truncation
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public ItemListRequest<T> WithProperties(IEnumerable<string> attribute)
        {
            _properties.AddRange(attribute);
            return this;
        }
        /// <summary>
        /// The primary key of the item from which to continue an earlier operation. This value is returned in the stopKey if that operation was interrupted before completion; either because of the result set size or because of the setting for limit.
        /// </summary>
        public ItemListRequest<T> WithStartKey(object primaryKey)
        {
            _startKey = new DataKey(primaryKey);
            return this;
        }
        
        /// <summary>
        /// The primary key of the item from which to continue an earlier operation. This value is returned in the stopKey if that operation was interrupted before completion; either because of the result set size or because of the setting for limit.
        /// </summary>
        public ItemListRequest<T> WithStartKey(object primaryKey, object secondaryKey)
        {
            _startKey = new DataKey(primaryKey, secondaryKey);
            return this;
        }

        /// <summary>
        /// The primary key of the item from which to continue an earlier operation. This value is returned in the stopKey if that operation was interrupted before completion; either because of the result set size or because of the setting for limit.
        /// </summary>
        public ItemListRequest<T> WithStartKey(DataKey startKey)
        {
            _startKey = startKey;
            return this;
        }

        /// <summary>
        /// The maximum number of items to evaluate (not necessarily the number of matching items).
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public ItemListRequest<T> Limit(int count)
        {
            _limit = count;
            return this;
        }

        /// <summary>
        /// Defines if the items will be retrieved in ascending order.
        /// </summary>
        /// <returns>This table reference.</returns>
        public ItemListRequest<T> Asc()
        {
            _searchForward = true;
            return this;
        }

        /// <summary>
        /// Defines if the items will be retrieved in descending order.
        /// </summary>
        /// <returns>This table reference.</returns>
        public ItemListRequest<T> Desc()
        {
            _searchForward = false;
            return this;
        }
        #endregion

        #region Filters


        /// <summary>
        /// Applies a filter that, upon retrieval, will have the items that have the selected property with a value other than null.
        /// </summary>
        /// <param name="attributeName">The name of the item's property.</param>
        /// <returns>This table reference.</returns>
        public ItemListRequest<T> NotNull(string attributeName)
        {
            _filters.Add(new Filter
            {
                item = attributeName,
                value = null,
                op = FilterOperator.notNull
            });

            return this;
        }

        /// <summary>
        /// Applies a filter that, upon retrieval, will have the items that have the selected property with a null value.
        /// </summary>
        /// <param name="attributeName">The name of the item's property.</param>
        /// <returns>This table reference.</returns>
        public ItemListRequest<T> IsNull(string attributeName)
        {
            _filters.Add(new Filter
            {
                item = attributeName,
                value = null,
                op = FilterOperator.@null
            });

            return this;
        }

        /// <summary>
        /// Applies a filter to the table. When fetched, it will return the items that match the filter property value.
        /// </summary>
        /// <param name="attributeName">The name of the item's property.</param>
        /// <param name="value">The value of the property to be matched.</param>
        /// <returns>This table reference.</returns>
        public ItemListRequest<T> IsEquals(string attributeName, Object value)
        {
            _filters.Add(new Filter
            {
                item = attributeName,
                value = value,
                op = FilterOperator.equals
            });

            return this;
        }

        /// <summary>
        /// Applies a filter to the table. When fetched, it will return the items that do not match the filter property value.
        /// </summary>
        /// <param name="attributeName">The name of the item's property.</param>
        /// <param name="value">The value of the property to filter.</param>
        /// <returns>This table reference.</returns>
        public ItemListRequest<T> NotEquals(string attributeName, Object value)
        {
            _filters.Add(new Filter
            {
                item = attributeName,
                value = value,
                op = FilterOperator.notEqual
            });

            return this;
        }

        /// <summary>
        /// Applies a filter to the table. When fetched, it will return the items greater or equal to filter property value.
        /// </summary>
        /// <param name="attributeName">The name of the item's property.</param>
        /// <param name="value">The value of the property to filter.</param>
        /// <returns>This table reference.</returns>
        public ItemListRequest<T> GreaterEqual(string attributeName, Object value)
        {
            _filters.Add(new Filter
            {
                item = attributeName,
                value = value,
                op = FilterOperator.greaterEqual
            });

            return this;
        }

        /// <summary>
        /// Applies a filter to the table. When fetched, it will return the items greater than the filter property value.
        /// </summary>
        /// <param name="attributeName">The name of the item's property.</param>
        /// <param name="value">The value of the property to filter.</param>
        /// <returns>This table reference.</returns>
        public ItemListRequest<T> GreaterThan(string attributeName, Object value)
        {
            _filters.Add(new Filter
            {
                item = attributeName,
                value = value,
                op = FilterOperator.greaterThan
            });

            return this;
        }

        /// <summary>
        /// Applies a filter to the table. When fetched, it will return the items lesser or equals to the filter property value.
        /// </summary>
        /// <param name="attributeName">The name of the item's property.</param>
        /// <param name="value">The value of the property to filter.</param>
        ///  <returns>This table reference.</returns>
        public ItemListRequest<T> LesserEqual(string attributeName, Object value)
        {
            _filters.Add(new Filter
            {
                item = attributeName,
                value = value,
                op = FilterOperator.lessEqual
            });

            return this;
        }

        /// <summary>
        /// Applies a filter to the table. When fetched, it will return the items lesser than the filter property value.
        /// </summary>
        /// <param name="attributeName">The name of the item's property.</param>
        /// <param name="value">The value of the property to filter.</param>
        /// <returns>This table reference.</returns>
        public ItemListRequest<T> LesserThan(string attributeName, Object value)
        {
            _filters.Add(new Filter
            {
                item = attributeName,
                value = value,
                op = FilterOperator.lessThan
            });

            return this;
        }

        /// <summary>
        /// Applies a filter to the table. When fetched, it will return the items that contains the filter property value.
        /// </summary>
        /// <param name="attributeName">The name of the item's property.</param>
        /// <param name="value">The value of the property to filter.</param>
        /// <returns>This table reference.</returns>
        public ItemListRequest<T> Contains(string attributeName, Object value)
        {
            _filters.Add(new Filter
            {
                item = attributeName,
                value = value,
                op = FilterOperator.contains
            });

            return this;
        }

        /// <summary>
        /// Applies a filter to the table. When fetched, it will return the items that do not contain the filter property value.
        /// </summary>
        /// <param name="attributeName">The name of the item's property.</param>
        /// <param name="value">The value of the property to filter.</param>
        ///   <returns>This table reference.</returns>
        public ItemListRequest<T> NotContains(string attributeName, Object value)
        {
            _filters.Add(new Filter
            {
                item = attributeName,
                value = value,
                op = FilterOperator.notContains
            });

            return this;
        }

        /// <summary>
        /// Applies a filter to the table. When fetched, it will return the items that begins with the filter property value.
        /// </summary>
        /// <param name="attributeName">The name of the item's property.</param>
        /// <param name="value">The value of the property to filter.</param>
        /// <returns>This table reference.</returns>
        public ItemListRequest<T> BeginsWith(string attributeName, Object value)
        {
            _filters.Add(new Filter
            {
                item = attributeName,
                value = value,
                op = FilterOperator.beginsWith
            });

            return this;
        }

        /// <summary>
        /// Applies a filter to the table. When fetched, it will return the items in range of the filter property value.
        /// </summary>
        /// <param name="attributeName">The name of the item's property.</param>
        /// <param name="startValue"></param>
        /// <param name="endValue"></param>
        /// <returns>This table reference.</returns>
        public ItemListRequest<T> Between(string attributeName, Object startValue, Object endValue)
        {
            _filters.Add(new BetweenFilter
            {
                item = attributeName,
                value = startValue,
                endvalue = endValue,
                op = FilterOperator.between
            });

            return this;
        }
        #endregion
    }
}