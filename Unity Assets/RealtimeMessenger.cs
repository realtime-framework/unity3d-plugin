// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Foundation.Tasks;
using UnityEngine;

namespace Realtime.Messaging
{
    #region subclass

    /// <summary>
    /// Channel Permission Instructions
    /// </summary>
    [Serializable]
    public class RealtimePermission
    {
        /// <summary>
        /// Channel Name
        /// </summary>
        [SerializeField]
        public string Channel;
        /// <summary>
        /// Permission
        /// </summary>
        [SerializeField]
        public ChannelPermissions Permission;

        public RealtimePermission()
        {

        }

        public RealtimePermission(string c, ChannelPermissions p)
        {
            Channel = c;
            Permission = p;
        }
    }

    /// <summary>
    /// Occurs when the client connects to the gateway.
    /// </summary>
    public delegate void OnConnectionChangedDelegate(ConnectionState state);

    /// <summary>
    /// Occurs when there is an exception.
    /// </summary>
    /// <param name="message"></param>
    public delegate void OnChannelMessageDelegate(string message);

    /// <summary>
    /// Describes the status of a connection
    /// </summary>
    public enum ConnectionState
    {
        /// <summary>
        /// Not connected
        /// </summary>
        Disconnected,
        /// <summary>
        /// Is Connecting
        /// </summary>
        Connecting,
        /// <summary>
        /// Lost connection
        /// </summary>
        Reconnecting,
        /// <summary>
        /// connected
        /// </summary>
        Connected,
        /// <summary>
        /// Disconnected with saved subscriptions
        /// </summary>
        Paused,
        /// <summary>
        /// Is disconnecting with saved subscriptions
        /// </summary>
        Pausing,
        /// <summary>
        /// Is reconnecting with saved subscriptions
        /// </summary>
        Resuming,
    }

    /// <summary>
    /// describes the current status of a channel subscription
    /// </summary>
    public enum SubscriptionState
    {
        /// <summary>
        /// Not receiving messages
        /// </summary>
        Unsubscribed,
        /// <summary>
        /// Waiting for subscription confirmation
        /// </summary>
        Subscribing,
        /// <summary>
        /// Connection was lost and is reconnecting
        /// </summary>
        Resubscribing,
        /// <summary>
        /// Receiving messages
        /// </summary>
        Subscribed,
        /// <summary>
        /// Subscription will occur on resume
        /// </summary>
        Paused,
    }

    /// <summary>
    /// Url for the realtime network
    /// </summary>
    public struct RealtimeUrl
    {
        /// <summary>
        /// The url to the server
        /// </summary>
        public string Path;

        /// <summary>
        /// Is this server a cluster
        /// </summary>
        public bool IsCluster;

        public RealtimeUrl(string url, bool isCluster)
        {
            Path = url;
            IsCluster = isCluster;
        }

    }

    #endregion

    /// <summary>
    /// A Unity-Friendly messenger api using the IBT.ORTC service
    /// </summary>
    public class RealtimeMessenger
    {
        #region fields
        private readonly OrtcClient _client;

        // cache tasks. Use "Custom" Execution. Pass Exception/Success result back in ORTC handlers
        private UnityTask _connectionTask;
        private UnityTask _unsubscribeTask;
        private UnityTask _subscribeTask;
        private string _lastSubscribeChannel;
        private string _lastUnsubscribeChannel;
        #endregion

        #region properties

        private ConnectionState _state;
        /// <summary>
        /// Current connection state
        /// </summary>
        public ConnectionState State
        {
            get { return _state; }
            set
            {
                if (_state == value)
                    return;
                _state = value;

                if (_onConnectionChanged != null)
                {
                    _onConnectionChanged(value);
                }
            }
        }

        /// <summary>
        ///  State == ConnectionState.Connected
        /// </summary>
        public bool IsConnected
        {
            get { return State == ConnectionState.Connected; }
        }

        /// <summary>
        /// Unique identifier set be the server for this connection
        /// </summary>
        public string SessionId
        {
            get { return _client.SessionId; }
        }

        /// <summary>
        /// Unique identifier for this client
        /// </summary>
        public string Id
        {
            get { return _client.Id; }
        }

        /// <summary> 
        /// UserName or UserId
        /// </summary>
        /// <remarks>
        /// Should be set prior to connection.
        /// </remarks>
        public string ConnectionMetadata
        {
            get { return _client.ConnectionMetadata; }
            set { _client.ConnectionMetadata = value; }
        }

        /// <summary> 
        /// Gets or sets the client announcement subchannel.
        /// </summary>
        public string AnnouncementSubChannel
        {
            get { return _client.AnnouncementSubChannel; }
            set { _client.AnnouncementSubChannel = value; }
        }

        /// <summary>
        /// Url for the Realtime Service
        /// </summary>
        public RealtimeUrl Url
        {
            get
            {
                if (_client.IsCluster)
                    return new RealtimeUrl
                    {
                        IsCluster = _client.IsCluster,
                        Path = _client.ClusterUrl,
                    };
                return new RealtimeUrl
                {
                    IsCluster = _client.IsCluster,
                    Path = _client.Url,
                };
            }
            set
            {
                if (value.IsCluster)
                    _client.ClusterUrl = value.Path;
                else
                    _client.Url = value.Path;
            }
        }

        /// <summary>
        /// Default Application Key.
        /// Acquired from RealtimeSettings 
        /// </summary>
        public string ApplicationKey { get; set; }

        /// <summary>
        /// Default Private Key. Used for authentication.
        /// Acquired from RealtimeSettings 
        /// </summary>
        public string PrivateKey { get; set; }

        /// <summary>
        /// Time for an authentication token to live in seconds.
        /// Expires after inactivity.
        /// </summary>
        public int AuthenticationTime { get; set; }

        /// <summary>
        /// Restricts authentication token's use to a single client
        /// </summary>
        public bool AuthenticationIsPrivate { get; set; }

        /// <summary>
        /// Current Authentication Token
        /// </summary>
        public string AuthenticationToken { get; set; }

        /// <summary>
        /// Enables reconnection on disconnection
        /// </summary>
        public bool EnableReconnect
        {
            get { return _client.EnableReconnect; }
            set { _client.EnableReconnect = value; }
        }

        /// <summary>
        /// Indicates that client is disconnected and has cached subscriptions.
        /// Call Resume to reconnect and reapply cached subscriptions.
        /// </summary>
        public bool IsPaused
        {
            get { return State == ConnectionState.Paused; }
        }

        /// <summary>
        /// The ortc client.
        /// </summary>
        public OrtcClient Client
        {
            get { return _client; }
        }

        /// <summary>
        /// Collection of all message handlers : Channel Name by a list of handlers
        /// </summary>
        protected Dictionary<string, List<OnChannelMessageDelegate>> Listeners = new Dictionary<string, List<OnChannelMessageDelegate>>();

        /// <summary>
        /// All subscriptions : Channel Name by The current state of the subscription
        /// </summary>
        protected Dictionary<string, SubscriptionState> SubscriptionStates = new Dictionary<string, SubscriptionState>();

        #endregion

        #region events
        OnConnectionChangedDelegate _onConnectionChanged;
        /// <summary>
        /// Raised when the connection status of the client has changed
        /// </summary>
        public event OnConnectionChangedDelegate OnConnectionChanged
        {
            add
            {
                _onConnectionChanged = (OnConnectionChangedDelegate)Delegate.Combine(_onConnectionChanged, value);
            }

            remove
            {
                _onConnectionChanged = (OnConnectionChangedDelegate)Delegate.Remove(_onConnectionChanged, value);
            }
        }


        OnExceptionDelegate _onException;
        /// <summary>
        /// Raised when a message is received
        /// </summary>
        public event OnExceptionDelegate OnException
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

        OnMessageDelegate _onMessage;
        /// <summary>
        /// Raised when a message is received
        /// </summary>
        public event OnMessageDelegate OnMessage
        {
            add
            {
                _onMessage = (OnMessageDelegate)Delegate.Combine(_onMessage, value);
            }

            remove
            {
                _onMessage = (OnMessageDelegate)Delegate.Remove(_onMessage, value);
            }
        }

        #endregion

        #region ctor
        /// <summary>
        /// Creates a new messenger with the default url
        /// </summary>
        public RealtimeMessenger(OrtcClient client, MessengerSettings settings)
        {
            TaskManager.ConfirmInit();

            _client = client;

            _client.OnConnected += _client_OnConnected;
            _client.OnDisconnected += _client_OnDisconnected;
            _client.OnReconnected += _client_OnReconnected;
            _client.OnReconnecting += _client_OnReconnecting;
            _client.OnException += _client_OnException;
            _client.OnSubscribed += _client_OnSubscribed;
            _client.OnUnsubscribed += _client_OnUnsubscribed;

            ApplicationKey = settings.ApplicationKey;
            PrivateKey = settings.PrivateKey;
            Url = new RealtimeUrl(settings.Url, settings.IsCluster);
            AuthenticationTime = settings.AuthenticationTime;
            AuthenticationIsPrivate = settings.AuthenticationIsPrivate;
        }
        
        /// <summary>
        /// Creates a new messenger with the default url
        /// </summary>
        public RealtimeMessenger()
            : this(new UnityOrtcClient(), MessengerSettings.Instance)
        {
        }


        #endregion

        #region Messenger Methods
        /// <summary>
        /// Begins a Connection Task.
        /// Be sure to set your AuthenticationToken and Metadata First !
        /// </summary>
        /// <returns>A task for the duration of the process</returns>
        public UnityTask Connect()
        {
            if (State == ConnectionState.Connecting)
            {
                return _connectionTask;
            }

            if (State != ConnectionState.Disconnected && State != ConnectionState.Paused)
            {
                return UnityTask.FailedTask(new Exception("Already Connected"));
            }

            if (State == ConnectionState.Paused)
            {
                State = ConnectionState.Disconnected;
                SubscriptionStates.Clear();
            }

            _connectionTask = new UnityTask(TaskStrategy.Custom);

            State = ConnectionState.Connecting;

            _client.Connect(ApplicationKey, AuthenticationToken);

            return _connectionTask;
        }

        /// <summary>
        /// Begins a Resume Task.
        /// Will reconnect and reapply paused subscriptions
        /// </summary>
        /// <returns>A task for the duration of the process</returns>
        public UnityTask Resume()
        {
            if (State != ConnectionState.Paused)
            {
                return UnityTask.FailedTask(new Exception("The client is not paused"));
            }

            State = ConnectionState.Resuming;

            // 
            var task = _connectionTask = new UnityTask(TaskStrategy.Custom);

            _client.Connect(ApplicationKey, AuthenticationToken);

            return task;
        }

        /// <summary>
        /// Begins Disconnection
        /// </summary>
        /// <returns>A task for the duration of the process</returns>
        public UnityTask Disconnect()
        {
            if (State == ConnectionState.Disconnected)
            {
                return UnityTask.FailedTask(new Exception("The client is already disconnected"));
            }

            var disconnect = new UnityTask(TaskStrategy.Custom);

            _client.Disconnect();

            SubscriptionStates.Clear();

            disconnect.Status = TaskStatus.Success;

            return disconnect;
        }

        /// <summary>
        /// Disconnects but caches subscriptions. Call Resume to resubscribe from cache.
        /// </summary>
        /// <returns>A task for the duration of the process</returns>
        public UnityTask Pause()
        {
            if (State == ConnectionState.Paused || State == ConnectionState.Pausing)
            {
                return UnityTask.FailedTask(new Exception("The client is already paused or pausing"));
            }

            if (State == ConnectionState.Disconnected || State == ConnectionState.Disconnected)
            {
                return UnityTask.FailedTask(new Exception("The client is already disconnected"));
            }

            State = ConnectionState.Pausing;

            var disconnect = new UnityTask(TaskStrategy.Custom);

            _client.Disconnect();

            foreach (var state in SubscriptionStates.Keys.ToArray())
            {
                SubscriptionStates[state] = SubscriptionState.Paused;
            }

            State = ConnectionState.Paused;

            disconnect.Status = TaskStatus.Success;

            return disconnect;
        }

        /// <summary>
        /// Begins subscription to the ORTC channel
        /// </summary>
        /// <param name="channel"></param>
        /// <returns>A task for the duration of the process</returns>
        public UnityTask Subscribe(string channel)
        {
            if (State != ConnectionState.Connected)
            {
                return UnityTask.FailedTask(new Exception("Not Connected"));
            }

            var s = GetSubscriptionState(channel);

            if (s != SubscriptionState.Unsubscribed)
            {
                return UnityTask.FailedTask(new Exception("Already subscribed or is Subscribing"));
            }

            _subscribeTask = new UnityTask(TaskStrategy.Custom);
            _lastSubscribeChannel = channel;

            _client.Subscribe(channel, _client_OnMessage);

            return _subscribeTask;
        }

        /// <summary>
        /// Begins unsubscription to the ORTC channel
        /// </summary>
        /// <param name="channel"></param>
        /// <returns>A task for the duration of the process</returns>
        public UnityTask Unsubscribe(string channel)
        {
            if (State != ConnectionState.Connected)
            {
                return UnityTask.FailedTask(new Exception("Not connected"));
            }

            if (GetSubscriptionState(channel) != SubscriptionState.Subscribed)
            {
                return UnityTask.FailedTask(new Exception("Not subscribed"));
            }

            _unsubscribeTask = new UnityTask(TaskStrategy.Custom);
            _lastUnsubscribeChannel = channel;

            _client.Unsubscribe(channel);

            return _unsubscribeTask;
        }

        /// <summary>
        /// Sends a message to the specific channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        /// <returns>A task for the duration of the process</returns>
        public UnityTask Send(string channel, string message)
        {
            if (State != ConnectionState.Connected)
            {
                return UnityTask.FailedTask(new Exception("Not connected"));
            }

            _client.Send(channel, message);

            return new UnityTask(TaskStrategy.Custom)
            {
                Status = TaskStatus.Success
            };
        }

        /// <summary>
        /// Returns the subscription state of the channel
        /// </summary>
        /// <param name="channel"></param>
        /// <returns>The current subscription state for the channel</returns>
        public SubscriptionState GetSubscriptionState(string channel)
        {
            if (!SubscriptionStates.ContainsKey(channel))
                return SubscriptionState.Unsubscribed;
            return SubscriptionStates[channel];
        }

        /// <summary>
        /// Adds a handler for a specific channel
        /// </summary>
        /// <param name="channel">the channel to listen to</param>
        /// <param name="action">the message received action handler </param>
        public void AddListener(string channel, OnChannelMessageDelegate action)
        {
            if (!Listeners.ContainsKey(channel))
                Listeners.Add(channel, new List<OnChannelMessageDelegate>());

            Listeners[channel].Add(action);
        }

        /// <summary>
        /// Removes a message handler for a specific channel
        /// </summary>
        /// <param name="channel">the channel to listen to</param>
        /// <param name="action">the message received action handler </param>
        public void RemoveListener(string channel, OnChannelMessageDelegate action)
        {
            if (!Listeners.ContainsKey(channel))
                return;

            Listeners[channel].Remove(action);
        }

        /// <summary>
        /// Wait coroutine for waiting for the connection state to == Connected
        /// </summary>
        /// <returns>A coroutine method</returns>
        public IEnumerator WaitForConnected()
        {
            while (State != ConnectionState.Connected)
            {
                yield return 1;
            }
        }
        #endregion

        #region authentication

        /// <summary>
        /// Posts an authentication token to the network.
        /// This token may then be used by a connecting client to gain access 
        /// </summary>
        /// <remarks>
        /// - Authentication may be disabled (if you want)
        /// - It is suggested you do not authenticate from the client, but, a webserver
        /// </remarks>
        /// <returns>A task with true if authenticated</returns>
        public UnityTask<bool> PostAuthentication(IEnumerable<RealtimePermission> permissions)
        {
            var p = new Dictionary<string, List<ChannelPermissions>>();

            foreach (var k in permissions)
            {
                if (!p.ContainsKey(k.Channel))
                    p.Add(k.Channel, new List<ChannelPermissions>());

                p[k.Channel].Add(k.Permission);
            }

            return AuthenticationClient.PostAuthentication(Url.Path, Url.IsCluster, AuthenticationToken, AuthenticationIsPrivate, ApplicationKey, AuthenticationTime, PrivateKey, p);
        }

        #endregion

        #region presence

        /// <summary>
        /// Returns the metadata for a channel
        /// </summary>
        /// <param name="authenticationToken">Current Authentication token</param>
        /// <param name="channel"></param>
        /// <returns>A task with the current presence state</returns>
        public UnityTask<Presence> GetPresence(string authenticationToken, string channel)
        {
            return PresenceClient.GetPresence(Url.Path, Url.IsCluster, ApplicationKey, authenticationToken, channel);
        }

        /// <summary>
        /// Enables Presence
        /// </summary>
        /// <param name="channel">channel to enable</param>
        /// <param name="metadata">If should collect the first 100 unique metadata</param>
        /// <returns>A task with the current presence state</returns>
        public UnityTask<string> EnablePresence(string channel, bool metadata)
        {
            return PresenceClient.EnablePresence(Url.Path, Url.IsCluster, ApplicationKey, PrivateKey, channel, metadata);
        }

        /// <summary>
        /// Disables Presence
        /// </summary>
        /// <param name="channel">channel to disable</param>
        /// <returns>A task with the current presence state</returns>
        public UnityTask<string> DisabledPresence(string channel)
        {
            return PresenceClient.DisablePresence(Url.Path, Url.IsCluster, ApplicationKey, PrivateKey, channel);
        }
        #endregion

        #region internal Methods

        void _client_OnUnsubscribed(string channel)
        {
            if (!SubscriptionStates.ContainsKey(channel))
                SubscriptionStates.Add(channel, SubscriptionState.Unsubscribed);
            else
                SubscriptionStates[channel] = SubscriptionState.Unsubscribed;

            if (_unsubscribeTask != null)
            {
                _unsubscribeTask.Status = TaskStatus.Success;
                _unsubscribeTask = null;
            }
        }

        void _client_OnSubscribed(string channel)
        {
            if (!SubscriptionStates.ContainsKey(channel))
                SubscriptionStates.Add(channel, SubscriptionState.Subscribed);
            else
                SubscriptionStates[channel] = SubscriptionState.Subscribed;

            if (_subscribeTask != null)
            {
                _subscribeTask.Status = TaskStatus.Success;
                _subscribeTask = null;
            }
        }

        void _client_OnException(Exception ex)
        {
            //UnityEngine.Debug.LogError("_client_OnException " + ex);
            if (_onException != null)
            {
                _onException(ex);
            }

            if (_connectionTask != null)
            {
                _connectionTask.Exception = ex;
                _connectionTask.Status = TaskStatus.Faulted;
            }

            if (_subscribeTask != null)
            {
                if (SubscriptionStates.ContainsKey(_lastSubscribeChannel))
                    SubscriptionStates.Add(_lastSubscribeChannel, SubscriptionState.Unsubscribed);

                // fault it
                _subscribeTask.Exception = ex;
                _subscribeTask.Status = TaskStatus.Faulted;
            }
            if (_unsubscribeTask != null)
            {
                if (SubscriptionStates.ContainsKey(_lastUnsubscribeChannel))
                    SubscriptionStates.Add(_lastUnsubscribeChannel, SubscriptionState.Unsubscribed);

                //fault it
                _unsubscribeTask.Exception = ex;
                _unsubscribeTask.Status = TaskStatus.Faulted;
            }
        }

        void _client_OnReconnecting()
        {
            State = ConnectionState.Reconnecting;
        }

        void _client_OnReconnected()
        {
            State = ConnectionState.Connected;

            if (_connectionTask != null)
            {
                _connectionTask.Status = TaskStatus.Success;
            }
        }

        void _client_OnDisconnected()
        {
            //UnityEngine.Debug.LogError("_client_OnDisconnected");
            if (State == ConnectionState.Pausing || State == ConnectionState.Paused)
                State = ConnectionState.Paused;
            else
                State = ConnectionState.Disconnected;
        }

        void _client_OnConnected()
        {
            //UnityEngine.Debug.LogError("_client_OnConnected");
            State = ConnectionState.Connected;

            if (_connectionTask != null)
            {
                _connectionTask.Status = TaskStatus.Success;
            }

            // Re apply
            var subs = SubscriptionStates.Where(o => o.Value == SubscriptionState.Paused).ToArray();
            foreach (var s in subs)
            {
                _client.Subscribe(s.Key, _client_OnMessage);
            }
        }

        void _client_OnMessage(string channel, string message)
        {
            if (_onMessage != null)
            {
                _onMessage(channel, message);
            }

            if (Listeners.ContainsKey(channel))
            {
                for (int i = 0;i < Listeners[channel].Count;i++)
                {
                    Listeners[channel][i](message);
                }
            }
        }

        #endregion
    }
}