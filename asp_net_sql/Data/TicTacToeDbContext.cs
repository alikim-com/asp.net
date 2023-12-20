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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RowRoster>()
            .HasOne(x => x.RowRosterEnum)
            .WithOne(x => x.RowRoster)
            .HasPrincipalKey<RowRosterEnum>(x => x.UniColumn);
    }
}
