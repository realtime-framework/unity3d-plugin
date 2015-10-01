// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------
namespace Realtime.Storage.Models
{
    /// <summary>
    /// Describes the service endpoint. A Host path with IsCluster / IsSecure option
    /// </summary>
    public class UriPrototype
    {
        /// <summary>
        /// The Url
        /// </summary>
        public string Url;
        /// <summary>
        /// Is Clustered
        /// </summary>
        public bool IsCluster;
        /// <summary>
        /// Use Https
        /// </summary>
        public bool IsSecure;
    }
}