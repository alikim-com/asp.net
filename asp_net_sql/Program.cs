using asp_net_sql.Data;
using asp_net_sql.GameEngine;
using Microsoft.EntityFrameworkCore;

namespace asp_net_sql;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorPages();

        builder.Services.AddDbContext<TicTacToe_Context>(options =>
           options.UseSqlServer(builder.Configuration.GetConnectionString("SQLExpressConnection")));

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

        var game = new Engine();
        game.Start();

        app.Run();
    }
}