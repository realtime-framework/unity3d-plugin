// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System;
using System.Net;
using Foundation.Tasks;

namespace Realtime.Messaging.Internal
{
    /// <summary>
    /// Extends Task with Web Values
    /// </summary>
    public class HttpTask : UnityTask
    {
        /// <summary>
        /// Computed from WebResponse
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// HTTP Status Code
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public HttpTask()
        {
            Strategy = TaskStrategy.Custom;
        }

        public HttpTask(Exception ex)
        {
            Strategy = TaskStrategy.Custom;
            Exception = ex;
            Status = TaskStatus.Faulted;
            StatusCode = HttpStatusCode.BadRequest;
        }
    }
}