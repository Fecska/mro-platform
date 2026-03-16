using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mro.Domain.Aggregates.User;

namespace Mro.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(254);

        builder.HasIndex(u => new { u.Email, u.OrganisationId })
            .IsUnique()
            .HasDatabaseName("ix_users_email_org");

        builder.Property(u => u.DisplayName)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(72);      // BCrypt output is always 60 chars; 72 gives headroom

        builder.Property(u => u.IsActive).IsRequired();
        builder.Property(u => u.FailedLoginAttempts).IsRequired().HasDefaultValue(0);

        // ── Owned collections ───────────────────────────────────────────────

        // ── Ignored computed properties ─────────────────────────────────────
        builder.Ignore(u => u.Roles);
        builder.Ignore(u => u.RefreshTokens);
        builder.Ignore(u => u.IsLocked);

        builder.HasMany<UserRole>("_roles")
            .WithOne()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<RefreshToken>("_refreshTokens")
            .WithOne()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.RoleName).IsRequired().HasMaxLength(50);
        builder.Property(r => r.IsActive).IsRequired();

        // OperationalScope stored as JSONB
        builder.OwnsOne(r => r.Scope, scope =>
        {
            scope.ToJson();
        });

        builder.HasIndex(r => new { r.UserId, r.RoleName, r.IsActive })
            .HasDatabaseName("ix_user_roles_user_role_active");
    }
}

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TokenHash).IsRequired().HasMaxLength(64);
        builder.HasIndex(t => t.TokenHash)
            .IsUnique()
            .HasDatabaseName("ix_refresh_tokens_hash");

        builder.Property(t => t.DeviceInfo).HasMaxLength(512);
        builder.Property(t => t.RevokedReason).HasMaxLength(256);

        // Ignore computed properties
        builder.Ignore(t => t.IsExpired);
        builder.Ignore(t => t.IsActive);
    }
}
