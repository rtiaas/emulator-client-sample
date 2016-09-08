namespace EmulatorClientSample
{
    using System;
    using System.Collections.Concurrent;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class Client
    {
        private ConcurrentQueue<string> _received = new ConcurrentQueue<string>();
        private ConcurrentQueue<byte[]> _sendqueue = new ConcurrentQueue<byte[]>();

        private Task _receiveTask;
        private Task _sendTask;

        private Uri _uri;

        public Client(string url)
        {
            _uri = new Uri(url);
        }

        public async Task Connect(string bearerToken)
        {
            var webSocket = new ClientWebSocket();
            webSocket.Options.SetRequestHeader("Authorization", $"Bearer {bearerToken}");

            _receiveTask = Receive(webSocket);
            _sendTask = Send(webSocket);

            await webSocket.ConnectAsync(_uri, CancellationToken.None);
        }

        public bool TryGetReceivedMessage(out string message)
        {
            message = null;

            if (_received.Count == 0)
                return false;

            return _received.TryDequeue(out message);
        }

        public void Send(byte[] message)
        {
            _sendqueue.Enqueue(message);
        }

        private async Task Send(ClientWebSocket webSocket)
        {
            while (true)
            {
                byte[] message;
                if (_sendqueue.TryPeek(out message))
                {
                    if (webSocket.State == WebSocketState.Open)
                    {
                        var sendBuffer = new ArraySegment<byte>(message);

                        await webSocket.SendAsync(sendBuffer, WebSocketMessageType.Binary, true, CancellationToken.None);

                        _sendqueue.TryDequeue(out message);
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }

        private async Task Receive(ClientWebSocket webSocket)
        {
            var buffer = new byte[1024 * 32];

            while (true)
            {
                if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseSent)
                {
                    var receiveBuffer = new ArraySegment<byte>(buffer);
                    var result = await webSocket.ReceiveAsync(receiveBuffer, CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        if (webSocket.State == WebSocketState.Open)
                        {
                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, String.Empty, CancellationToken.None);
                        }

                        _received.Enqueue($"Connection closed. Reason - {result.CloseStatusDescription}");
                        break;
                    }
                    else
                    {
                        var content = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        _received.Enqueue(content);
                    }
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }
    }
}
