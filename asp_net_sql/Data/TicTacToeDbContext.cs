using Microsoft.EntityFrameworkCore;
using asp_net_sql.Models;

namespace asp_net_sql.Data;

/// <summary>
/// dotnet ef migrations add InitialCreate
/// dotnet ef database update
/// </summary>
internal class TicTacToeDbContext : DbContext
{
    internal DbSet<RowRosterEnum> TableRosterEnum { get; set; }
    internal DbSet<RowRoster> TableRoster { get; set; }

    public TicTacToeDbContext(DbContextOptions<TicTacToeDbContext> options) : base(options)
    {
    }
}
