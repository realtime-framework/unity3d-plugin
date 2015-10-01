// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------
using System.Collections.Generic;

namespace Realtime.Storage.Models
{

    // ReSharper disable InconsistentNaming

    /// <summary>
    /// Request for a ordered Query of item's from the repository
    /// </summary>
    /// <typeparam name="T">The Type of the Item</typeparam>
    public class ItemQueryRequest<T> where T : class
    {
        internal bool _searchForward = true;
        internal Filter _filter;
        internal List<string> _properties;
        internal DataKey _startKey;
        internal DataKey _datakey;
        internal int _limit = 0;

        #region ctor

        /// <summary>
        /// creates a new query for the primary key
        /// </summary>
        /// <param name="primaryKey"></param>
        public ItemQueryRequest(object primaryKey)
        {
            _datakey = new DataKey(primaryKey);
        }

        #endregion

        #region Filters

        /// <summary>
        /// Adds property truncation
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public ItemQueryRequest<T> WithProperty(string attribute)
        {
            if(_properties == null)
                _properties = new List<string>();

            _properties.Add(attribute);
            return this;
        }

        /// <summary>
        /// adds property truncation
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public ItemQueryRequest<T> WithProperties(IEnumerable<string> attribute)
        {
            if (_properties == null)
                _properties = new List<string>();

            _properties.AddRange(attribute);
            return this;
        }

        /// <summary>
        /// The primary key of the item from which to continue an earlier operation. This value is returned in the stopKey if that operation was interrupted before completion; either because of the result set size or because of the setting for limit.
        /// </summary>
        public ItemQueryRequest<T> WithStartKey(object primaryKey)
        {
            _startKey = new DataKey(primaryKey);
            return this;
        }

        /// <summary>
        /// The primary key of the item from which to continue an earlier operation. This value is returned in the stopKey if that operation was interrupted before completion; either because of the result set size or because of the setting for limit.
        /// </summary>
        public ItemQueryRequest<T> WithStartKey(object primaryKey, object secondaryKey)
        {
            _startKey = new DataKey(primaryKey, secondaryKey);
            return this;
        }

        /// <summary>
        /// The primary key of the item from which to continue an earlier operation. This value is returned in the stopKey if that operation was interrupted before completion; either because of the result set size or because of the setting for limit.
        /// </summary>
        public ItemQueryRequest<T> WithStartKey(DataKey startKey)
        {
            _startKey = startKey;
            return this;
        }

        /// <summary>
        /// The maximum number of items to evaluate (not necessarily the number of matching items).
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public ItemQueryRequest<T> Limit(int count)
        {
            _limit = count;
            return this;
        }
        #endregion

        #region Filters

        /// <summary>
        /// Defines if the items will be retrieved in ascending order.
        /// </summary>
        /// <returns>This table reference.</returns>
        public ItemQueryRequest<T> Asc()
        {
            _searchForward = true;
            return this;
        }

        /// <summary>
        /// Defines if the items will be retrieved in descending order.
        /// </summary>
        /// <returns>This table reference.</returns>
        public ItemQueryRequest<T> Desc()
        {
            _searchForward = false;
            return this;
        }


        #region Filters
        
        /// <summary>
        /// Applies a filter that, upon retrieval, will have the items that have the selected property with a value other than null.
        /// </summary>
        /// <returns>This table reference.</returns>
        public ItemQueryRequest<T> NotNull(string attributeName)
        {
            _filter = new Filter
            {
                
                value = null,
                op = FilterOperator.notNull
            };

            return this;
        }

        /// <summary>
        /// Applies a filter that, upon retrieval, will have the items that have the selected property with a null value.
        /// </summary>
        /// <returns>This table reference.</returns>
        public ItemQueryRequest<T> IsNull(string attributeName)
        {
            _filter = new Filter
            {
                
                value = null,
                op = FilterOperator.@null
            };

            return this;
        }

        /// <summary>
        /// Applies a filter to the table. When fetched, it will return the items that match the filter property value.
        /// </summary>
        /// <param name="value">The value of the property to be matched.</param>
        /// <returns>This table reference.</returns>
        public ItemQueryRequest<T> IsEquals(object value)
        {
            _filter = new Filter
            {
                
                value = value,
                op = FilterOperator.equals
            };

            return this;
        }

        /// <summary>
        /// Applies a filter to the table. When fetched, it will return the items that do not match the filter property value.
        /// </summary>
        /// <param name="value">The value of the property to filter.</param>
        /// <returns>This table reference.</returns>
        public ItemQueryRequest<T> NotEquals(object value)
        {
            _filter = new Filter
            {
                
                value = value,
                op = FilterOperator.notEqual
            };

            return this;
        }

        /// <summary>
        /// Applies a filter to the table. When fetched, it will return the items greater or equal to filter property value.
        /// </summary>
        /// <param name="value">The value of the property to filter.</param>
        /// <returns>This table reference.</returns>
        public ItemQueryRequest<T> GreaterEqual(object value)
        {
            _filter = new Filter
            {
                
                value = value,
                op = FilterOperator.greaterEqual
            };

            return this;
        }

        /// <summary>
        /// Applies a filter to the table. When fetched, it will return the items greater than the filter property value.
        /// </summary>
        /// <param name="value">The value of the property to filter.</param>
        /// <returns>This table reference.</returns>
        public ItemQueryRequest<T> GreaterThan(object value)
        {
            _filter = new Filter
            {
                
                value = value,
                op = FilterOperator.greaterThan
            };

            return this;
        }

        /// <summary>
        /// Applies a filter to the table. When fetched, it will return the items lesser or equals to the filter property value.
        /// </summary>
        /// <param name="value">The value of the property to filter.</param>
        ///  <returns>This table reference.</returns>
        public ItemQueryRequest<T> LesserEqual(object value)
        {
            _filter = new Filter
            {
                
                value = value,
                op = FilterOperator.lessEqual
            };

            return this;
        }

        /// <summary>
        /// Applies a filter to the table. When fetched, it will return the items lesser than the filter property value.
        /// </summary>
        /// <param name="value">The value of the property to filter.</param>
        /// <returns>This table reference.</returns>
        public ItemQueryRequest<T> LesserThan(object value)
        {
            _filter = new Filter
            {
                
                value = value,
                op = FilterOperator.lessThan
            };

            return this;
        }

        /// <summary>
        /// Applies a filter to the table. When fetched, it will return the items that contains the filter property value.
        /// </summary>
        /// <param name="value">The value of the property to filter.</param>
        /// <returns>This table reference.</returns>
        public ItemQueryRequest<T> Contains(object value)
        {
            _filter = new Filter
            {
                
                value = value,
                op = FilterOperator.contains
            };

            return this;
        }

        /// <summary>
        /// Applies a filter to the table. When fetched, it will return the items that do not contain the filter property value.
        /// </summary>
        /// <param name="value">The value of the property to filter.</param>
        ///   <returns>This table reference.</returns>
        public ItemQueryRequest<T> NotContains(object value)
        {
            _filter = new Filter
            {
                
                value = value,
                op = FilterOperator.notContains
            };
            return this;
        }

        /// <summary>
        /// Applies a filter to the table. When fetched, it will return the items that begins with the filter property value.
        /// </summary>
        /// <param name="value">The value of the property to filter.</param>
        /// <returns>This table reference.</returns>
        public ItemQueryRequest<T> BeginsWith(object value)
        {
            _filter = new Filter
            {
                value = value,
                op = FilterOperator.beginsWith
            };

            return this;
        }

        /// <summary>
        /// Applies a filter to the table. When fetched, it will return the items in range of the filter property value.
        /// </summary>
        /// <param name="startValue"></param>
        /// <param name="endValue"></param>
        /// <returns>This table reference.</returns>
        public ItemQueryRequest<T> Between(object startValue, object endValue)
        {
            _filter = new BetweenFilter
            {
                value = startValue,
                endvalue = endValue,
                op = FilterOperator.between
            };

            return this;
        }
        #endregion
        #endregion
    }
}

