// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------

using System;
using System.Collections;
using System.Linq;
using System.Net;
using Foundation.Tasks;
using Realtime.Messaging.Internal;

namespace Realtime.Messaging
{

    /// <summary>
    /// Http Client for messaging
    /// </summary>
    public class MessageClient
    {
        #region static
        static HttpTaskService Client = new HttpTaskService("application/x-www-form-urlencoded");

        /// <summary> 
        /// Sends a message to a channel.
        /// </summary>
        /// <param name="url">ORTC server URL.</param>
        /// <param name="isCluster">Indicates whether the ORTC server is in a cluster.</param>
        /// <param name="authenticationToken">Authentication Token which is generated by the application server, for instance a unique session ID.</param>
        /// <param name="applicationKey">Application Key that was provided to you together with the ORTC service purchasing.</param>
        /// <param name="privateKey">The private key provided to you together with the ORTC service purchasing.</param>
        /// <param name="channel">The channel where the message will be sent.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>True if the send was successful or false if it was not.</returns>
        public static UnityTask<bool> SendMessage(string url, bool isCluster, string authenticationToken, string applicationKey, string privateKey, string channel, string message)
        {
            var client = new MessageClient(url, isCluster, authenticationToken, applicationKey, privateKey, channel, message);
            return client.SendMessage();
        }
        #endregion

        #region public
        /// <summary>
        /// ORTC server URL.
        /// </summary>
        public string Url;
        /// <summary>
        /// Indicates whether the ORTC server is in a cluster.
        /// </summary>
        public bool IsCluster;
        /// <summary>
        /// Authentication Token which is generated by the application server, for instance a unique session ID.
        /// </summary>
        public string AuthenticationToken;
        /// <summary>
        /// Application Key that was provided to you together with the ORTC service purchasing.
        /// </summary>
        public string ApplicationKey;
        /// <summary>
        /// The private key provided to you together with the ORTC service purchasing.
        /// </summary>
        public string PrivateKey;
        /// <summary>
        /// The channel where the message will be sent.
        /// </summary>
        public string Channel;
        /// <summary>
        /// The message to send.
        /// </summary>
        public string Message;

        public MessageClient()
        {

        }

        /// <summary> 
        /// creates a new client to send a message to a channel.
        /// </summary>
        /// <param name="url">ORTC server URL.</param>
        /// <param name="isCluster">Indicates whether the ORTC server is in a cluster.</param>
        /// <param name="authenticationToken">Authentication Token which is generated by the application server, for instance a unique session ID.</param>
        /// <param name="applicationKey">Application Key that was provided to you together with the ORTC service purchasing.</param>
        /// <param name="privateKey">The private key provided to you together with the ORTC service purchasing.</param>
        /// <param name="channel">The channel where the message will be sent.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>True if the send was successful or false if it was not.</returns>
        public MessageClient(string url, bool isCluster, string authenticationToken, string applicationKey, string privateKey, string channel, string message)
        {
            Url = url;
            IsCluster = isCluster;
            AuthenticationToken = authenticationToken;
            ApplicationKey = applicationKey;
            PrivateKey = privateKey;
            Channel = channel;
            Message = message;
        }

        /// <summary> 
        /// Sends a message to a channel.
        /// </summary>
        public UnityTask<bool> SendMessage()
        {
            if (String.IsNullOrEmpty(Url))
            {
                UnityTask.FailedTask<bool>(new OrtcException(OrtcExceptionReason.InvalidArguments, "Server URL can not be null or empty."));
            }

            return UnityTask.RunCoroutine<bool>(SendMessageAsync);
        }
        #endregion

        #region private

        IEnumerator SendMessageAsync(UnityTask<bool> task)
        {
            var connectionUrl = Url;

            if (IsCluster)
            {
                var cTask = ClusterClient.GetClusterServerWithRetry(Url, ApplicationKey);
                yield return TaskManager.StartRoutine(cTask.WaitRoutine());

                if (cTask.IsFaulted)
                {
                    task.Exception = cTask.Exception;
                    task.Status = TaskStatus.Faulted;
                    yield break;
                }

                connectionUrl = cTask.Result;
            }

            connectionUrl = connectionUrl.Last() == '/' ? connectionUrl : connectionUrl + "/";

            var postParameters = String.Format("AT={0}&AK={1}&PK={2}&C={3}&M={4}", AuthenticationToken, ApplicationKey, PrivateKey, Channel, HttpUtility.UrlEncode(Message));

            var hTask = Client.PostAsync(String.Format("{0}send", connectionUrl), postParameters);

            yield return TaskManager.StartRoutine(hTask.WaitRoutine());

            if (hTask.IsFaulted)
            {
                task.Exception = hTask.Exception;
                task.Status = TaskStatus.Faulted;
                yield break;
            }

            task.Result = hTask.StatusCode == HttpStatusCode.Created;
        }
        #endregion


    }
}