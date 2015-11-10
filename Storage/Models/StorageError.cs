// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------
namespace Realtime.Storage.Models
{
    /// <summary>
    /// An exception thrown by the StorageRepository.
    /// Includes a error code
    /// </summary>
    public class StorageError 
    {
        /// <summary>
        /// The error code
        /// </summary>
        /// <remarks>
        /// TODO replace with ENUM
        /// </remarks>
        public int code { get; set; }

        /// <summary>
        /// The error reason
        /// </summary>
        public string message { get; set; }


        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="c"></param>
        /// <param name="m"></param>
        public StorageError(int c, string m)
        {
            code = c;
            message = m;
        }

        public StorageError()
        {
            
        }
    }
}