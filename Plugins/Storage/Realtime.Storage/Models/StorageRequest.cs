// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------
namespace Realtime.Storage.Models
{
    /// <summary>
    /// Used to create a Json Request without depending on ExpandoObjects
    /// </summary>
    internal class StorageRequest
    {
        // ReSharper disable InconsistentNaming

        public object applicationKey { get; set; }
        public object privateKey { get; set; }
        public object authenticationToken { get; set; }
        public object table { get; set; }
        public object key { get; set; }
        public object item { get; set; }
        public object property { get; set; }
        public object value { get; set; }
        public object properties { get; set; }
        public object startKey { get; set; }
        public object limit { get; set; }
        public object filter { get; set; }

        public object provisionType { get; set; }
        public object provisionLoad { get; set; }
        public object throughput { get; set; }
        public object startTable { get; set; }

        public object timeout { get; set; }
        public object roles { get; set; }
        public object role { get; set; }
        public object policies { get; set; }
    }
}