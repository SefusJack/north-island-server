using Microsoft.EntityFrameworkCore;

namespace NorthIsland.Server.Data.Sql;

/// <summary>The EF Core database context of the server: accounts, characters and documents.</summary>
/// <param name="options">The context options, naming the database.</param>
public sealed class ServerDbContext(
    DbContextOptions<ServerDbContext> options) : DbContext(options)
{
    /// <summary>The account rows.</summary>
    public DbSet<AccountRow> Accounts => Set<AccountRow>();

    /// <summary>The character rows.</summary>
    public DbSet<CharacterRow> Characters => Set<CharacterRow>();

    /// <summary>The document rows.</summary>
    public DbSet<DocumentRow> Documents => Set<DocumentRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountRow>(e =>
        {
            e.ToTable("accounts");
            e.HasKey(a => a.LoginId);
            e.Property(a => a.LoginId).HasColumnName("login_id").ValueGeneratedNever();
            e.Property(a => a.Name).HasColumnName("name");
            e.Property(a => a.NameKey).HasColumnName("name_key");
            e.HasIndex(a => a.NameKey).IsUnique();
            e.Property(a => a.CreatedUtc).HasColumnName("created_utc");
            e.Property(a => a.LastLoginUtc).HasColumnName("last_login_utc");
            e.Property(a => a.OnlineSeconds).HasColumnName("online_seconds");
            e.Property(a => a.MallPoints).HasColumnName("mall_points");
            e.Property(a => a.SavedUtc).HasColumnName("saved_utc");
            e.Property(a => a.Data).HasColumnName("data").HasColumnType("jsonb");
            e.HasMany(a => a.Characters).WithOne().HasForeignKey(c => c.LoginId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<CharacterRow>(e =>
        {
            e.ToTable("characters");
            e.HasKey(c => new { c.LoginId, c.Slot });
            e.Property(c => c.LoginId).HasColumnName("login_id");
            e.Property(c => c.Slot).HasColumnName("slot");
            e.Property(c => c.Name).HasColumnName("name");
            e.Property(c => c.NameKey).HasColumnName("name_key");
            e.HasIndex(c => c.NameKey);
            e.Property(c => c.Level).HasColumnName("level");
            e.Property(c => c.MapId).HasColumnName("map_id");
        });
        modelBuilder.Entity<DocumentRow>(e =>
        {
            e.ToTable("documents");
            e.HasKey(d => d.Name);
            e.Property(d => d.Name).HasColumnName("name");
            e.Property(d => d.Data).HasColumnName("data").HasColumnType("jsonb");
            e.Property(d => d.SavedUtc).HasColumnName("saved_utc");
        });
    }
}
