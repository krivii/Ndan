using Microsoft.EntityFrameworkCore;
using NDanApp.Backend.Models.Entities;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace NDanApp.Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Event> Events { get; set; }
    public DbSet<Guest> Guests { get; set; }
    public DbSet<Media> Media { get; set; }
    public DbSet<Like> Likes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Event configuration
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId);
            entity.HasIndex(e => e.InviteTokenHash).IsUnique();
            entity.HasIndex(e => e.IsActive);

            entity.Property(e => e.CreatedUtc)
                .HasDefaultValueSql("NOW()");
        });

        // Guest configuration
        modelBuilder.Entity<Guest>(entity =>
        {
            entity.HasKey(g => g.GuestId);
            entity.HasIndex(g => g.EventId);

            entity.HasOne(g => g.Event)
                .WithMany(e => e.Guests)
                .HasForeignKey(g => g.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(g => g.CreatedUtc)
                .HasDefaultValueSql("NOW()");
        });

        // Media configuration

        modelBuilder.HasPostgresEnum<MediaType>("media_type_enum");

        modelBuilder.Entity<Media>(entity =>
        {
            entity.HasKey(m => m.MediaId);
            entity.HasIndex(m => new { m.EventId, m.CreatedUtc });

            entity.HasOne(m => m.Event)
                .WithMany(e => e.MediaItems)
                .HasForeignKey(m => m.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Guest)
                .WithMany(g => g.MediaItems)
                .HasForeignKey(m => m.GuestId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.Property(m => m.MediaType)
                .HasConversion<string>()
                .HasColumnType("media_type_enum");

            entity.Property(m => m.ProcessingStatus)
                .HasDefaultValue(ProcessingStatus.Ready);

            entity.Property(m => m.CreatedUtc)
                .HasDefaultValueSql("NOW()");
        });

        // Like configuration
        modelBuilder.Entity<Like>(entity =>
        {
            entity.HasKey(l => l.LikeId);
            entity.HasIndex(l => new { l.MediaId, l.GuestId }).IsUnique();
            entity.HasIndex(l => l.MediaId);

            entity.HasOne(l => l.Media)
                .WithMany(m => m.Likes)
                .HasForeignKey(l => l.MediaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.Guest)
                .WithMany(g => g.Likes)
                .HasForeignKey(l => l.GuestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(l => l.CreatedUtc)
                .HasDefaultValueSql("NOW()");
        });
    }
}