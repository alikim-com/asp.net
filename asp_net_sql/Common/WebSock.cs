using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace asp_net_sql.Common;

public class WebSock(WebSocket _socket, int bufferSize)
{
    readonly WebSocket socket = _socket;
    readonly byte[] buffer = new byte[bufferSize];

    async Task Send(APIPacket packet)
    {
        var json = JsonSerializer.Serialize(packet, Post.includeFields);

        byte[] buffer = Encoding.UTF8.GetBytes(json);

        await socket.SendAsync(
            new ArraySegment<byte>(buffer),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }

    public async Task SocketLoop(WebSocket _socket, HttpContext context)
    {
        
        WebSocketReceiveResult res;
        APIPacket resp;

        do
        {
            res = await socket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

            if (res.MessageType == WebSocketMessageType.Text)
            {
                var json = Encoding.UTF8.GetString(buffer, 0, res.Count);

                resp = new APIPacket();

                try
                {
                    var wsData = JsonSerializer.Deserialize<APIPacket>
                        (json, Post.includeFields) ?? throw new Exception
                            ($"WebSock.OnReceiveAsync : wsData is null");

                    switch (wsData.command)
                    {
                        case APICmd.Test:
                            resp.status = "success";
                            resp.message = "received packet";
                            resp.command = wsData.command;
                            resp.keyValuePairs = wsData.keyValuePairs;
                            break;

                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    resp.status = "error";
                    resp.message = "exception";
                    resp.info["info"] = [];
                    resp.AddExeptionInfo("info", ex);
                }

                await Send(resp);
            }

            

        } while (!res.CloseStatus.HasValue);

        await socket.CloseAsync(
            res.CloseStatus.Value,
            res.CloseStatusDescription,
            CancellationToken.None);
    }
}
