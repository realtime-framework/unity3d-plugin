// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------

#if !UNITY_WSA
using System;
using WebSocketSharp;

namespace Realtime.Messaging.Internal
{
    public class WebSocketConnection : IDisposable
    {
        #region Attributes (1)

        WebSocket _websocket;

        #endregion

        #region Methods - Public (3)

        public void Connect(string url)
        {
            Uri uri;

            var connectionId = Strings.RandomString(8);
            var serverId = Strings.RandomNumber(1, 1000);

            try
            {
                uri = new Uri(url);
            }
            catch (Exception)
            {
                throw new OrtcException(OrtcExceptionReason.InvalidArguments, String.Format("Invalid URL: {0}", url));
            }

            var prefix = "https".Equals(uri.Scheme) ? "wss" : "ws";

            var connectionUrl = new Uri(String.Format("{0}://{1}:{2}/broadcast/{3}/{4}/websocket", prefix, uri.DnsSafeHost, uri.Port, serverId, connectionId));

            if (_websocket != null)
                Dispose();

            _websocket = new WebSocket(connectionUrl.AbsoluteUri);

            _websocket.OnOpen += _websocket_OnOpen;
            _websocket.OnError += _websocket_OnError;
            _websocket.OnClose += _websocket_OnClose;
            _websocket.OnMessage += _websocket_OnMessage;

            _websocket.Connect();
        }

        public void Close()
        {
            if (_websocket != null)
            {
                _websocket.Close();
            }
        }

        public void Send(string message)
        {
            if (_websocket != null)
            {
                // Wrap in quotes, escape inner quotes
                _websocket.Send(string.Format("\"{0}\"", message.Replace("\"", "\\\"")));
            }
        }

        #endregion

#region Methods - Private (1)

        #endregion

#region Delegates (4)

        public delegate void OnOpenedDelegate();
        public delegate void OnClosedDelegate();
        public delegate void OnErrorDelegate(string error);
        public delegate void OnMessageReceivedDelegate(string message);

        #endregion

#region Events (4)

        event OnOpenedDelegate _onOpened;
        public event OnOpenedDelegate OnOpened
        {
            add
            {
                _onOpened = (OnOpenedDelegate)Delegate.Combine(_onOpened, value);
            }

            remove
            {
                _onOpened = (OnOpenedDelegate)Delegate.Remove(_onOpened, value);
            }
        }
        
        event OnClosedDelegate _onClosed;
        public event OnClosedDelegate OnClosed
        {
            add
            {
                _onClosed = (OnClosedDelegate)Delegate.Combine(_onClosed, value);
            }

            remove
            {
                _onClosed = (OnClosedDelegate)Delegate.Remove(_onClosed, value);
            }
        }

        event OnErrorDelegate _onError;
        public event OnErrorDelegate OnError
        {
            add
            {
                _onError = (OnErrorDelegate)Delegate.Combine(_onError, value);
            }

            remove
            {
                _onError = (OnErrorDelegate)Delegate.Remove(_onError, value);
            }
        }

        event OnMessageReceivedDelegate _onMessageReceived;
        public event OnMessageReceivedDelegate OnMessageReceived
        {
            add
            {
                _onMessageReceived = (OnMessageReceivedDelegate)Delegate.Combine(_onMessageReceived, value);
            }

            remove
            {
                _onMessageReceived = (OnMessageReceivedDelegate)Delegate.Remove(_onMessageReceived, value);
            }
        }
        
        #endregion

#region Events Handles (4)

        void _websocket_OnMessage(object sender, MessageEventArgs e)
        {
            var ev = _onMessageReceived;

            if (ev != null)
            {
                ev(e.Data);
            }
        }

        void _websocket_OnClose(object sender, CloseEventArgs e)
        {
            var ev = _onClosed;

            if (ev != null)
            {
                ev();
            }
        }

        void _websocket_OnError(object sender, ErrorEventArgs e)
        {
            var ev = _onError;

            if (ev != null)
            {
                ev(e.Message);
            }
        }

        void _websocket_OnOpen(object sender, EventArgs e)
        {
            var ev = _onOpened;

            if (ev != null)
            {
                ev();}
        }


        #endregion

        public void Dispose()
        {
           if (_websocket != null)
           {
               _websocket.Close();
               _websocket.OnOpen -= _websocket_OnOpen;
               _websocket.OnError -= _websocket_OnError;
               _websocket.OnClose -= _websocket_OnClose;
               _websocket.OnMessage -= _websocket_OnMessage;
           }

            _websocket = null;
        }
    }
}
#endif