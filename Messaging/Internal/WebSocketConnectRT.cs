// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------

using Windows.Web;
using UnityEngine;
#if UNITY_WSA
using Windows.Foundation;
using Windows.Storage.Streams;
using System.Threading;
using System;
using Windows.Networking.Sockets;

namespace Realtime.Messaging.Internal
{
    public class WebSocketConnection : IDisposable
    {
        #region Delegates (4)

        public delegate void OnOpenedDelegate();
        public delegate void OnClosedDelegate();
        public delegate void OnErrorDelegate(string error);
        public delegate void OnMessageReceivedDelegate(string message);

        #endregion

        #region Events (4)

        event OnOpenedDelegate _onOpened = delegate { };
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

        event OnClosedDelegate _onClosed = delegate { };
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

        event OnErrorDelegate _onError = delegate { };
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

        event OnMessageReceivedDelegate _onMessageReceived = delegate { };
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

        void MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            var ev = _onMessageReceived;

            if (ev == null)
                return;

            try
            {
                using (var reader = args.GetDataReader())
                {
                    reader.UnicodeEncoding = UnicodeEncoding.Utf8;
                    var read = reader.ReadString(reader.UnconsumedBufferLength);
                    ev(read);
                }
            }
            catch
            {

            }

        }

        void Closed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
            // You can add code to log or display the code and reason
            // for the closure (stored in args.Code and args.Reason)

            // This is invoked on another thread so use Interlocked 
            // to avoid races with the Start/Close/Reset methods.
            var webSocket = Interlocked.Exchange(ref streamWebSocket, null);
            if (webSocket != null)
            {
                webSocket.Dispose();
            }

            var ev = _onClosed;

            if (ev != null)
            {
                ev();
            }

            streamWebSocket = null;
        }

        void RaiseError(string m)
        {
            var ev = _onError;

            if (ev != null)
            {
                ev(m);
            }
        }

        #endregion

        #region Attributes (1)

        private MessageWebSocket pending;
        private MessageWebSocket streamWebSocket;
        private DataWriter messageWriter;

        #endregion

        #region Methods - Public (3)

        public async void Connect(string url)
        {
            if (pending != null)
            {
                pending.Dispose();
            }

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

            try
            {
                var prefix = "https".Equals(uri.Scheme) ? "wss" : "ws";

                var connectionUrl = new Uri(String.Format("{0}://{1}:{2}/broadcast/{3}/{4}/websocket", prefix, uri.DnsSafeHost, uri.Port, serverId, connectionId));


                pending = new MessageWebSocket();
                pending.Control.MessageType = SocketMessageType.Utf8;
                pending.Closed += Closed;
                pending.MessageReceived += MessageReceived;

                try
                {
                    await pending.ConnectAsync(connectionUrl);
                }
                catch(Exception ex)
                {
                    WebErrorStatus status = WebSocketError.GetStatus(ex.GetBaseException().HResult);
                    switch (status)
                    {
                        case WebErrorStatus.CannotConnect:
                            throw new Exception("Can't connect" + ex.Message);
                        case WebErrorStatus.NotFound:
                            throw new Exception("Not found" + ex.Message);
                        case WebErrorStatus.RequestTimeout:
                            throw new Exception("Request timeout" + ex.Message);
                        default:
                            throw new Exception("unknown" + ex.Message);
                    }
                }

                streamWebSocket = pending;
                messageWriter = new DataWriter(pending.OutputStream);

                var ev = _onOpened;
                if (ev != null)
                {
                    ev();
                }
            }
            catch
            {
                throw new OrtcException(OrtcExceptionReason.InvalidArguments, String.Format("Invalid URL: {0}", url));
            }
        }

        public void Close()
        {
            if (pending != null)
            {
                pending.Dispose();
                pending = null;
            }

            if (streamWebSocket != null)
            {
                streamWebSocket.Close(1000, "Normal closure");
                streamWebSocket.Dispose();
                streamWebSocket = null;
            }
        }

        public async void Send(string message)
        {

            if (streamWebSocket != null)
            {
                if (messageWriter != null)
                {
                    try
                    {
                        message = "\"" + message + "\"";
                        messageWriter.WriteString(message);
                        await ((IAsyncOperation<uint>)messageWriter.StoreAsync());
                    }
                    catch (Exception ex)
                    {
                        RaiseError("Send failed");
                        Close();
                    }
                }
            }
        }

        #endregion

        public void Dispose()
        {
            Close();
        }
    }
}
#endif