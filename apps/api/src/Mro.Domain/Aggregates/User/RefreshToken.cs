using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.User;

/// <summary>
/// An opaque refresh token issued to a client session.
/// Only the SHA-256 hash of the raw token is stored — the raw token is
/// transmitted once at issuance and never persisted.
///
/// Refresh tokens are owned by the User aggregate.
/// Do not query the refresh_tokens table directly; use IUserRepository.
/// </summary>
public sealed class RefreshToken : BaseEntity
{
    /// <summary>FK to the owning user.</summary>
    public Guid UserId { get; private set; }

    /// <summary>SHA-256 hex digest of the raw token sent to the client.</summary>
    public string TokenHash { get; private set; } = string.Empty;

    /// <summary>When this token expires.</summary>
    public DateTimeOffset ExpiresAt { get; private set; }

    /// <summary>When this token was issued.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Browser / device description from the User-Agent header.
    /// Stored for display in "active sessions" UI.
    /// </summary>
    public string? DeviceInfo { get; private set; }

    public bool IsRevoked { get; private set; }
    public string? RevokedReason { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    // EF Core
    private RefreshToken() { }

    internal static RefreshToken Create(
        Guid userId,
        string tokenHash,
        DateTimeOffset expiresAt,
        string? deviceInfo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        return new RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedAt = DateTimeOffset.UtcNow,
            DeviceInfo = deviceInfo,
        };
    }

    internal void Revoke(string reason)
    {
        if (IsRevoked) return;
        IsRevoked = true;
        RevokedReason = reason;
        RevokedAt = DateTimeOffset.UtcNow;
    }
}
