using System.Net.WebSockets;
using System.Text;

namespace asp_net_sql.Common;

public class WebSock
{
    public static async Task SocketLoop(WebSocket socket, HttpContext context)
    {
        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult res;

        do
        {
            res = await socket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

            if (res.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, res.Count);
                // json.encode into APIPacket
            }

            // APIPAcket to buffer
            await socket.SendAsync(
                new ArraySegment<byte>(buffer, 0, res.Count),
                res.MessageType,
                res.EndOfMessage,
                CancellationToken.None);

        } while (!res.CloseStatus.HasValue);

        await socket.CloseAsync(
            res.CloseStatus.Value,
            res.CloseStatusDescription,
            CancellationToken.None);
    }
}
