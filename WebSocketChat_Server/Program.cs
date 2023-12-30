using System.Net;
using System.Net.WebSockets;
using System.Text;

class WebSocketChatServer
{
    private readonly HttpListener _listener;
    private readonly List<WebSocket> _clients;

    public WebSocketChatServer(string url)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add(url);
        _clients = new List<WebSocket>();
    }

    public async Task Start()
    {
        _listener.Start();
        Console.WriteLine($"Server started. Listening on {_listener.Prefixes.FirstOrDefault()}...");

        while (true)
        {
            var context = await _listener.GetContextAsync();
            if (context.Request.IsWebSocketRequest)
            {
                _ = Task.Run(() => ProcessWebSocketRequest(context));
            }
            else
            {
                context.Response.StatusCode = 400;
                context.Response.Close();
            }
        }

    }

    private async Task ProcessWebSocketRequest(HttpListenerContext context)
    {
        try
        {
            var webSocketContext = await context.AcceptWebSocketAsync(null);
            var socket = webSocketContext.WebSocket;

            Console.WriteLine($"WebSocket connected: {context.Request.RemoteEndPoint}");
            _clients.Add(socket);

            var buffer = new byte[1024];

            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var receivedData = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine($"Received message from {context.Request.RemoteEndPoint}: {receivedData}");

                        var sendData = Encoding.UTF8.GetBytes($"{context.Request.RemoteEndPoint}: {receivedData}");
                        var sendTasks = new List<Task>();
                        foreach (var client in _clients)
                        {
                            if (client.State == WebSocketState.Open)
                            {
                                sendTasks.Add(client.SendAsync(new ArraySegment<byte>(sendData, 0, sendData.Length), WebSocketMessageType.Text, true, CancellationToken.None));
                            }
                        }
                        await Task.WhenAll(sendTasks);
                    }
                }
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"WebSocket disconnected: {context.Request.RemoteEndPoint}. Reason: {ex.Message}");

                _clients.Remove(socket);

                NotifyDisconnectionToOtherClients(context.Request.RemoteEndPoint);
            }
            finally
            {
                _clients.Remove(socket);
                Console.WriteLine($"WebSocket disconnected: {context.Request.RemoteEndPoint}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing WebSocket request: {ex.Message}");
        }
    }

    private void NotifyDisconnectionToOtherClients(EndPoint disconnectedClient)
    {
        var message = Encoding.UTF8.GetBytes($"User at {disconnectedClient} has disconnected");
        var sendTasks = new List<Task>();
        foreach (var client in _clients)
        {
            if (client.State == WebSocketState.Open)
            {
                sendTasks.Add(client.SendAsync(new ArraySegment<byte>(message, 0, message.Length), WebSocketMessageType.Text, true, CancellationToken.None));
            }
        }
        Task.WhenAll(sendTasks).Wait();
    }
}

class Program
{
    static async Task Main()
    {
        var server = new WebSocketChatServer("http://localhost:8080/");
        await server.Start();
    }
}