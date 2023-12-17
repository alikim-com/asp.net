using Microsoft.EntityFrameworkCore;
using asp_net_sql.Models;

namespace asp_net_sql.Data;

internal class TicTacToeDbContext : DbContext
{
    internal DbSet<Player> Roster { get; set; }

    internal TicTacToeDbContext(DbContextOptions<TicTacToeDbContext> options) : base(options)
    {
    }
}
