// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------
namespace Realtime.Storage.Models
{
    /// <summary>
    /// Response from the service
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StorageResponse<T>
    {
        // ReSharper disable InconsistentNaming

        /// <summary>
        /// Requested Data
        /// </summary>
        public T data { get; set; }

        /// <summary>
        /// Error (if any)
        /// </summary>
        public StorageError error { get; set; }
        
        /// <summary>
        /// error is not null
        /// </summary>
        public bool hasError
        {
            get { return error != null; }
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="data"></param>
        public StorageResponse(T data)
        {
            this.data = data;
        }

        /// <summary>
        /// ctor
        /// </summary>
        public StorageResponse(StorageError error)
        {
            this.error = error;
        }

        /// <summary>
        /// ctor
        /// </summary>
        public StorageResponse()
        {
            
        }
    }
}