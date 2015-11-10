// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Realtime.Messaging
{
    #region Delegates

    /// <summary>
    ///     Occurs when the client connects to the gateway.
    /// </summary>
    public delegate void OnConnectedDelegate();

    /// <summary>
    ///     Occurs when the client disconnects from the gateway.
    /// </summary>
    public delegate void OnDisconnectedDelegate();

    /// <summary>
    ///     Occurs when the client subscribed to a channel.
    /// </summary>
    public delegate void OnSubscribedDelegate(string channel);

    /// <summary>
    ///     Occurs when the client unsubscribed from a channel.
    /// </summary>
    public delegate void OnUnsubscribedDelegate(string channel);

    /// <summary>
    ///     Occurs when there is an exception.
    /// </summary>
    public delegate void OnExceptionDelegate(Exception ex);

    /// <summary>
    ///     Occurs when the client attempts to reconnect to the gateway.
    /// </summary>
    public delegate void OnReconnectingDelegate();

    /// <summary>
    ///     Occurs when the client reconnected to the gateway.
    /// </summary>
    public delegate void OnReconnectedDelegate();

    /// <summary>
    ///     Occurs when the client receives a message in the specified channel.
    /// </summary>
    public delegate void OnMessageDelegate(string channel, string message);

    #endregion

    /// <summary>
    ///     Represents a <see cref="OrtcClient" /> that connects to a specified gateway.
    /// </summary>
    public abstract class OrtcClient
    {
        #region Constants (4)

        // ReSharper disable InconsistentNaming

        /// <summary>
        ///     Message maximum size in bytes
        /// </summary>
        /// <exclude />
        public const int MAX_MESSAGE_SIZE = 700;

        /// <summary>
        ///     Channel maximum size in bytes
        /// </summary>
        /// <exclude />
        public const int MAX_CHANNEL_SIZE = 100;

        /// <summary>
        ///     Connection Metadata maximum size in bytes
        /// </summary>
        /// <exclude />
        public const int MAX_CONNECTION_METADATA_SIZE = 256;

        /// <summary>
        ///     Session storage name
        /// </summary>
        public const string SESSION_STORAGE_NAME = "ortcsession";

        protected const int HEARTBEAT_MAX_TIME = 60;
        protected const int HEARTBEAT_MIN_TIME = 10;
        protected const int HEARTBEAT_MAX_FAIL = 6;
        protected const int HEARTBEAT_MIN_FAIL = 1;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the client object identifier.
        /// </summary>
        /// <value>Object identifier.</value>
        public string Id { get; protected set; }

        /// <summary>
        ///     Gets or sets the session id.
        /// </summary>
        /// <value>
        ///     The session id.
        /// </value>
        public string SessionId { get; protected set; }

        /// <summary>
        ///     Gets a value indicating whether this client object is connected.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this client is connected; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnecting { get; protected set; }

        /// <summary>
        ///     Gets a value indicating whether this client object is connecting
        /// </summary>
        /// <value>
        ///     <c>true</c> if this client is connecting; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected { get; protected set; }

        /// <summary>
        ///     Gets a value indicating whether this instance is clustered.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is cluster; otherwise, <c>false</c>.
        /// </value>
        public bool IsCluster { get; protected set; }

        /// <summary>
        ///     Gets or sets a value indicating how many times can the client fail the heartbeat.
        /// </summary>
        /// <value>
        ///     Failure limit.
        /// </value>
        public abstract int HeartbeatFails { get; set; }

        /// <summary>
        ///     Gets or sets the gateway URL.
        /// </summary>
        /// <value>Gateway URL where the socket is going to connect.</value>
        public abstract string Url { get; set; }

        /// <summary>
        ///     Gets or sets the cluster gateway URL.
        /// </summary>
        public abstract string ClusterUrl { get; set; }

        /// <summary>
        ///     Gets or sets the connection timeout. Default value is 5000 miliseconds.
        /// </summary>
        public abstract int ConnectionTimeout { get; set; }

        /// <summary>
        ///     Gets or sets the client connection metadata.
        /// </summary>
        public abstract string ConnectionMetadata { get; set; }

        /// <summary>
        ///     Gets or sets the client announcement subchannel.
        /// </summary>
        public abstract string AnnouncementSubChannel { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this client has a heartbeat activated.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the heartbeat is active; otherwise, <c>false</c>.
        /// </value>
        public abstract bool HeartbeatActive { get; set; }

        /// <summary>
        ///     Gets or sets the heartbeat interval.
        /// </summary>
        /// <value>
        ///     Interval in seconds between heartbeats.
        /// </value>
        public abstract int HeartbeatTime { get; set; }

        /// <summary>
        ///     Enables / Disables automatic reconnect on disconnect.
        /// </summary>
        /// <value>
        ///     true if enabled
        /// </value>
        public abstract bool EnableReconnect { get; set; }
        #endregion

        #region Events (7)


        /// <summary>
        ///     Occurs when a connection attempt was successful.
        /// </summary>
        public abstract event OnConnectedDelegate OnConnected;


        /// <summary>
        ///     Occurs when the client connection terminated.
        /// </summary>
        public abstract event OnDisconnectedDelegate OnDisconnected;


        /// <summary>
        ///     Occurs when the client subscribed to a channel.
        /// </summary>
        public abstract event OnSubscribedDelegate OnSubscribed;

        /// <summary>
        ///     Occurs when the client unsubscribed from a channel.
        /// </summary>
        public abstract event OnUnsubscribedDelegate OnUnsubscribed;

        /// <summary>
        ///     Occurs when there is an error.
        /// </summary>
        public abstract event OnExceptionDelegate OnException;

        /// <summary>
        ///     Occurs when a client attempts to reconnect.
        /// </summary>
        public abstract event OnReconnectingDelegate OnReconnecting;

        /// <summary>
        ///     Occurs when a client reconnected.
        /// </summary>
        public abstract event OnReconnectedDelegate OnReconnected;

        #endregion

        #region Methods

        /// <summary>
        ///     Connects to the gateway with the application key and authentication token. The gateway must be set before using
        ///     this method.
        /// </summary>
        /// <param name="applicationKey">Your application key to use ORTC.</param>
        /// <param name="authenticationToken">Authentication token that identifies your permissions.</param>
        /// <example>
        ///     <code>
        /// ortcClient.Connect("myApplicationKey", "myAuthenticationToken");
        ///   </code>
        /// </example>
        public abstract void Connect(string applicationKey, string authenticationToken);

        /// <summary>
        ///     Disconnects from the gateway.
        /// </summary>
        /// <example>
        ///     <code>
        /// ortcClient.Disconnect();
        ///   </code>
        /// </example>
        public abstract void Disconnect();

        /// <summary>
        ///     Subscribes to a channel.
        /// </summary>
        /// <param name="channel">Channel name.</param>
        /// <param name="onMessage"><see cref="OnMessageDelegate" /> callback.</param>
        /// <example>
        ///     <code>
        /// ortcClient.Subscribe("channelName", true, OnMessageCallback);
        /// private void OnMessageCallback(object sender, string channel, string message)
        /// {
        /// // Do something
        /// }
        ///   </code>
        /// </example>
        public abstract void Subscribe(string channel, OnMessageDelegate onMessage);

        /// <summary>
        ///     Unsubscribes from a channel.
        /// </summary>
        /// <param name="channel">Channel name.</param>
        /// <example>
        ///     <code>
        /// ortcClient.Unsubscribe("channelName");
        ///   </code>
        /// </example>
        public abstract void Unsubscribe(string channel);

        /// <summary>
        ///     Sends a message to a channel.
        /// </summary>
        /// <param name="channel">Channel name.</param>
        /// <param name="message">Message to be sent.</param>
        /// <example>
        ///     <code>
        /// ortcClient.Send("channelName", "messageToSend");
        ///   </code>
        /// </example>
        public abstract void Send(string channel, string message);

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="applicationKey"></param>
        /// <param name="privateKey"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public abstract void SendProxy(string applicationKey, string privateKey, string channel, string message);

        /// <summary>
        ///     Indicates whether is subscribed to a channel.
        /// </summary>
        /// <param name="channel">The channel name.</param>
        /// <returns>
        ///     <c>true</c> if subscribed to the channel; otherwise, <c>false</c>.
        /// </returns>
        public abstract bool IsSubscribed(string channel);


        /// <summary>
        ///     Reads the SessionID from local storage.
        /// </summary>
        /// <param name="applicationKey">The application key.</param>
        /// <param name="sessionExpirationTime">The session expiration time.</param>
        /// <returns></returns>
        public virtual string ReadLocalStorage(string applicationKey, int sessionExpirationTime)
        {
            return string.Empty;
        }

        /// <summary>
        ///    Writes the Session ID to the local storage.
        /// </summary>
        /// <param name="applicationKey">The application key.</param>
        public virtual void CreateLocalStorage(string applicationKey)
        {
        }

        /// <summary>
        ///     Splits the message to conform with MAX_MESSAGE_SIZE
        /// </summary>
        /// <param name="channelBytes"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected virtual List<string> SplitMessage(byte[] channelBytes, string message)
        {
            message = message.Replace(Environment.NewLine, "\n");

            var messageBytes = Encoding.UTF8.GetBytes(message);
            var messageParts = new List<string>();
            var pos = 0;
            int remaining;

            // Multi part
            while ((remaining = messageBytes.Length - pos) > 0)
            {
                var messagePart = remaining >= MAX_MESSAGE_SIZE - channelBytes.Length ? new byte[MAX_MESSAGE_SIZE - channelBytes.Length] : new byte[remaining];

                Array.Copy(messageBytes, pos, messagePart, 0, messagePart.Length);

#if UNITY_WSA
                messageParts.Add(Encoding.UTF8.GetString(messagePart, 0, messagePart.Length));
#else
                messageParts.Add(Encoding.UTF8.GetString(messagePart));
#endif

                pos += messagePart.Length;
            }

            return messageParts;
        }

        #endregion
    }
}
