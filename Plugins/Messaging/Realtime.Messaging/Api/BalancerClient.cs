// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------

using System;
using System.Collections;
using System.Text.RegularExpressions;
using Foundation.Tasks;
using Realtime.Messaging.Internal;

namespace Realtime.Messaging
{

    /// <summary>
    /// A static class containing all the methods to communicate with the Ortc Balancer 
    /// </summary>
    public class BalancerClient
    {
        #region static / const
        const String BalancerServerPattern = "^var SOCKET_SERVER = \"(?<server>http.*)\";$";

        static HttpTaskService Client = new HttpTaskService("application/x-www-form-urlencoded");

        /// <summary>
        /// Retrieves an Ortc Server url from the Ortc Balancer
        /// </summary>
        /// <param name="balancerUrl">The Ortc Balancer url.</param>
        /// <param name="applicationKey"></param>
        /// <remarks></remarks>
        public static UnityTask<string> GetServerFromBalancer(string balancerUrl, string applicationKey)
        {
            var client = new BalancerClient
            {
                Url = balancerUrl,
                ApplicationKey = applicationKey,
            };
            return client.GetServerFromBalancer();
        }

        /// <summary>
        /// Retrieves an Ortc Server url from the Ortc Balancer
        /// </summary>
        /// <param name="url"></param>
        /// <param name="isCluster"></param>
        /// <param name="applicationKey"></param>
        /// <returns></returns>
        public static UnityTask<string> GetServerUrl(string url, bool isCluster, string applicationKey)
        {
            var client = new BalancerClient
            {
                Url = url,
                IsCluster = isCluster,
                ApplicationKey = applicationKey,
            };
            return client.GetServerUrl();
        }

        #endregion

        #region public
        public string Url;
        public bool IsCluster;
        public string ApplicationKey;

        public BalancerClient()
        {

        }

        /// <summary>
        /// Retrieves an Ortc Server url from the Ortc Balancer
        /// </summary>
        /// <remarks></remarks>
        public UnityTask<string> GetServerFromBalancer()
        {
            return UnityTask.RunCoroutine<string>(GetServerFromBalancerAsync);
        }

        /// <summary>
        /// Retrieves an Ortc Server url from the Ortc Balancer
        /// </summary>
        /// <returns></returns>
        public UnityTask<string> GetServerUrl()
        {
            if (!String.IsNullOrEmpty(Url) && IsCluster)
            {
                return UnityTask.RunCoroutine<string>(GetServerFromBalancerAsync);
            }

            return UnityTask.SuccessTask(Url);
        }

        #endregion

        #region private

        IEnumerator GetServerFromBalancerAsync(UnityTask<string> task)
        {
            var parsedUrl = String.IsNullOrEmpty(ApplicationKey) ? Url : Url + "?appkey=" + ApplicationKey;

            var innerTask = Client.GetAsync(parsedUrl);

            yield return TaskManager.StartRoutine(innerTask.WaitRoutine());

            if (innerTask.IsFaulted)
            {
                task.Exception = innerTask.Exception;
                task.Status = TaskStatus.Faulted;
                yield break;
            }

            task.Result = ParseBalancerResponse(innerTask.Content);
        }

        string ParseBalancerResponse(string responseBody)
        {
            var match = Regex.Match(responseBody, BalancerServerPattern);

            return match.Success ? match.Groups["server"].Value : string.Empty;
        }

        #endregion

    }
}
