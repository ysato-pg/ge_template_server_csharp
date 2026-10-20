using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GeTemplate.Server.Data;

/// <summary>
/// ゲームの DB（SQLite）。
/// </summary>
public sealed class GameDbContext(DbContextOptions<GameDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // SQLite は日時の種類（UTC かどうか）を保存しないため、読み込み時に UTC として扱う。
        var utc = new ValueConverter<DateTime, DateTime>(
            value => value,
            value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

        modelBuilder.Entity<User>(user =>
        {
            user.HasKey(u => u.Id);
            user.Property(u => u.CreatedAt).HasConversion(utc);
            user.Property(u => u.LastLoginAt).HasConversion(utc);
            user.HasIndex(u => u.SessionToken).IsUnique();
        });
    }
}
