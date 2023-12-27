using asp_net_sql.Models;
using Microsoft.EntityFrameworkCore;

namespace asp_net_sql.Data;

public partial class TicTacToe_Context : DbContext
{
    public TicTacToe_Context()
    {
    }

    public TicTacToe_Context(DbContextOptions<TicTacToe_Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Chosen> Chosens { get; set; }

    public virtual DbSet<EnumBtnMsg> EnumBtnMsgs { get; set; }

    public virtual DbSet<EnumGameRoster> EnumGameRosters { get; set; }

    public virtual DbSet<EnumGameState> EnumGameStates { get; set; }

    public virtual DbSet<EnumUISide> EnumUISides { get; set; }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<GameBoard> GameBoards { get; set; }

    public virtual DbSet<MenuString> MenuStrings { get; set; }

    public virtual DbSet<ResxString> ResxStrings { get; set; }

    /// <summary>
    /// Provides matching DbSet<T> for generic admin page model classes and Admin menu
    /// </summary>
    public Dictionary<Type, object?> GetDbSetDictionary()
    {
        var dbSetProperties = GetType().GetProperties()
            .Where(p => p.PropertyType.IsGenericType &&
                        p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

        var dictionary = dbSetProperties.ToDictionary(
            p => p.PropertyType.GetGenericArguments()[0],
            p => p.GetValue(this)
        );

        return dictionary;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:SQLExpressConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Chosen>(entity =>
        {
            entity.HasKey(e => e.RosterID).HasName("PK_Chn_RID");

            entity.ToTable("Chosen");

            entity.Property(e => e.RosterID).ValueGeneratedNever();

            entity.HasOne(d => d.Roster).WithOne(p => p.Chosen)
                .HasForeignKey<Chosen>(d => d.RosterID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EGR_ID__Chn_RID");

            entity.HasOne(d => d.UISideNavigation).WithMany(p => p.Chosens)
                .HasForeignKey(d => d.UISide)
                .HasConstraintName("FK_EUS_Val__Chn_USd");
        });

        modelBuilder.Entity<EnumBtnMsg>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK_EBM_ID");

            entity.ToTable("EnumBtnMsg");

            entity.Property(e => e.ID).ValueGeneratedNever();
            entity.Property(e => e.Value).HasMaxLength(64);
        });

        modelBuilder.Entity<EnumGameRoster>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK_EGR_ID");

            entity.ToTable("EnumGameRoster");

            entity.HasIndex(e => new { e.Origin, e.IDX }, "UQ_EGR_Ent").IsUnique();

            entity.Property(e => e.ID).ValueGeneratedNever();
            entity.Property(e => e.Identity).HasMaxLength(64);
            entity.Property(e => e.Origin)
                .HasMaxLength(64)
                .IsUnicode(false);

            // added manually
            entity.ToTable(b => b.HasCheckConstraint("CK_EGR_Origin", "Origin IN ('None', 'Human', 'AI')"));
        });

        modelBuilder.Entity<EnumGameState>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK_EGS_ID");

            entity.Property(e => e.ID).ValueGeneratedNever();
            entity.Property(e => e.Value).HasMaxLength(64);
        });

        modelBuilder.Entity<EnumUISide>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK_EUS_ID");

            entity.Property(e => e.ID).ValueGeneratedNever();
            entity.Property(e => e.Value).HasMaxLength(64);

            // added manually
            entity.ToTable(b => b.HasCheckConstraint("CK_EUS_Value", "[Value] IN ('None', 'Left', 'Right')"));
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK_Gms_ID");

            entity.Property(e => e.ID)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(64);

            entity.HasOne(d => d.StateNavigation).WithMany(p => p.Games)
                .HasForeignKey(d => d.State)
                .HasConstraintName("FK_Gms_ID__EGS_Val");
        });

        modelBuilder.Entity<GameBoard>(entity =>
        {
            entity.HasKey(e => e.Row).HasName("PK_GBr_ID");

            entity.ToTable("GameBoard");

            entity.HasOne(d => d.Col1Navigation).WithMany(p => p.GameBoardCol1Navigations)
                .HasForeignKey(d => d.Col1)
                .HasConstraintName("FK_GBr_Col1__EGR_ID");

            entity.HasOne(d => d.Col2Navigation).WithMany(p => p.GameBoardCol2Navigations)
                .HasForeignKey(d => d.Col2)
                .HasConstraintName("FK_GBr_Col2__EGR_ID");

            entity.HasOne(d => d.Col3Navigation).WithMany(p => p.GameBoardCol3Navigations)
                .HasForeignKey(d => d.Col3)
                .HasConstraintName("FK_GBr_Col3__EGR_ID");
        });

        modelBuilder.Entity<MenuString>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK_MnS_ID");

            entity.Property(e => e.Value).HasMaxLength(2048);

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentID)
                .HasConstraintName("FK_MnS_ID__MnS_ParentID");
        });

        modelBuilder.Entity<ResxString>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK_RxS_ID");

            entity.Property(e => e.Name).HasMaxLength(64);
            entity.Property(e => e.Value).HasMaxLength(2048);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
