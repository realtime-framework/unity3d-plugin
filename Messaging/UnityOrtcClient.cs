// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Foundation.Tasks;
using UnityEngine;
#if UNITY_WSA
using System.Collections.Concurrent;
#else
#endif
using Realtime.Messaging.Internal;

namespace Realtime.Messaging
{
    /// <summary>
    /// IBT Real Time SJ type client.
    /// </summary>
    public class UnityOrtcClient : OrtcClient
    {
        #region Constants (11)

        // REGEX patterns
        private const string OPERATION_PATTERN = @"^a\[""{""op"":""(?<op>[^\""]+)"",(?<args>.*)}""\]$";
        private const string CLOSE_PATTERN = @"^c\[?(?<code>[^""]+),?""?(?<message>.*)""?\]?$";
        private const string VALIDATED_PATTERN = @"^(""up"":){1}(?<up>.*)?,""set"":(?<set>.*)$";
        private const string CHANNEL_PATTERN = @"^""ch"":""(?<channel>.*)""$";
        private const string EXCEPTION_PATTERN = @"^""ex"":{(""op"":""(?<op>[^""]+)"",)?(""ch"":""(?<channel>.*)"",)?""ex"":""(?<error>.*)""}$";
        private const string RECEIVED_PATTERN = @"^a\[""{""ch"":""(?<channel>.*)"",""m"":""(?<message>[\s\S]*)""}""\]$";
        private const string MULTI_PART_MESSAGE_PATTERN = @"^(?<messageId>.[^_]*)_(?<messageCurrentPart>.[^-]*)-(?<messageTotalPart>.[^_]*)_(?<message>[\s\S]*)$";
        private const string PERMISSIONS_PATTERN = @"""(?<key>[^""]+)"":{1}""(?<value>[^,""]+)"",?";

        #endregion

        #region Attributes (17)

        private string _url;
        private string _clusterUrl;
        private string _applicationKey;
        private string _authenticationToken;

        private bool _alreadyConnectedFirstTime;
        private bool _forcedClosed;
        private bool _waitingServerResponse;
        private bool _enableReconnect = true;

        private int _sessionExpirationTime; // minutes

        private List<KeyValuePair<string, string>> _permissions;
        private ConcurrentDictionary<string, ChannelSubscription> _subscribedChannels;
        private ConcurrentDictionary<string, ConcurrentDictionary<int, BufferedMessage>> _multiPartMessagesBuffer;
        private WebSocketConnection _webSocketConnection;

        private TaskTimer _reconnectTimer;
        private TaskTimer _heartbeatTimer;

        #endregion

        #region Properties (9)

        /// <summary>
        /// Gets or sets the gateway URL.
        /// </summary>
        /// <value>Gateway URL where the socket is going to connect.</value>
        public override string Url
        {
            get
            {
                return _url;
            }
            set
            {
                IsCluster = false;
                _url = String.IsNullOrEmpty(value) ? String.Empty : value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the cluster gateway URL.
        /// </summary>
        public override string ClusterUrl
        {
            get
            {
                return _clusterUrl;
            }
            set
            {
                IsCluster = true;
                _clusterUrl = String.IsNullOrEmpty(value) ? String.Empty : value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the heartbeat interval.
        /// </summary>
        /// <value>
        /// Interval in seconds between heartbeats.
        /// </value>
        public override int HeartbeatTime
        {
            get { return _heartbeatTimer.Interval; }
            set { _heartbeatTimer.Interval = value > HEARTBEAT_MAX_TIME ? HEARTBEAT_MAX_TIME : (value < HEARTBEAT_MIN_TIME ? HEARTBEAT_MIN_TIME : value); }
        }

        public override int HeartbeatFails { get; set; }

        public override int ConnectionTimeout { get; set; }

        public override string ConnectionMetadata { get; set; }

        public override string AnnouncementSubChannel { get; set; }

        public override bool HeartbeatActive
        {
            get { return _heartbeatTimer.IsRunning; }
            set
            {
                if (value)
                    _heartbeatTimer.Start();
                else
                    _heartbeatTimer.Stop();
            }
        }
        
        public override bool EnableReconnect
        {
            get { return _enableReconnect; }
            set { _enableReconnect = value; }
        }

        #endregion

        #region Events (7)
        event OnConnectedDelegate _onConnected = delegate { };
        event OnDisconnectedDelegate _onDisconnected = delegate { };
        event OnSubscribedDelegate _onSubscribed = delegate { };
        event OnUnsubscribedDelegate _onUnsubscribed = delegate { };
        event OnExceptionDelegate _onException = delegate { };
        event OnReconnectingDelegate _onReconnecting = delegate { };
        event OnReconnectedDelegate _onReconnected = delegate { };

        /// <summary>
        /// Occurs when a connection attempt was successful.
        /// </summary>
        public override event OnConnectedDelegate OnConnected
        {
            // note : IOS WTF for Unity5
            add
            {
                _onConnected = (OnConnectedDelegate)Delegate.Combine(_onConnected, value);
            }

            remove
            {
                _onConnected = (OnConnectedDelegate)Delegate.Remove(_onConnected, value);
            }
        }

        /// <summary>
        /// Occurs when the client connection terminated. 
        /// </summary>
        public override event OnDisconnectedDelegate OnDisconnected
        {
            add
            {
                _onDisconnected = (OnDisconnectedDelegate)Delegate.Combine(_onDisconnected, value);
            }

            remove
            {
                _onDisconnected = (OnDisconnectedDelegate)Delegate.Remove(_onDisconnected, value);
            }
        }

        /// <summary>
        /// Occurs when the client subscribed to a channel.
        /// </summary>
        public override event OnSubscribedDelegate OnSubscribed
        {
            add
            {
                _onSubscribed = (OnSubscribedDelegate)Delegate.Combine(_onSubscribed, value);
            }

            remove
            {
                _onSubscribed = (OnSubscribedDelegate)Delegate.Remove(_onSubscribed, value);
            }
        }

        /// <summary>
        /// Occurs when the client unsubscribed from a channel.
        /// </summary>
        public override event OnUnsubscribedDelegate OnUnsubscribed
        {
            add
            {
                _onUnsubscribed = (OnUnsubscribedDelegate)Delegate.Combine(_onUnsubscribed, value);
            }

            remove
            {
                _onUnsubscribed = (OnUnsubscribedDelegate)Delegate.Remove(_onUnsubscribed, value);
            }
        }

        /// <summary>
        /// Occurs when there is an error.
        /// </summary>
        public override event OnExceptionDelegate OnException
        {
            add
            {
                _onException = (OnExceptionDelegate)Delegate.Combine(_onException, value);
            }

            remove
            {
                _onException = (OnExceptionDelegate)Delegate.Remove(_onException, value);
            }
        }

        /// <summary>
        /// Occurs when a client attempts to reconnect.
        /// </summary>
        public override event OnReconnectingDelegate OnReconnecting
        {
            add
            {
                _onReconnecting = (OnReconnectingDelegate)Delegate.Combine(_onReconnecting, value);
            }

            remove
            {
                _onReconnecting = (OnReconnectingDelegate)Delegate.Remove(_onReconnecting, value);
            }
        }

        /// <summary>
        /// Occurs when a client reconnected.
        /// </summary>
        public override event OnReconnectedDelegate OnReconnected
        {
            add
            {
                _onReconnected = (OnReconnectedDelegate)Delegate.Combine(_onReconnected, value);
            }

            remove
            {
                _onReconnected = (OnReconnectedDelegate)Delegate.Remove(_onReconnected, value);
            }
        }


        #endregion

        #region Constructor (1)

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityOrtcClient"/> class.
        /// </summary>
        public UnityOrtcClient()
        {
            _heartbeatTimer = new TaskTimer { Interval = 2, AutoReset = true };
            _heartbeatTimer.Elapsed += _heartbeatTimer_Elapsed;
            _heartbeatTimer.Start();

            _reconnectTimer = new TaskTimer { Interval = 2, AutoReset = false };
            _reconnectTimer.Elapsed += _reconnectTimer_Elapsed;

            _permissions = new List<KeyValuePair<string, string>>();
            _subscribedChannels = new ConcurrentDictionary<string, ChannelSubscription>();
            _multiPartMessagesBuffer = new ConcurrentDictionary<string, ConcurrentDictionary<int, BufferedMessage>>();

            // To catch unobserved exceptions
            // TaskScheduler.UnobservedTaskException += new EventHandler<UnobservedTaskExceptionEventArgs>(TaskScheduler_UnobservedTaskException);

            MakeSocket();

        }

        void MakeSocket()
        {
            if (_webSocketConnection != null)
            {
                _webSocketConnection.OnOpened -= _webSocketConnection_OnOpened;
                _webSocketConnection.OnClosed -= _webSocketConnection_OnClosed;
                _webSocketConnection.OnError -= _webSocketConnection_OnError;
                _webSocketConnection.OnMessageReceived -= _webSocketConnection_OnMessageReceived;
                _webSocketConnection.Dispose();
            }

            _webSocketConnection = new WebSocketConnection();

            _webSocketConnection.OnOpened += _webSocketConnection_OnOpened;
            _webSocketConnection.OnClosed += _webSocketConnection_OnClosed;
            _webSocketConnection.OnError += _webSocketConnection_OnError;
            _webSocketConnection.OnMessageReceived += _webSocketConnection_OnMessageReceived;
        }

        #endregion

        #region Public Methods (6)

        /// <summary>
        /// Connects to the gateway with the application key and authentication token. The gateway must be set before using this method.
        /// </summary>
        /// <param name="appKey">Your application key to use ORTC.</param>
        /// <param name="authToken">Authentication token that identifies your permissions.</param>
        /// <example>
        ///   <code>
        /// ortcClient.Connect("myApplicationKey", "myAuthenticationToken");
        ///   </code>
        ///   </example>
        public override void Connect(string appKey, string authToken)
        {

            #region Sanity Checks

            if (IsConnected)
            {
                DelegateExceptionCallback(new OrtcException(OrtcExceptionReason.InvalidArguments, "Already connected"));
            }
            else if (IsConnecting)
            {
                DelegateExceptionCallback(new OrtcException(OrtcExceptionReason.InvalidArguments, "Already trying to connect"));
            }
            else if (String.IsNullOrEmpty(ClusterUrl) && String.IsNullOrEmpty(Url))
            {
                DelegateExceptionCallback(new OrtcException(OrtcExceptionReason.InvalidArguments, "URL and Cluster URL are null or empty"));
            }
            else if (String.IsNullOrEmpty(appKey))
            {
                DelegateExceptionCallback(new OrtcException(OrtcExceptionReason.InvalidArguments, "Application Key is null or empty"));
            }
            else if (String.IsNullOrEmpty(authToken))
            {
                DelegateExceptionCallback(new OrtcException(OrtcExceptionReason.InvalidArguments, "Authentication ToKen is null or empty"));
            }
            else if (!IsCluster && !Url.OrtcIsValidUrl())
            {
                DelegateExceptionCallback(new OrtcException(OrtcExceptionReason.InvalidArguments, "Invalid URL"));
            }
            else if (IsCluster && !ClusterUrl.OrtcIsValidUrl())
            {
                DelegateExceptionCallback(new OrtcException(OrtcExceptionReason.InvalidArguments, "Invalid Cluster URL"));
            }
            else if (!appKey.OrtcIsValidInput())
            {
                DelegateExceptionCallback(new OrtcException(OrtcExceptionReason.InvalidArguments, "Application Key has invalid characters"));
            }
            else if (!authToken.OrtcIsValidInput())
            {
                DelegateExceptionCallback(new OrtcException(OrtcExceptionReason.InvalidArguments, "Authentication Token has invalid characters"));
            }
            else if (AnnouncementSubChannel != null && !AnnouncementSubChannel.OrtcIsValidInput())
            {
                DelegateExceptionCallback(new OrtcException(OrtcExceptionReason.InvalidArguments, "Announcement Subchannel has invalid characters"));
            }
            else if (!String.IsNullOrEmpty(ConnectionMetadata) && ConnectionMetadata.Length > MAX_CONNECTION_METADATA_SIZE)
            {
                DelegateExceptionCallback(new OrtcException(OrtcExceptionReason.InvalidArguments, String.Format("Connection metadata size exceeds the limit of {0} characters", MAX_CONNECTION_METADATA_SIZE)));
            }

            else

            #endregion

            {
                _forcedClosed = false;
                _authenticationToken = authToken;
                _applicationKey = appKey;

                TaskManager.StartRoutine(DoConnect());
            }
        }

        /// <summary>
        /// Sends a message to a channel.
        /// </summary>
        /// <param name="channel">Channel name.</param>
        /// <param name="message">Message to be sent.</param>
        /// <example>
        ///   <code>
        /// ortcClient.Send("channelName", "messageToSend");
        ///   </code>
        ///   </example>
        public override void Send(string channel, string message)
        {
            #region Sanity Checks

            if (!IsConnected)
            {
                DelegateExceptionCallback(new OrtcException("Not connected"));
            }
            else if (String.IsNullOrEmpty(channel))
            {
                DelegateExceptionCallback(new OrtcException("Channel is null or empty"));
            }
            else if (!channel.OrtcIsValidInput())
            {
                DelegateExceptionCallback(new OrtcException("Channel has invalid characters"));
            }
            else if (String.IsNullOrEmpty(message))
            {
                DelegateExceptionCallback(new OrtcException("Message is null or empty"));
            }
            else

            #endregion
            {
                byte[] channelBytes = Encoding.UTF8.GetBytes(channel);

                if (channelBytes.Length > MAX_CHANNEL_SIZE)
                {
                    DelegateExceptionCallback(new OrtcException(String.Format("Channel size exceeds the limit of {0} characters", MAX_CHANNEL_SIZE)));
                }
                else
                {
                    var domainChannelCharacterIndex = channel.IndexOf(':');
                    var channelToValidate = channel;

                    if (domainChannelCharacterIndex > 0)
                    {
                        channelToValidate = channel.Substring(0, domainChannelCharacterIndex + 1) + "*";
                    }



                    string hash = GetChannelHash(channel, channelToValidate);

                    if (_permissions != null && _permissions.Count > 0 && String.IsNullOrEmpty(hash))
                    {
                        DelegateExceptionCallback(new OrtcException(String.Format("No permission found to send to the channel '{0}'", channel)));
                    }
                    else
                    {
                        message = message.Replace(Environment.NewLine, "\n");

                        if (channel != String.Empty && message != String.Empty)
                        {
                            try
                            {
                                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                                List<string> messageParts = new List<string>();
                                int pos = 0;
                                int remaining;
                                string messageId = Strings.GenerateId(8);

                                // Multi part
                                while ((remaining = messageBytes.Length - pos) > 0)
                                {
                                    byte[] messagePart;

                                    if (remaining >= MAX_MESSAGE_SIZE - channelBytes.Length)
                                    {
                                        messagePart = new byte[MAX_MESSAGE_SIZE - channelBytes.Length];
                                    }
                                    else
                                    {
                                        messagePart = new byte[remaining];
                                    }

                                    Array.Copy(messageBytes, pos, messagePart, 0, messagePart.Length);

#if UNITY_WSA
                                    var b = (byte[])messagePart;
                                    messageParts.Add(Encoding.UTF8.GetString(b, 0, b.Length));
#else
                                    messageParts.Add(Encoding.UTF8.GetString((byte[])messagePart));
#endif

                                    pos += messagePart.Length;
                                }

                                for (int i = 0;i < messageParts.Count;i++)
                                {
                                    string s = String.Format("send;{0};{1};{2};{3};{4}", _applicationKey, _authenticationToken, channel, hash, String.Format("{0}_{1}-{2}_{3}", messageId, i + 1, messageParts.Count, messageParts[i]));

                                    DoSend(s);
                                }
                            }
                            catch (Exception ex)
                            {
                                string exName = null;

                                if (ex.InnerException != null)
                                {
                                    exName = ex.InnerException.GetType().Name;
                                }

                                switch (exName)
                                {
                                    case "OrtcNotConnectedException":
                                        // Server went down
                                        if (IsConnected)
                                        {
                                            DoDisconnect();
                                        }
                                        break;
                                    default:
                                        DelegateExceptionCallback(new OrtcException(String.Format("Unable to send: {0}", ex)));
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void SendProxy(string applicationKey, string privateKey, string channel, string message)
        {
            #region Sanity Checks

            if (!IsConnected)
            {
                DelegateExceptionCallback(new OrtcException("Not connected"));
            }
            else if (String.IsNullOrEmpty(applicationKey))
            {
                DelegateExceptionCallback(new OrtcException("Application Key is null or empty"));
            }
            else if (String.IsNullOrEmpty(privateKey))
            {
                DelegateExceptionCallback(new OrtcException("Private Key is null or empty"));
            }
            else if (String.IsNullOrEmpty(channel))
            {
                DelegateExceptionCallback(new OrtcException("Channel is null or empty"));
            }
            else if (!channel.OrtcIsValidInput())
            {
                DelegateExceptionCallback(new OrtcException("Channel has invalid characters"));
            }
            else if (String.IsNullOrEmpty(message))
            {
                DelegateExceptionCallback(new OrtcException("Message is null or empty"));
            }
            else

            #endregion
            {
                byte[] channelBytes = Encoding.UTF8.GetBytes(channel);

                if (channelBytes.Length > MAX_CHANNEL_SIZE)
                {
                    DelegateExceptionCallback(new OrtcException(String.Format("Channel size exceeds the limit of {0} characters", MAX_CHANNEL_SIZE)));
                }
                else
                {

                    message = message.Replace(Environment.NewLine, "\n");

                    if (channel != String.Empty && message != String.Empty)
                    {
                        try
                        {
                            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                            var messageParts = new List<string>();
                            int pos = 0;
                            int remaining;
                            string messageId = Strings.GenerateId(8);

                            // Multi part
                            while ((remaining = messageBytes.Length - pos) > 0)
                            {
                                byte[] messagePart;

                                if (remaining >= MAX_MESSAGE_SIZE - channelBytes.Length)
                                {
                                    messagePart = new byte[MAX_MESSAGE_SIZE - channelBytes.Length];
                                }
                                else
                                {
                                    messagePart = new byte[remaining];
                                }

                                Array.Copy(messageBytes, pos, messagePart, 0, messagePart.Length);

#if UNITY_WSA
                                var b = (byte[])messagePart;
                                messageParts.Add(Encoding.UTF8.GetString(b, 0, b.Length));
#else
                                messageParts.Add(Encoding.UTF8.GetString((byte[])messagePart));
#endif

                                pos += messagePart.Length;
                            }

                            for (int i = 0;i < messageParts.Count;i++)
                            {
                                string s = String.Format("sendproxy;{0};{1};{2};{3}", applicationKey, privateKey, channel, String.Format("{0}_{1}-{2}_{3}", messageId, i + 1, messageParts.Count, messageParts[i]));

                                DoSend(s);
                            }
                        }
                        catch (Exception ex)
                        {
                            string exName = null;

                            if (ex.InnerException != null)
                            {
                                exName = ex.InnerException.GetType().Name;
                            }

                            switch (exName)
                            {
                                case "OrtcNotConnectedException":
                                    // Server went down
                                    if (IsConnected)
                                    {
                                        DoDisconnect();
                                    }
                                    break;
                                default:
                                    DelegateExceptionCallback(new OrtcException(String.Format("Unable to send: {0}", ex)));
                                    break;
                            }
                        }
                    }
                }

            }
        }

        /// <summary>
        /// Subscribes to a channel.
        /// </summary>
        /// <param name="channel">Channel name.</param>
        /// <param name="onMessage"><see cref="OnMessageDelegate"/> callback.</param>
        /// <example>
        ///   <code>
        /// ortcClient.Subscribe("channelName", true, OnMessageCallback);
        /// private void OnMessageCallback(object sender, string channel, string message)
        /// {
        /// // Do something
        /// }
        ///   </code>
        ///   </example>
        public override void Subscribe(string channel, OnMessageDelegate onMessage)
        {
            #region Sanity Checks

            bool sanityChecked = true;

            if (!IsConnected)
            {
                DelegateExceptionCallback(new OrtcException("Not connected"));
                sanityChecked = false;
            }
            else if (String.IsNullOrEmpty(channel))
            {
                DelegateExceptionCallback(new OrtcException("Channel is null or empty"));
                sanityChecked = false;
            }
            else if (!channel.OrtcIsValidInput())
            {
                DelegateExceptionCallback(new OrtcException("Channel has invalid characters"));
                sanityChecked = false;
            }
            else if (_subscribedChannels.ContainsKey(channel))
            {
                ChannelSubscription channelSubscription = null;
                _subscribedChannels.TryGetValue(channel, out channelSubscription);

                if (channelSubscription != null)
                {
                    if (channelSubscription.IsSubscribing)
                    {
                        DelegateExceptionCallback(new OrtcException(String.Format("Already subscribing to the channel {0}", channel)));
                        sanityChecked = false;
                    }
                    else if (channelSubscription.IsSubscribed)
                    {
                        DelegateExceptionCallback(new OrtcException(String.Format("Already subscribed to the channel {0}", channel)));
                        sanityChecked = false;
                    }
                }
            }
            else
            {
                byte[] channelBytes = Encoding.UTF8.GetBytes(channel);

                if (channelBytes.Length > MAX_CHANNEL_SIZE)
                {
                    if (_subscribedChannels.ContainsKey(channel))
                    {
                        ChannelSubscription channelSubscription = null;
                        _subscribedChannels.TryGetValue(channel, out channelSubscription);

                        if (channelSubscription != null)
                        {
                            channelSubscription.IsSubscribing = false;
                        }
                    }

                    DelegateExceptionCallback(new OrtcException(String.Format("Channel size exceeds the limit of {0} characters", MAX_CHANNEL_SIZE)));
                    sanityChecked = false;
                }
            }

            #endregion

            if (sanityChecked)
            {
                var domainChannelCharacterIndex = channel.IndexOf(':');
                var channelToValidate = channel;

                if (domainChannelCharacterIndex > 0)
                {
                    channelToValidate = channel.Substring(0, domainChannelCharacterIndex + 1) + "*";
                }

                string hash = GetChannelHash(channel, channelToValidate);

                if (_permissions != null && _permissions.Count > 0 && String.IsNullOrEmpty(hash))
                {
                    DelegateExceptionCallback(new OrtcException(String.Format("No permission found to subscribe to the channel '{0}'", channel)));
                }
                else
                {
                    if (!_subscribedChannels.ContainsKey(channel))
                    {
                        _subscribedChannels.TryAdd(channel,
                            new ChannelSubscription
                            {
                                IsSubscribing = true,
                                IsSubscribed = false,
                                SubscribeOnReconnected = true,
                                OnMessage = onMessage
                            });
                    }

                    try
                    {
                        if (_subscribedChannels.ContainsKey(channel))
                        {
                            ChannelSubscription channelSubscription = null;
                            _subscribedChannels.TryGetValue(channel, out channelSubscription);

                            channelSubscription.IsSubscribing = true;
                            channelSubscription.IsSubscribed = false;
                            channelSubscription.SubscribeOnReconnected = true;
                            channelSubscription.OnMessage = onMessage;
                        }

                        string s = String.Format("subscribe;{0};{1};{2};{3}", _applicationKey, _authenticationToken, channel, hash);
                        DoSend(s);
                    }
                    catch (Exception ex)
                    {
                        string exName = null;

                        if (ex.InnerException != null)
                        {
                            exName = ex.InnerException.GetType().Name;
                        }

                        switch (exName)
                        {
                            case "OrtcNotConnectedException":
                                // Server went down
                                if (IsConnected)
                                {
                                    DoDisconnect();
                                }
                                break;
                            default:
                                DelegateExceptionCallback(new OrtcException(String.Format("Unable to subscribe: {0}", ex)));
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Unsubscribes from a channel.
        /// </summary>
        /// <param name="channel">Channel name.</param>
        /// <example>
        ///   <code>
        /// ortcClient.Unsubscribe("channelName");
        ///   </code>
        ///   </example>
        public override void Unsubscribe(string channel)
        {
            #region Sanity Checks

            bool sanityChecked = true;

            if (!IsConnected)
            {
                DelegateExceptionCallback(new OrtcException("Not connected"));
                sanityChecked = false;
            }
            else if (String.IsNullOrEmpty(channel))
            {
                DelegateExceptionCallback(new OrtcException("Channel is null or empty"));
                sanityChecked = false;
            }
            else if (!channel.OrtcIsValidInput())
            {
                DelegateExceptionCallback(new OrtcException("Channel has invalid characters"));
                sanityChecked = false;
            }
            else if (!_subscribedChannels.ContainsKey(channel))
            {
                DelegateExceptionCallback(new OrtcException(String.Format("Not subscribed to the channel {0}", channel)));
                sanityChecked = false;
            }
            else if (_subscribedChannels.ContainsKey(channel))
            {
                ChannelSubscription channelSubscription = null;
                _subscribedChannels.TryGetValue(channel, out channelSubscription);

                if (channelSubscription != null && !channelSubscription.IsSubscribed)
                {
                    DelegateExceptionCallback(new OrtcException(String.Format("Not subscribed to the channel {0}", channel)));
                    sanityChecked = false;
                }
            }
            else
            {
                byte[] channelBytes = Encoding.UTF8.GetBytes(channel);

                if (channelBytes.Length > MAX_CHANNEL_SIZE)
                {
                    DelegateExceptionCallback(new OrtcException(String.Format("Channel size exceeds the limit of {0} characters", MAX_CHANNEL_SIZE)));
                    sanityChecked = false;
                }
            }

            #endregion

            if (sanityChecked)
            {
                try
                {
                    string s = String.Format("unsubscribe;{0};{1}", _applicationKey, channel);
                    DoSend(s);
                }
                catch (Exception ex)
                {
                    string exName = null;

                    if (ex.InnerException != null)
                    {
                        exName = ex.InnerException.GetType().Name;
                    }

                    switch (exName)
                    {
                        case "OrtcNotConnectedException":
                            // Server went down
                            if (IsConnected)
                            {
                                DoDisconnect();
                            }
                            break;
                        default:
                            DelegateExceptionCallback(new OrtcException(String.Format("Unable to unsubscribe: {0}", ex)));
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Disconnects from the gateway.
        /// </summary>
        /// <example>
        ///   <code>
        /// ortcClient.Disconnect();
        ///   </code>
        ///   </example>
        public override void Disconnect()
        {
            // Clear subscribed channels
            _subscribedChannels.Clear();


            #region Sanity Checks
            if (!IsConnecting && !IsConnected)
            {
                DelegateExceptionCallback(new OrtcException("Not connected"));
            }
            else

            #endregion
            {
                DoDisconnect();
            }
        }

        /// <summary>
        /// Indicates whether is subscribed to a channel.
        /// </summary>
        /// <param name="channel">The channel name.</param>
        /// <returns>
        ///   <c>true</c> if subscribed to the channel; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsSubscribed(string channel)
        {
            bool result = false;

            #region Sanity Checks

            if (!IsConnected)
            {
                DelegateExceptionCallback(new OrtcException("Not connected"));
            }
            else if (String.IsNullOrEmpty(channel))
            {
                DelegateExceptionCallback(new OrtcException("Channel is null or empty"));
            }
            else if (!channel.OrtcIsValidInput())
            {
                DelegateExceptionCallback(new OrtcException("Channel has invalid characters"));
            }
            else

            #endregion
            {
                result = false;

                if (_subscribedChannels.ContainsKey(channel))
                {
                    ChannelSubscription channelSubscription = null;
                    _subscribedChannels.TryGetValue(channel, out channelSubscription);

                    if (channelSubscription != null && channelSubscription.IsSubscribed)
                    {
                        result = true;
                    }
                }
            }

            return result;
        }

        #endregion

        #region Private Methods (13)

        string GetChannelHash(string channel, string channelToValidate)
        {
            foreach (var keyValuePair in _permissions)
            {
                if (keyValuePair.Key == channel)
                    return keyValuePair.Value;

                if (keyValuePair.Key == channelToValidate)
                    return keyValuePair.Value;
            }

            return null;
        }

        /// <summary>
        /// Processes the operation validated.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        private void ProcessOperationValidated(string arguments)
        {
            if (!String.IsNullOrEmpty(arguments))
            {
                bool isValid = false;

                // Try to match with authentication
                Match validatedAuthMatch = Regex.Match(arguments, VALIDATED_PATTERN);

                if (validatedAuthMatch.Success)
                {
                    isValid = true;

                    string userPermissions = String.Empty;

                    if (validatedAuthMatch.Groups["up"].Length > 0)
                    {
                        userPermissions = validatedAuthMatch.Groups["up"].Value;
                    }

                    if (validatedAuthMatch.Groups["set"].Length > 0)
                    {
                        _sessionExpirationTime = int.Parse(validatedAuthMatch.Groups["set"].Value);
                    }
                    if (String.IsNullOrEmpty(ReadLocalStorage(_applicationKey, _sessionExpirationTime)))
                    {
                        CreateLocalStorage(_applicationKey);
                    }

                    if (!String.IsNullOrEmpty(userPermissions) && userPermissions != "null")
                    {
                        MatchCollection matchCollection = Regex.Matches(userPermissions, PERMISSIONS_PATTERN);

                        var permissions = new List<KeyValuePair<string, string>>();

                        foreach (Match match in matchCollection)
                        {
                            string channel = match.Groups["key"].Value;
                            string hash = match.Groups["value"].Value;

                            permissions.Add(new KeyValuePair<string, string>(channel, hash));
                        }

                        _permissions = new List<KeyValuePair<string, string>>(permissions);
                    }
                }

                if (isValid)
                {
                    IsConnecting = false;

                    if (_alreadyConnectedFirstTime)
                    {
                        var channelsToRemove = new List<string>();

                        // Subscribe to the previously subscribed channels
                        foreach (KeyValuePair<string, ChannelSubscription> item in _subscribedChannels)
                        {
                            string channel = item.Key;
                            ChannelSubscription channelSubscription = item.Value;

                            // Subscribe again
                            if (channelSubscription.SubscribeOnReconnected && (channelSubscription.IsSubscribing || channelSubscription.IsSubscribed))
                            {
                                channelSubscription.IsSubscribing = true;
                                channelSubscription.IsSubscribed = false;

                                var domainChannelCharacterIndex = channel.IndexOf(':');
                                var channelToValidate = channel;

                                if (domainChannelCharacterIndex > 0)
                                {
                                    channelToValidate = channel.Substring(0, domainChannelCharacterIndex + 1) + "*";
                                }

                                string hash = GetChannelHash(channel, channelToValidate);

                                string s = String.Format("subscribe;{0};{1};{2};{3}", _applicationKey, _authenticationToken, channel, hash);

                                DoSend(s);
                            }
                            else
                            {
                                channelsToRemove.Add(channel);
                            }
                        }

                        for (int i = 0;i < channelsToRemove.Count;i++)
                        {
                            ChannelSubscription removeResult = null;
                            _subscribedChannels.TryRemove(channelsToRemove[i].ToString(), out removeResult);
                        }

                        // Clean messages buffer (can have lost message parts in memory)
                        _multiPartMessagesBuffer.Clear();

                        DelegateReconnectedCallback();
                    }
                    else
                    {
                        _alreadyConnectedFirstTime = true;

                        // Clear subscribed channels
                        _subscribedChannels.Clear();

                        DelegateConnectedCallback();
                    }

                    if (arguments.IndexOf("busy") < 0)
                    {
                        StopReconnect();
                    }
                }
            }
        }

        /// <summary>
        /// Processes the operation subscribed.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        private void ProcessOperationSubscribed(string arguments)
        {
            if (!String.IsNullOrEmpty(arguments))
            {
                Match subscribedMatch = Regex.Match(arguments, CHANNEL_PATTERN);

                if (subscribedMatch.Success)
                {
                    string channelSubscribed = String.Empty;

                    if (subscribedMatch.Groups["channel"].Length > 0)
                    {
                        channelSubscribed = subscribedMatch.Groups["channel"].Value;
                    }

                    if (!String.IsNullOrEmpty(channelSubscribed))
                    {
                        ChannelSubscription channelSubscription = null;
                        _subscribedChannels.TryGetValue(channelSubscribed, out channelSubscription);

                        if (channelSubscription != null)
                        {
                            channelSubscription.IsSubscribing = false;
                            channelSubscription.IsSubscribed = true;
                        }

                        DelegateSubscribedCallback(channelSubscribed);
                    }
                }
            }
        }

        /// <summary>
        /// Processes the operation unsubscribed.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        private void ProcessOperationUnsubscribed(string arguments)
        {
            // UnityEngine.Debug.Log("ProcessOperationUnsubscribed");
            if (!String.IsNullOrEmpty(arguments))
            {
                Match unsubscribedMatch = Regex.Match(arguments, CHANNEL_PATTERN);

                if (unsubscribedMatch.Success)
                {
                    string channelUnsubscribed = String.Empty;

                    if (unsubscribedMatch.Groups["channel"].Length > 0)
                    {
                        channelUnsubscribed = unsubscribedMatch.Groups["channel"].Value;
                    }

                    if (!String.IsNullOrEmpty(channelUnsubscribed))
                    {
                        ChannelSubscription channelSubscription = null;
                        _subscribedChannels.TryGetValue(channelUnsubscribed, out channelSubscription);

                        if (channelSubscription != null)
                        {
                            channelSubscription.IsSubscribed = false;
                        }

                        DelegateUnsubscribedCallback(channelUnsubscribed);
                    }
                }
            }
        }

        /// <summary>
        /// Processes the operation error.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        private void ProcessOperationError(string arguments)
        {
            if (!String.IsNullOrEmpty(arguments))
            {
                Match errorMatch = Regex.Match(arguments, EXCEPTION_PATTERN);

                if (errorMatch.Success)
                {
                    string op = String.Empty;
                    string error = String.Empty;
                    string channel = String.Empty;

                    if (errorMatch.Groups["op"].Length > 0)
                    {
                        op = errorMatch.Groups["op"].Value;
                    }

                    if (errorMatch.Groups["error"].Length > 0)
                    {
                        error = errorMatch.Groups["error"].Value;
                    }

                    if (errorMatch.Groups["channel"].Length > 0)
                    {
                        channel = errorMatch.Groups["channel"].Value;
                    }

                    if (!String.IsNullOrEmpty(error))
                    {
                        DelegateExceptionCallback(new OrtcException(error));
                    }

                    if (!String.IsNullOrEmpty(op))
                    {
                        switch (op)
                        {
                            case "validate":
                                if (!String.IsNullOrEmpty(error) && (error.Contains("Unable to connect") || error.Contains("Server is too busy")))
                                {
                                    DelegateExceptionCallback(new Exception(error));
                                    DoReconnectOrDisconnect();
                                }
                                else
                                {
                                    DoDisconnect();
                                }
                                break;
                            case "subscribe":
                                if (!String.IsNullOrEmpty(channel))
                                {
                                    ChannelSubscription channelSubscription = null;
                                    _subscribedChannels.TryGetValue(channel, out channelSubscription);

                                    if (channelSubscription != null)
                                    {
                                        channelSubscription.IsSubscribing = false;
                                    }
                                }
                                break;
                            case "subscribe_maxsize":
                            case "unsubscribe_maxsize":
                            case "send_maxsize":
                                if (!String.IsNullOrEmpty(channel))
                                {
                                    ChannelSubscription channelSubscription = null;
                                    _subscribedChannels.TryGetValue(channel, out channelSubscription);

                                    if (channelSubscription != null)
                                    {
                                        channelSubscription.IsSubscribing = false;
                                    }
                                }

                                DoDisconnect();
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

    
        private void ProcessOperationReceived(string message)
        {
            Match receivedMatch = Regex.Match(message, RECEIVED_PATTERN);

            // Received
            if (receivedMatch.Success)
            {
                string channelReceived = String.Empty;
                string messageReceived = String.Empty;

                if (receivedMatch.Groups["channel"].Length > 0)
                {
                    channelReceived = receivedMatch.Groups["channel"].Value;
                }

                if (receivedMatch.Groups["message"].Length > 0)
                {
                    messageReceived = receivedMatch.Groups["message"].Value;
                }

                if (!String.IsNullOrEmpty(channelReceived) && !String.IsNullOrEmpty(messageReceived) && _subscribedChannels.ContainsKey(channelReceived))
                {
                    messageReceived = messageReceived.Replace(@"\\n", Environment.NewLine).Replace("\\\\\"", @"""").Replace("\\\\\\\\", @"\");

                    // Multi part
                    Match multiPartMatch = Regex.Match(messageReceived, MULTI_PART_MESSAGE_PATTERN);

                    string messageId = String.Empty;
                    int messageCurrentPart = 1;
                    int messageTotalPart = 1;
                    bool lastPart = false;
                    ConcurrentDictionary<int, BufferedMessage> messageParts = null;

                    if (multiPartMatch.Success)
                    {
                        if (multiPartMatch.Groups["messageId"].Length > 0)
                        {
                            messageId = multiPartMatch.Groups["messageId"].Value;
                        }

                        if (multiPartMatch.Groups["messageCurrentPart"].Length > 0)
                        {
                            messageCurrentPart = Int32.Parse(multiPartMatch.Groups["messageCurrentPart"].Value);
                        }

                        if (multiPartMatch.Groups["messageTotalPart"].Length > 0)
                        {
                            messageTotalPart = Int32.Parse(multiPartMatch.Groups["messageTotalPart"].Value);
                        }

                        if (multiPartMatch.Groups["message"].Length > 0)
                        {
                            messageReceived = multiPartMatch.Groups["message"].Value;
                        }
                    }

                    lock (_multiPartMessagesBuffer)
                    {
                        // Is a message part
                        if (!String.IsNullOrEmpty(messageId))
                        {
                            if (!_multiPartMessagesBuffer.ContainsKey(messageId))
                            {
                                _multiPartMessagesBuffer.TryAdd(messageId, new ConcurrentDictionary<int, BufferedMessage>());
                            }


                            _multiPartMessagesBuffer.TryGetValue(messageId, out messageParts);

                            if (messageParts != null)
                            {
                                lock (messageParts)
                                {

                                    messageParts.TryAdd(messageCurrentPart, new BufferedMessage(messageCurrentPart, messageReceived));

                                    // Last message part
                                    if (messageParts.Count == messageTotalPart)
                                    {
                                        //messageParts.Sort();

                                        lastPart = true;
                                    }
                                }
                            }
                        }
                        // Message does not have multipart, like the messages received at announcement channels
                        else
                        {
                            lastPart = true;
                        }

                        if (lastPart)
                        {
                            if (_subscribedChannels.ContainsKey(channelReceived))
                            {
                                ChannelSubscription channelSubscription = null;
                                _subscribedChannels.TryGetValue(channelReceived, out channelSubscription);

                                if (channelSubscription != null)
                                {
                                    var ev = channelSubscription.OnMessage;

                                    if (ev != null)
                                    {
                                        if (!String.IsNullOrEmpty(messageId) && _multiPartMessagesBuffer.ContainsKey(messageId))
                                        {
                                            messageReceived = String.Empty;
                                            //lock (messageParts)
                                            //{
                                            var bufferedMultiPartMessages = new List<BufferedMessage>();

                                            foreach (var part in messageParts.Keys)
                                            {
                                                bufferedMultiPartMessages.Add(messageParts[part]);
                                            }

                                            bufferedMultiPartMessages.Sort();

                                            foreach (var part in bufferedMultiPartMessages)
                                            {
                                                if (part != null)
                                                {
                                                    messageReceived = String.Format("{0}{1}", messageReceived, part.Message);
                                                }
                                            }
                                            //}

                                            // Remove from messages buffer
                                            ConcurrentDictionary<int, BufferedMessage> removeResult = null;
                                            _multiPartMessagesBuffer.TryRemove(messageId, out removeResult);
                                        }

                                        if (!String.IsNullOrEmpty(messageReceived))
                                        {
                                            TaskManager.RunOnMainThread(() =>
                                            {
                                                ev(channelReceived, messageReceived);
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Unknown
                DelegateExceptionCallback(new OrtcException(String.Format("Unknown message received: {0}", message)));
            }
        }


        /// <summary>
        /// Do the Connect Task
        /// </summary>
        IEnumerator DoConnect()
        {
            IsConnecting = true;

            if (IsCluster)
            {
                var cTask = ClusterClient.GetClusterServer(ClusterUrl, _applicationKey);
                yield return TaskManager.StartRoutine(cTask.WaitRoutine());

                if (cTask.IsFaulted)
                {
                    DoReconnectOrDisconnect();
                    DelegateExceptionCallback(new OrtcException("Connection Failed. Unable to get URL from cluster"));
                    yield break;
                }

                Url = cTask.Result;
                IsCluster = true;

                if (String.IsNullOrEmpty(Url))
                {
                    DoReconnectOrDisconnect();
                    DelegateExceptionCallback(new OrtcException("Connection Failed. Unable to get URL from cluster"));
                    yield break;
                }
            }

            if (!String.IsNullOrEmpty(Url))
            {
                try
                {
                    // make a new socket
                    MakeSocket();

                    // use socket
                    _webSocketConnection.Connect(Url);

                    // Just in case the server does not respond
                    _waitingServerResponse = true;
                }
                catch (Exception ex)
                {
                    DoReconnectOrDisconnect();
                    DelegateExceptionCallback(new OrtcException("Connection Failed. " + ex.Message));
                }
            }
        }

        /// <summary>
        /// Disconnect the TCP client.
        /// </summary>
        private void DoDisconnect()
        {
            _forcedClosed = true;
            StopReconnect();

            if (IsConnected)
            {
                try
                {
                    _webSocketConnection.Close();
                }
                catch (Exception ex)
                {
                    DelegateExceptionCallback(new OrtcException(String.Format("Error disconnecting: {0}", ex)));
                    DelegateDisconnectedCallback();
                }
            }
            else
            {
                DelegateDisconnectedCallback();
            }
        }

        /// <summary>
        /// Sends a message through the TCP client.
        /// </summary>
        /// <param name="message"></param>
        private void DoSend(string message)
        {
            try
            {
                _webSocketConnection.Send(message);
            }
            catch (Exception ex)
            {
                DelegateExceptionCallback(new OrtcException(String.Format("Unable to send: {0}", ex)));
            }
        }

        public override string ReadLocalStorage(string applicationKey, int sessionExpirationTime)
        {
            //server sends multiple messages if reconnecting. new session to be safe.
            return string.Empty;
            var created = PlayerPrefs.GetString(string.Format("{0}-{1}-{2}", SESSION_STORAGE_NAME, "created", applicationKey));
            var session = PlayerPrefs.GetString(string.Format("{0}-{1}-{2}", SESSION_STORAGE_NAME, "session", applicationKey));

            if (string.IsNullOrEmpty(created))
                return string.Empty;

            if (string.IsNullOrEmpty(session))
                return string.Empty;

            var createdDate = DateTime.Parse(created);

            var currentDateTime = DateTime.Now;
            var interval = currentDateTime.Subtract(createdDate);

            if (createdDate != DateTime.MinValue && interval.TotalMinutes >= sessionExpirationTime)
            {
                return string.Empty;
            }
            SessionId = session;

            return session;
        }

        public override void CreateLocalStorage(string applicationKey)
        {
            //server sends multiple messages if reconnecting. new session to be safe.
            return;
            PlayerPrefs.SetString(string.Format("{0}-{1}-{2}", SESSION_STORAGE_NAME, "created", applicationKey), DateTime.UtcNow.ToString());
            PlayerPrefs.SetString(string.Format("{0}-{1}-{2}", SESSION_STORAGE_NAME, "session", applicationKey), SessionId);
        }

        private void DoReconnectOrDisconnect()
        {
            if (EnableReconnect)
                DoReconnect();
            else
                DoDisconnect();

        }


        private void DoReconnect()
        {
            IsConnecting = true;
            DelegateReconnectingCallback();

            _reconnectTimer.Start();
        }

        private void StopReconnect()
        {
            IsConnecting = false;
            _reconnectTimer.Stop();
        }

        #endregion

        #region Events handlers (6)

        void _reconnectTimer_Elapsed()
        {
            if (!IsConnected)
            {
                if (_waitingServerResponse)
                {
                    _waitingServerResponse = false;
                    DelegateExceptionCallback(new OrtcException("Unable to connect"));
                }

                DelegateReconnectingCallback();
                TaskManager.StartRoutine(DoConnect());
            }
        }

        void _heartbeatTimer_Elapsed()
        {
            if (IsConnected)
            {
                DoSend("b");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void _webSocketConnection_OnOpened()
        {
            // Do nothing
        }

        /// <summary>
        /// 
        /// </summary>
        private void _webSocketConnection_OnClosed()
        {
            IsConnected = false;

            if (!_forcedClosed && EnableReconnect)
                DoReconnect();
            else
                DoDisconnect();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="error"></param>
        private void _webSocketConnection_OnError(string error)
        {
            DelegateExceptionCallback(new OrtcException(error));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private void _webSocketConnection_OnMessageReceived(string message)
        {
            if (!String.IsNullOrEmpty(message))
            {
                // Open
                if (message == "o")
                {
                    try
                    {
                        SessionId = Strings.GenerateId(16);

                        string s;
                        if (HeartbeatActive)
                        {
                            s = String.Format("validate;{0};{1};{2};{3};{4};{5};{6}", _applicationKey, _authenticationToken, AnnouncementSubChannel, SessionId,
                                ConnectionMetadata, HeartbeatTime, HeartbeatFails);
                        }
                        else
                        {
                            s = String.Format("validate;{0};{1};{2};{3};{4}", _applicationKey, _authenticationToken, AnnouncementSubChannel, SessionId, ConnectionMetadata);
                        }
                        DoSend(s);
                    }
                    catch (Exception ex)
                    {
                        DelegateExceptionCallback(new OrtcException(String.Format("Exception sending validate: {0}", ex)));
                    }
                }
                // Heartbeat
                else if (message == "h")
                {
                    // Do nothing
                }
                else
                {
                    message = message.Replace("\\\"", @"""");

                    // UnityEngine.Debug.Log(message);

                    // Operation
                    Match operationMatch = Regex.Match(message, OPERATION_PATTERN);

                    //   Debug.Log(operationMatch.Success);

                    if (operationMatch.Success)
                    {
                        string operation = operationMatch.Groups["op"].Value;
                        string arguments = operationMatch.Groups["args"].Value;

                        // Debug.Log(operation);
                        // Debug.Log(arguments);

                        switch (operation)
                        {
                            case "ortc-validated":
                                ProcessOperationValidated(arguments);
                                break;
                            case "ortc-subscribed":
                                ProcessOperationSubscribed(arguments);
                                break;
                            case "ortc-unsubscribed":
                                ProcessOperationUnsubscribed(arguments);
                                break;
                            case "ortc-error":
                                ProcessOperationError(arguments);
                                break;
                            default:
                                // Unknown operation
                                DelegateExceptionCallback(new OrtcException(String.Format("Unknown operation \"{0}\" for the message \"{1}\"", operation, message)));
                                DoDisconnect();
                                break;
                        }
                    }
                    else
                    {
                        // Close
                        Match closeOperationMatch = Regex.Match(message, CLOSE_PATTERN);

                        if (!closeOperationMatch.Success)
                        {
                            ProcessOperationReceived(message);
                        }
                    }
                }
            }
        }

        #endregion

        #region Events calls (7)

        private void DelegateConnectedCallback()
        {
            IsConnected = true;
            IsConnecting = false;
            _reconnectTimer.Stop();
            //_heartbeatTimer.Start();

            TaskManager.RunOnMainThread(() =>
            {
                //Debug.Log("Ortc.Connected");
                var ev = _onConnected;

                if (ev != null)
                {
                    ev();
                }

            });
        }

        private void DelegateDisconnectedCallback()
        {
            IsConnected = false;
            IsConnecting = false;
            _alreadyConnectedFirstTime = false;
            _reconnectTimer.Stop();
            //_heartbeatTimer.Stop();

            // Clear user permissions
            _permissions.Clear();

            UnityTask.RunOnMain(() =>
            {
                //Debug.Log("Ortc.Disconnected");
                var ev = _onDisconnected;
                if (ev != null)
                {
                    ev();
                }
            });

        }

        private void DelegateSubscribedCallback(string channel)
        {
            TaskManager.RunOnMainThread(() =>
            {
                var ev = _onSubscribed;

                if (ev != null)
                {
                    ev(channel);
                }

            });
        }

        private void DelegateUnsubscribedCallback(string channel)
        {
            //   Debug.Log("DelegateUnsubscribedCallback");
            TaskManager.RunOnMainThread(() =>
            {
                var ev = _onUnsubscribed;

                if (ev != null)
                {
                    ev(channel);
                }

            });
        }

        private void DelegateExceptionCallback(Exception ex)
        {
            TaskManager.RunOnMainThread(() =>
            {
                var ev = _onException;
                if (ev != null)
                {
                    ev(ex);
                }
            });
        }

        private void DelegateReconnectingCallback()
        {
            TaskManager.RunOnMainThread(() =>
            {
                // Debug.Log("Ortc.Reconnecting");
                var ev = _onReconnecting;

                if (ev != null)
                {
                    ev();
                }

            });
        }

        private void DelegateReconnectedCallback()
        {
            IsConnected = true;
            IsConnecting = false;
            _reconnectTimer.Stop();

            TaskManager.RunOnMainThread(() =>
            {
                //Debug.Log("Ortc.Reconnected");
                var ev = _onReconnected;
                if (ev != null)
                {
                    ev();
                }
            });
        }


        #endregion
    }
}