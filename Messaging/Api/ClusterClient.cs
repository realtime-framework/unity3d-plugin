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
    /// Http Client for resolving the cluster url
    /// </summary>
    public class ClusterClient
    {
        #region static / const
        const int MaxConnectionAttempts = 10;
        const int RetryThreadSleep = 5;
        const string ResponsePattern = "var SOCKET_SERVER = \"(?<host>.*)\";";
        private static HttpTaskService Client = new HttpTaskService("application/x-www-form-urlencoded");

        /// <summary>
        /// Gets the cluster server.
        /// </summary>
        /// <returns></returns>
        public static UnityTask<string> GetClusterServer(string url, string applicationKey)
        {
            var client = new ClusterClient(url, applicationKey);

            return client.GetClusterServer();
        }

        /// <summary>
        /// Does the get cluster server logic.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="applicationKey"></param>
        /// <returns></returns>
        public static UnityTask<string> GetClusterServerWithRetry(string url, string applicationKey)
        {
            var client = new ClusterClient(url, applicationKey);

            return client.GetClusterServerWithRetry();
        }

        #endregion

        #region public
        /// <summary>
        /// Url for the cluster
        /// </summary>
        public string Url;

        /// <summary>
        /// App key
        /// </summary>
        public string ApplicationKey;

        public ClusterClient()
        {

        }

        public ClusterClient(string url, string applicationKey)
        {
            Url = url;
            ApplicationKey = applicationKey;
        }

        /// <summary>
        /// Gets the cluster server.
        /// </summary>
        /// <returns></returns>
        public UnityTask<string> GetClusterServer()
        {
            return UnityTask.RunCoroutine<string>(GetAsync);
        }

        /// <summary>
        /// Does the get cluster server logic.
        /// </summary>
        /// <returns></returns>
        public UnityTask<string> GetClusterServerWithRetry()
        {
            return UnityTask.RunCoroutine<string>(GetClusterServerWithRetryAsync);
        }
        #endregion

        #region private
        IEnumerator GetClusterServerWithRetryAsync(UnityTask<string> task)
        {
            int currentAttempts;

            for (currentAttempts = 0;currentAttempts <= MaxConnectionAttempts;currentAttempts++)
            {
                var innerTask = GetClusterServer();

                yield return TaskManager.StartRoutine(innerTask.WaitRoutine());

                task.Result = innerTask.Result;

                if (innerTask.IsSuccess && !String.IsNullOrEmpty(task.Result))
                {
                    yield break;
                }

                currentAttempts++;

                if (currentAttempts > MaxConnectionAttempts)
                {
                    task.Exception = new OrtcException(OrtcExceptionReason.ConnectionError, "Unable to connect to the authentication server.");
                    task.Status = TaskStatus.Faulted;
                    yield break;
                }

                yield return TaskManager.WaitForSeconds(RetryThreadSleep);
            }
        }

        IEnumerator GetAsync(UnityTask<string> task)
        {
            var clusterRequestParameter = !String.IsNullOrEmpty(ApplicationKey)
                ? String.Format("appkey={0}", ApplicationKey)
                : String.Empty;

            var clusterUrl = String.Format("{0}{1}?{2}", Url, !String.IsNullOrEmpty(Url) && Url[Url.Length - 1] != '/' ? "/" : String.Empty, clusterRequestParameter);

            var hTask = Client.GetAsync(clusterUrl);

            yield return TaskManager.StartRoutine(hTask.WaitRoutine());

            if (hTask.IsFaulted)
            {
                task.Exception = hTask.Exception;
                task.Status = TaskStatus.Faulted;
                yield break;
            }

            task.Result = ParseResponse(hTask.Content);
        }

        /// <summary>
        /// parses the response
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        static string ParseResponse(string input)
        {
            var match = Regex.Match(input, ResponsePattern);

            return match.Groups["host"].Value;
        }
        #endregion
    }
}