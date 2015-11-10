// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------


namespace Realtime.Storage.Models
{
    /// <summary>
    /// Filter Operaation
    /// </summary>
    public enum FilterOperator
    {
        // ReSharper disable InconsistentNaming

        /// <summary>
        /// @equals
        /// </summary>
        @equals,
        /// <summary>
        /// notEqual
        /// </summary>
        notEqual,
        /// <summary>
        /// lessEqual
        /// </summary>
        lessEqual,
        /// <summary>
        /// lessThan
        /// </summary>
        lessThan,
        /// <summary>
        /// greaterEqual
        /// </summary>
        greaterEqual,
        /// <summary>
        /// greaterThan
        /// </summary>
        greaterThan,
        /// <summary>
        /// notNull
        /// </summary>
        notNull,
        /// <summary>
        /// @null
        /// </summary>
        @null,
        /// <summary>
        /// contains
        /// </summary>
        contains,
        /// <summary>
        /// notContains
        /// </summary>
        notContains,
        /// <summary>
        /// beginsWith
        /// </summary>
        beginsWith,
        /// <summary>
        /// In
        /// </summary>
        @in,
        /// <summary>
        /// Between
        /// </summary>
        between
    }

    /// <summary>
    /// Item Query 
    /// </summary>
    public class Filter
    {
        /// <summary>
        /// filter operation
        /// </summary>
        public FilterOperator op { get; set; }
        /// <summary>
        /// filter column
        /// </summary>
        public string item { get; set; }
        /// <summary>
        /// filter argument
        /// </summary>
        public object value { get; set; }
    }

    /// <summary>
    /// Item Query
    /// </summary>
    public class BetweenFilter : Filter
    {
        /// <summary>
        /// end value for between
        /// </summary>
        public object endvalue { get; set; }
    }
}