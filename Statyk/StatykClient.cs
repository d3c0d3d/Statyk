using System;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Statyk
{
    public class StatykClient
    {
        public Uri Uri { get; set; }
        public bool Reconnect { get; set; }

        private const int ReceiveChunkSize = 1024;
        private const int SendChunkSize = 1024;
        private ClientWebSocket _ws;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _cancellationToken;

        private Action<StatykClient> _onConnected;
        private Action<StatykMessage, StatykClient> _onMessage;
        private Action<StatykClient> _onDisconnected;

        public bool IsConnected { get; private set; }

        public StatykClient(string uri)
        {
            if (uri.StartsWith("wss://"))
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            _ws = new ClientWebSocket();
            //_ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);
            Uri = new Uri(uri);
            _cancellationToken = _cancellationTokenSource.Token;
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="uri">The URI of the WebSocket server.</param>
        /// <returns></returns>
        public static StatykClient Create(string uri)
        {
            return new StatykClient(uri);
        }

        /// <summary>
        /// Connects to the WebSocket server.
        /// </summary>
        /// <returns></returns>
        public StatykClient Connect()
        {
            ConnectAsync();
            return this;
        }

        /// <summary>
        /// Set the Action to call when the connection has been established.
        /// </summary>
        /// <param name="onConnect">The Action to call.</param>
        /// <returns></returns>
        public StatykClient OnConnect(Action<StatykClient> onConnect)
        {
            _onConnected = onConnect;
            return this;
        }

        /// <summary>
        /// Set the Action to call when the connection has been terminated.
        /// </summary>
        /// <param name="onDisconnect">The Action to call</param>
        /// <returns></returns>
        public StatykClient OnDisconnect(Action<StatykClient> onDisconnect)
        {
            _onDisconnected = onDisconnect;
            return this;
        }

        /// <summary>
        /// Set the Action to call when a messages has been received.
        /// </summary>
        /// <param name="onMessage">The Action to call.</param>
        /// <returns></returns>
        public StatykClient OnMessage(Action<StatykMessage, StatykClient> onMessage)
        {
            _onMessage = onMessage;
            return this;
        }

        /// <summary>
        /// Close the connection with server
        /// </summary>
        public void Close()
        {
            _ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None).Wait();
            _ws.Dispose();

            IsConnected = false;
        }

        /// <summary>
        /// Send a string message to the WebSocket server.
        /// </summary>
        /// <param name="message">The message to send</param>
        public void SendMessage(string message)
        {
            SendMessageAsync(message, true);
        }

        /// <summary>
        /// Send a bytes message to the WebSocket server.
        /// </summary>
        /// <param name="message">The message to send</param>
        public void SendMessage(byte[] message)
        {
            SendMessageAsync(message, false);
        }

        private async void SendMessageAsync(object message, bool encodingMessage)
        {
            while (_ws.State == WebSocketState.Connecting) { };
            if (_ws.State != WebSocketState.Open)
            {
                throw new Exception("Connection is not open.");
            }

            var messageBuffer = encodingMessage ? Encoding.UTF8.GetBytes(message.ToString()) : message as byte[];
            var messagesCount = (int)Math.Ceiling((double)messageBuffer.Length / SendChunkSize);

            for (var i = 0; i < messagesCount; i++)
            {
                var offset = (SendChunkSize * i);
                var count = SendChunkSize;
                var lastMessage = ((i + 1) == messagesCount);

                if ((count * (i + 1)) > messageBuffer.Length)
                {
                    count = messageBuffer.Length - offset;
                }

                await _ws.SendAsync(new ArraySegment<byte>(messageBuffer, offset, count), encodingMessage ? WebSocketMessageType.Text : WebSocketMessageType.Binary, lastMessage, _cancellationToken);
            }
        }

        private void ConnectAsync()
        {
            try
            {
                _ws.ConnectAsync(Uri, _cancellationToken).Wait();
                //while (_ws.State == WebSocketState.Connecting) { };
                CallOnConnected();
                StartListen();
            }
            catch (WebSocketException)
            {
                if (Reconnect)
                {
                    Debug.WriteLine($"Reconnect in {Uri}");
                    _ws.Dispose();
                    _ws = new ClientWebSocket();
                    ConnectAsync();
                }
                else
                {
                    throw;

                }
            }
            catch (Exception ex)
            {
                if(ex?.InnerException is WebSocketException)
                {
                    if (Reconnect)
                    {
                        Debug.WriteLine($"Reconnect in {Uri}");
                        _ws.Dispose();
                        _ws = new ClientWebSocket();
                        ConnectAsync();
                    }
                    else
                    {
                        Debug.WriteLine(ex.InnerException);
                        throw ex.InnerException;

                    }
                }
                Debug.WriteLine(ex);
                throw;
            }

        }

        private async void StartListen()
        {
            var buffer = new byte[ReceiveChunkSize];

            try
            {
                while (_ws.State == WebSocketState.Open)
                {
                    IsConnected = true;

                    byte[] completeBuffer;
                    int bytesReceived = 0;
                    var stringResult = new StringBuilder();
                    WebSocketReceiveResult result;

                    using (MemoryStream memStream = new MemoryStream())
                    {
                        do
                        {
                            result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationToken);

                            memStream.Write(buffer, 0, result.Count);
                            bytesReceived += result.Count;

                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                                IsConnected = false;
                                CallOnDisconnected();
                            }
                            else
                            {
                                var str = Encoding.UTF8.GetString(buffer, 0, result.Count);
                                stringResult.Append(str);
                            }

                        } while (!result.EndOfMessage);

                        completeBuffer = memStream.ToArray();
                        Debug.WriteLine($"{bytesReceived} Bytes Received");
                    }
                    if (bytesReceived > 0)
                        CallOnMessage(new StatykMessage(
                            stringResult.ToString(),
                            completeBuffer,
                            result.MessageType == WebSocketMessageType.Binary));
                }
            }
            catch (Exception)
            {
                if (_ws != null || _ws.State == WebSocketState.Closed || _ws.State == WebSocketState.Aborted)
                {
                    if (IsConnected)
                        IsConnected = false;

                    CallOnDisconnected(); 
                }
            }
            finally
            {
                _ws.Dispose();
                IsConnected = false;
            }
        }

        private void CallOnMessage(StatykMessage webSocketMessage)
        {
            if (_onMessage != null)
                RunInTask(() => _onMessage(webSocketMessage, this));
        }

        private void CallOnDisconnected()
        {
            if (_onDisconnected != null)
                RunInTask(() => _onDisconnected(this));
        }

        private void CallOnConnected()
        {
            if (_onConnected != null)
                RunInTask(() => _onConnected(this));
        }

        private static void RunInTask(Action action)
        {
            Task.Factory.StartNew(action);
        }
    }
}
