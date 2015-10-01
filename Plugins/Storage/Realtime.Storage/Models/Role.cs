// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------
namespace Realtime.Storage.Models
{
    /// <summary>
    /// Security Role
    /// </summary>
    public class Role
    {
        /// <summary>
        /// role name
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// policies of the role
        /// </summary>
        public Policy policies { get; set; }
    }
}