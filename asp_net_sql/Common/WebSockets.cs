
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
//
using TCS = System.Threading.Tasks.TaskCompletionSource<object>;
//
namespace asp_net_sql.Common;

public class WebSockHub
{
    static public readonly List<WebSockRunner> wsRunners = [];

    public static Guid Add(WebSocket _webSocket, TCS _tcs)
    {
        var _guid = Guid.NewGuid();
        var rnr = new WebSockRunner(_webSocket, _tcs, _guid);
        wsRunners.Add(rnr);
        rnr.OpenLoop();

        WelcomeRunner(rnr);
        UpdateRunners();

        return _guid;
    }

    public static void Remove(WebSockRunner wsRunner)
    {
        wsRunners.Remove(wsRunner);
        UpdateRunners();
    }

    static void WelcomeRunner(WebSockRunner wsRunner)
    {
        Dictionary<string, string> dict = new() { 
            { wsRunner.name, "server: connected" } };
            // { wsRunner.name, wsRunner.Tcs.Task.Status.ToString() } };

        var packet = new Packet(
            PackStat.None,
            PackCmd.Welcome,
            dict,
            "");

        wsRunner.SendWelcome(packet);
    }

    static void UpdateRunners()
    {
        Dictionary<string, string> dict = [];
        foreach (var rnr in wsRunners)
            dict.Add(rnr.name, "server: connected");

        var packet = new Packet(
            PackStat.None,
            PackCmd.Update,  
            dict,
            "wsRunners");

        foreach (var rnr in wsRunners) 
            rnr.SendUpdate(packet);
    }
}

public class WebSockRunner(
    WebSocket _socket,
    TCS _tcs,
    Guid _guid,
    int bufferSize = 4 * 1024)
{
    readonly WebSocket socket = _socket;
    public TCS Tcs { get; } = _tcs;
    readonly Guid guid = _guid;
    public readonly string name = _guid.ShortStr();
    readonly byte[] buffer = new byte[bufferSize];

    public void SendWelcome(Packet packet) => Send(packet);
    public void SendUpdate(Packet packet) => Send(packet);

    async void Send(Packet packet)
    {
        var json = JsonSerializer.Serialize(packet, Post.includeFields);

        byte[] buffer = Encoding.UTF8.GetBytes(json);

        await socket.SendAsync(
            new ArraySegment<byte>(buffer),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }

    public async void OpenLoop()
    {

        WebSocketReceiveResult res;
        Packet resp;

        do
        {
            res = await socket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

            if (res.MessageType == WebSocketMessageType.Text)
            {
                var json = Encoding.UTF8.GetString(buffer, 0, res.Count);

                resp = new Packet();

                try
                {
                    var wsData = JsonSerializer.Deserialize<Packet>
                        (json, Post.includeFields) ?? throw new Exception
                            ($"WebSock.OnReceiveAsync : wsData is null");

                    switch (wsData.command)
                    {
                        case PackCmd.Test:
                            resp.status = PackStat.Success;
                            resp.message = "bouncing back packet";
                            resp.command = wsData.command;
                            resp.keyValuePairs = wsData.keyValuePairs;
                            break;

                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    resp.status = PackStat.Fail;
                    resp.message = $"wsRunner<{name}>.OpenLoop";
                    resp.AddExInfo(ex);
                }

                Send(resp);
            }

        } while (!res.CloseStatus.HasValue);

        await socket.CloseAsync( // CHECK IF WORKS WHEN TAB IS CLOSED
            res.CloseStatus.Value,
            res.CloseStatusDescription,
            CancellationToken.None);

        WebSockHub.Remove(this);

        // release middleware
        Tcs.SetResult(new { });
    }
}
