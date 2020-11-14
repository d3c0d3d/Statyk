using Statyk.HtmlGen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XStd.Net;

namespace Statyk
{
    public class StatykServer
    {
        private const int ReceiveChunkSize = 1024;
        private const int SendChunkSize = 1024;

        private readonly HttpListener _httpListener;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly CancellationToken _cancellationToken;

        private Action<WebSocket, StatykServer> _onConnected;
        private Action<WebSocket, StatykMessage, StatykServer> _onMessage;
        private Action<string, StatykServer> _onDisconnected;

        private readonly List<Type> _controllers = new();

        public StatykConnectionManager ConnectionManager { get; private set; }
        public string UriPrefix { get; private set; }
        public bool IsListen { get; private set; }
        public string RootPath { get; private set; }

        protected StatykServer(int port, bool isHttps = false)
        {
            UriPrefix = $"http{(isHttps ? "s" : "")}://localhost:{port}/";
            RootPath = Path.Combine(Util.AssemblyDirectory, "wwwroot");

            if (isHttps)
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(UriPrefix);
            _cancellationToken = _cancellationTokenSource.Token;

            ConnectionManager = new StatykConnectionManager();
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="uri">The URI of the WebSocket server.</param>
        /// <returns></returns>
        public static StatykServer Create(int port, bool isHttps = false)
        {
            return new StatykServer(port, isHttps);

        }

        /// <summary>
        /// Add type to Map to Controller
        /// </summary>
        /// <typeparam name="TType">type ex: IndexController</typeparam>
        /// <returns></returns>
        public StatykServer AddController<TType>() where TType : class
        {
            _controllers.Add(typeof(TType));

            return this;
        }

        /// <summary>
        /// Listener WebSocket connections.
        /// </summary>
        /// <returns></returns>
        public StatykServer Listen()
        {
            StartListen();
            return this;
        }

        /// <summary>
        /// Set the Action to call when the connection has been established.
        /// </summary>
        /// <param name="onConnect">The Action to call.</param>
        /// <returns></returns>
        public StatykServer OnConnect(Action<WebSocket, StatykServer> onConnect)
        {
            _onConnected = onConnect;
            return this;
        }

        /// <summary>
        /// Set the Action to call when the connection has been terminated.
        /// </summary>
        /// <param name="onDisconnect">The Action to call</param>
        /// <returns></returns>
        public StatykServer OnDisconnect(Action<string, StatykServer> onDisconnect)
        {
            _onDisconnected = onDisconnect;
            return this;
        }

        /// <summary>
        /// Set the Action to call when a messages has been received.
        /// </summary>
        /// <param name="onMessage">The Action to call.</param>
        /// <returns></returns>
        public StatykServer OnMessage(Action<WebSocket, StatykMessage, StatykServer> onMessage)
        {
            _onMessage = onMessage;
            return this;
        }

        /// <summary>
        /// Send a string message to the WebSocket server.
        /// </summary>
        /// <param name="message">The message to send</param>
        public void SendMessage(WebSocket webSocket, string message)
        {
            SendMessageAsync(webSocket, message, true);
        }

        /// <summary>
        /// Send a bytes message to the WebSocket server.
        /// </summary>
        /// <param name="message">The message to send</param>
        public void SendMessage(WebSocket webSocket, byte[] message)
        {
            SendMessageAsync(webSocket, message, false);
        }

        private async void SendMessageAsync(WebSocket webSocket, object message, bool encodingMessage)
        {
            while (webSocket.State == WebSocketState.Connecting) { };
            if (webSocket.State != WebSocketState.Open)
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

                await webSocket.SendAsync(new ArraySegment<byte>(messageBuffer, offset, count), encodingMessage ? WebSocketMessageType.Text : WebSocketMessageType.Binary, lastMessage, _cancellationToken);
            }
        }

        private void StartListen()
        {
            try
            {
                _httpListener.Start();
                IsListen = _httpListener.IsListening;
                Debug.WriteLine("Listener Started");

                _ = Task.Run(async () =>
                {
                    while (true)
                    {
                        HttpListenerContext context = await _httpListener.GetContextAsync();

                        _ = Task.Run(() =>
                        {
                            ProcessHttpContext(context);
                        });
                        if (context.Request.IsWebSocketRequest)
                        {
                            HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
                            WebSocket socket = webSocketContext.WebSocket;

                            ConnectionManager.AddSocket(socket, webSocketContext);
                            Debug.WriteLine($"Client: {ConnectionManager.GetId(socket)} Connected", ConsoleColor.Yellow);
                            CallOnConnected(socket);

                            _ = Task.Run(async () =>
                            {
                                await ProcessWebSocketMessageAsync(socket);

                            });
                        }
                    }
                });

            }
            catch (Exception ex)
            {
                var error = Util.GetFullError(ex);
                var stack = Util.GetFullStackTraceError(ex);
                var msg = $"{error}{stack}";
                Debug.WriteLine(msg);
            }
        }

        private async Task ProcessWebSocketMessageAsync(WebSocket socket)
        {
            try
            {
                var buffer = new byte[ReceiveChunkSize];

                while (socket.State == WebSocketState.Open)
                {
                    byte[] completeBuffer;
                    int bytesReceived = 0;
                    var stringResult = new StringBuilder();
                    WebSocketReceiveResult result;

                    using (MemoryStream memStream = new())
                    {
                        do
                        {
                            result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationToken);

                            memStream.Write(buffer, 0, result.Count);
                            bytesReceived += result.Count;

                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                                CallOnDisconnected(ConnectionManager.GetId(socket));
                                await ConnectionManager.RemoveSocket(ConnectionManager.GetId(socket));
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
                        CallOnMessage(socket, new StatykMessage(
                            stringResult.ToString(),
                            completeBuffer,
                            result.MessageType == WebSocketMessageType.Binary));
                }
            }
            catch (WebSocketException ex)
            {
                var error = Util.GetFullError(ex);
                var stack = Util.GetFullStackTraceError(ex);
                var msg = $"{error}{stack}";
                Debug.WriteLine(msg);

                if (socket != null && socket.State == WebSocketState.Aborted)
                {
                    CallOnDisconnected(ConnectionManager.GetId(socket));
                    await ConnectionManager.RemoveSocket(ConnectionManager.GetId(socket));
                }
                if (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                    if (socket != null)
                    {
                        CallOnDisconnected(ConnectionManager.GetId(socket));
                        await ConnectionManager.RemoveSocket(ConnectionManager.GetId(socket));
                    }
            }
            catch (Exception ex)
            {
                var error = Util.GetFullError(ex);
                var stack = Util.GetFullStackTraceError(ex);
                var msg = $"{error}{stack}";
                Debug.WriteLine(msg);
            }
        }

        private void ProcessHttpContext(HttpListenerContext context)
        {
            var response = context.Response;
            string controllerName = null;
            string methodName = null;
            string id = null;

            try
            {
                // {controller}/{?action(Get|Post|Put|Delete)}/{?id}
                for (int i = 0; i < context.Request.Url.Segments.Length; i++)
                {
                    var seg = context.Request.Url.Segments[i].Replace("/", string.Empty);
                    switch (i)
                    {
                        case 1:
                            controllerName = seg;
                            break;
                        case 2:
                            methodName = seg;
                            break;
                        case 3:
                            id = seg;
                            break;
                        default:
                            break;
                    }
                }

                var (controller, method) = ResolverControllerInstance(context, controllerName, methodName);
                if (controller != null)
                {
                    if (method != null)
                    {
                        object invokerResult = InvokeControllerInstance(context, controller, method, id);

                        if (invokerResult != null)
                        {
                            response.ContentType = ResolverContentType(invokerResult);
                        }

                        SendResponse(response, invokerResult);
                    }
                    else
                        TemplatePage.NotFound(response, methodName);

                }
                else
                {
                    if (string.IsNullOrEmpty(controllerName))
                        TemplatePage.WelcomePage(response);
                    else
                    {
                        var searchFile = Path.Combine(RootPath, controllerName);
                        if (File.Exists(searchFile))
                        {
                            SendResponse(response, File.ReadAllBytes(searchFile));
                        }
                        else
                            TemplatePage.NotFound(response, controllerName);
                    }
                }
            }
            catch (Exception ex)
            {
                var error = Util.GetFullError(ex);
                var stack = Util.GetFullStackTraceError(ex);
                var msg = $"{error}{stack}";

                //SendResponse(response, msg, 500);
                TemplatePage.ServerInternalError(response, msg);

                Debug.WriteLine(msg);
            }
        }

        private (Type Controller, MethodInfo Method) ResolverControllerInstance(HttpListenerContext context, string controllerName, string methodName)
        {
            MethodInfo method = null;
            var contrName = controllerName ?? "index";
            var controller = _controllers.FirstOrDefault(x => x.Name.ToLower().Replace("controller", string.Empty) == contrName.ToLower());

            if (controller != null)
            {
                var req = context.Request;
                switch (req.HttpMethod)
                {
                    case "GET":

                        if (methodName == null)
                        {
                            methodName = "get";
                            method = controller.GetMethods().FirstOrDefault(p => p.Name.ToLower() == methodName);
                        }
                        else
                            method = controller.GetMethods()
                                      .Where(mi => mi.GetCustomAttributes(true)
                                      .Any(attr => attr is GET get && get.Router.ToLower() == methodName))
                                      .FirstOrDefault();
                        break;
                    case "POST":
                        if (methodName == null)
                        {
                            methodName = "post";
                            method = controller.GetMethods().FirstOrDefault(p => p.Name.ToLower() == methodName);
                        }
                        else
                            method = controller.GetMethods()
                                  .Where(mi => mi.GetCustomAttributes(true)
                                  .Any(attr => attr is POST post && post.Router.ToLower() == methodName))
                                  .FirstOrDefault();
                        break;
                    case "PUT":
                        if (methodName == null)
                        {
                            methodName = "put";
                            method = controller.GetMethods().FirstOrDefault(p => p.Name.ToLower() == methodName);
                        }
                        else
                            method = controller.GetMethods()
                                  .Where(mi => mi.GetCustomAttributes(true)
                                  .Any(attr => attr is PUT put && put.Router.ToLower() == methodName))
                                  .FirstOrDefault();
                        break;
                    case "DELETE":
                        if (methodName == null)
                        {
                            methodName = "delete";
                            method = controller.GetMethods().FirstOrDefault(p => p.Name.ToLower() == methodName);
                        }
                        else
                            method = controller.GetMethods()
                                  .Where(mi => mi.GetCustomAttributes(true)
                                  .Any(attr => attr is DELETE delete && delete.Router.ToLower() == methodName))
                                  .FirstOrDefault();
                        break;
                    default:
                        // todo return 501
                        break;
                }

                return (controller, method);
            }

            return (controller, method);
        }

        private object InvokeControllerInstance(HttpListenerContext context, Type controller, MethodInfo method, string id)
        {
            List<object> paramsList = new();

            string bodyValue;
            var req = context.Request;
            var queryStrings = req?.QueryString;

            if (req.InputStream != null)
            {
                var reader = new StreamReader(req.InputStream, req.ContentEncoding);
                bodyValue = reader.ReadToEnd();

                var bodyParam = method.GetParameters().Where(c => c.GetCustomAttributes(true).Any(attr => attr is FromBody)).FirstOrDefault();
                if (bodyParam != null)
                {
                    paramsList.Add(Convert.ChangeType(bodyValue, bodyParam.ParameterType));
                }
            }
            if (id != null)
            {
                var idParam = method.GetParameters().FirstOrDefault(p => p.Name.ToLower() == "id");
                if (idParam != null)
                {
                    paramsList.Add(Convert.ChangeType(id, idParam.ParameterType));
                }
            }
            if (queryStrings.Count > 0)
            {
                for (int i = 0; i < queryStrings.Count; i++)
                {
                    var parameter = method.GetParameters().FirstOrDefault(x => x.Name.ToLower() == queryStrings.GetKey(i).ToLower());
                    if (parameter != null)
                    {
                        paramsList.Add(Convert.ChangeType(queryStrings.Get(i), parameter.ParameterType));
                    }
                }
            }

            var instance = Activator.CreateInstance(controller);
            object result = method.Invoke(instance, paramsList.Count > 0 ? paramsList.ToArray() : null);

            return result;
        }

        private static void SendResponse(HttpListenerResponse response, object content = null, int code = 200)
        {
            byte[] buffer = null;

            if (content == null)
            {
                buffer = Encoding.UTF8.GetBytes(string.Empty);
            }
            else
            {
                switch (content)
                {
                    case string:
                        buffer = Encoding.UTF8.GetBytes(content as string);
                        break;
                    case byte[]:
                        buffer = content as byte[];
                        break;
                    case object:
                        buffer = Encoding.UTF8.GetBytes(content.ToString());
                        break;
                    default:
                        break;
                }               
            }

            response.Headers.Add(HttpResponseHeader.Server, string.Empty);

            response.AddHeader("_server", nameof(Statyk));
            response.ContentLength64 = buffer.Length;
            response.StatusCode = code;
            var output = response.OutputStream;

            try
            {
                Debug.WriteLine("Buffer Length: " + buffer.Length);
                output.Write(buffer, 0, buffer.Length);
            }
            finally
            {
                output.Close();
                response.Close();
            }
        }

        private static string GetContentType(string ext) => ext switch
        {
            "js" => "text/javascript",
            "html" => "text/html",
            "css" => "text/css",
            "png" => "image/png",
            "xml" => "text/xml",
            "txt" => "text/plain",
            "gif" => "image/gif",
            "jpg" => "image/jpg",
            "jpeg" => "image/jpeg",
            "ico" => "image/x-icon",
            "icon" => "image/x-icon",
            "zip" => "application/zip",
            "woff" => "application/x-font-woff",
            "json" => "application/json",
            _ => null,
        };

        private static string ResolverContentType(object obj)
        {
            if (obj is HtmlNode)
                return GetContentType("html");
            if (obj is string)
            {
                var str = obj.ToString();

                // html?
                if (str.StartsWith("<"))
                    return GetContentType("html");

                // json ?
                if (str.IsValidJson())
                    return GetContentType("json");
            }

            return null;
        }

        private void CallOnMessage(WebSocket webSocket, StatykMessage webSocketMessage)
        {
            if (_onMessage != null)
                RunInTask(() => _onMessage(webSocket, webSocketMessage, this));
        }

        private void CallOnDisconnected(string key)
        {
            if (_onDisconnected != null)
                RunInTask(() => _onDisconnected(key, this));
        }

        private void CallOnConnected(WebSocket webSocket)
        {
            if (_onConnected != null)
                RunInTask(() => _onConnected(webSocket, this));
        }

        private static void RunInTask(Action action)
        {
            Task.Factory.StartNew(action);
        }
    }
}