using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StartedApi.Domain.Audit;
using StartedApi.Domain.Auth;
using StartedApi.Domain.Roles;
using StartedApi.Domain.Users;

namespace StartedApi.Infrastructure.Persistence;

public sealed class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(user => user.LastName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(user => user.CreatedAtUtc)
                .HasColumnType("datetime2");

            entity.Property(user => user.UpdatedAtUtc)
                .HasColumnType("datetime2");

            entity.Property(user => user.LastLoginAtUtc)
                .HasColumnType("datetime2");

            entity.HasMany(user => user.RefreshTokens)
                .WithOne(token => token.User)
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ApplicationRole>(entity =>
        {
            entity.Property(role => role.Description)
                .HasMaxLength(250);

            entity.Property(role => role.CreatedAtUtc)
                .HasColumnType("datetime2");
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(token => token.Id);

            entity.Property(token => token.TokenHash)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(token => token.CreatedByIp)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(token => token.RevokedByIp)
                .HasMaxLength(64);

            entity.Property(token => token.ReasonRevoked)
                .HasMaxLength(250);

            entity.Property(token => token.ReplacedByTokenHash)
                .HasMaxLength(128);

            entity.Property(token => token.CreatedAtUtc)
                .HasColumnType("datetime2");

            entity.Property(token => token.ExpiresAtUtc)
                .HasColumnType("datetime2");

            entity.Property(token => token.RevokedAtUtc)
                .HasColumnType("datetime2");

            entity.HasIndex(token => token.TokenHash)
                .IsUnique();

            entity.HasIndex(token => new { token.UserId, token.ExpiresAtUtc });
        });

        builder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(log => log.Id);

            entity.Property(log => log.Action)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(log => log.EntityName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(log => log.EntityId)
                .HasMaxLength(100);

            entity.Property(log => log.IpAddress)
                .HasMaxLength(64);

            entity.Property(log => log.UserAgent)
                .HasMaxLength(500);

            entity.Property(log => log.Details)
                .HasMaxLength(2000);

            entity.Property(log => log.OccurredAtUtc)
                .HasColumnType("datetime2");

            entity.HasOne(log => log.User)
                .WithMany()
                .HasForeignKey(log => log.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(log => log.UserId);
            entity.HasIndex(log => log.OccurredAtUtc);
            entity.HasIndex(log => log.Action);
        });
    }
}
