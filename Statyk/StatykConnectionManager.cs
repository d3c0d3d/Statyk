using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Statyk
{
    public class StatykConnectionManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();
        public WebSocket GetSocketById(string id)
        {
            return _sockets.FirstOrDefault(p => p.Key == id).Value;
        }

        public ConcurrentDictionary<string, WebSocket> GetAll()
        {
            return _sockets;
        }

        public string GetId(WebSocket socket)
        {
            return _sockets.FirstOrDefault(p => p.Value == socket).Key;
        }
        public void AddSocket(WebSocket socket, HttpListenerWebSocketContext context)
        {
            var sId = CreateConnectionId();

            while (!_sockets.TryAdd(sId, socket))
            {
                sId = CreateConnectionId();
            }
        }

        public async Task RemoveSocket(string id)
        {
            try
            {
                _sockets.TryRemove(id, out WebSocket socket);
                await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
            }
            catch (Exception)
            {

            }

        }

        private static string CreateConnectionId()
        {
            return XStd.Nanoid.Generate("0123456789abcdef",7);
        }
    }
}
