using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;
//
using asp_net_sql.Data;
using asp_net_sql.Common;
using System.Diagnostics;

namespace asp_net_sql;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorPages();

        builder.Services.AddDbContext<TicTacToe_Context>(options =>
           options.UseSqlServer(builder.Configuration.GetConnectionString("SQLExpressConnection")));

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");

            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();

        // websocket endpoint

        var webSocketOptions = new WebSocketOptions
        {
            KeepAliveInterval = TimeSpan.FromMinutes(2)
        };

        app.UseWebSockets(webSocketOptions);

        app.Map("/ws", async context => {

            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                var tcs = new TaskCompletionSource<object>();

                var _guid = WebSockHub.Add(webSocket, tcs);

                Debug.WriteLine($"{_guid.ShortStr()} wsRunner added");

                await tcs.Task;

                Debug.WriteLine($"{_guid.ShortStr()} wsRunner TCS released");

            } else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.Headers.Append("Voicemail", "Thank you for reaching out to us. Unfortunately, we only serve websocket clients at this address.");
            }

        });

        //

        app.Run();
    }
}