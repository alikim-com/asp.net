using Microsoft.EntityFrameworkCore;
using asp_net_sql.Models;

namespace asp_net_sql.Data;

/// <summary>
/// 
/// CMD COMMANDS
/// 
/// creates this file along with table models from existing SQL
/// > dotnet ef dbcontext scaffold "Name=ConnectionStrings:SQLExpressConnection" Microsoft.EntityFrameworkCore.SqlServer --use-database-names -o ScaffoldModels
/// 
/// creates Migration folder with files out of Models classes and this one
/// > dotnet ef migrations add InitialCreate
/// 
/// applies migration files
/// > dotnet ef database update
/// 
/// </summary>
internal partial class TicTacToeDbContext : DbContext
{
    //internal DbSet<RowRosterEnum> TableRosterEnum { get; set; }
    //internal DbSet<RowRoster> TableRoster { get; set; }

    public virtual DbSet<ChildTable> ChildTables { get; set; }
    public virtual DbSet<ParentTable> ParentTables { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChildTable>(entity =>
        {
            entity.HasKey(e => e.ChildID).HasName("PK_CT_ChildID");

            entity.ToTable("ChildTable");

            entity.Property(e => e.ChildID).ValueGeneratedNever();

            entity.HasOne(d => d.Child).WithOne(p => p.ChildTable)
                .HasPrincipalKey<ParentTable>(p => p.UniColumn)
                .HasForeignKey<ChildTable>(d => d.ChildID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChildID__PT_UniColumn");
        });

        modelBuilder.Entity<ParentTable>(entity =>
        {
            entity.HasKey(e => e.ParentID).HasName("PK_PT_ParentID");

            entity.ToTable("ParentTable");

            entity.HasIndex(e => e.UniColumn, "UQ_PT_UniColumn").IsUnique();

            entity.Property(e => e.ParentID).ValueGeneratedNever();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    public TicTacToeDbContext(DbContextOptions<TicTacToeDbContext> options) : base(options)
    {
    }
}
