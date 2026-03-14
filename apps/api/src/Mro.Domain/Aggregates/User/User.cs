using Mro.Domain.Aggregates.User.Events;
using Mro.Domain.Common.Audit;
using Mro.Domain.Common.Permissions;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.User;

/// <summary>
/// Aggregate root representing a platform user.
///
/// Security invariants enforced here:
///   - Password hash is the only credential stored; raw passwords are never kept
///   - Failed login counter triggers an account lock (configurable threshold)
///   - A locked account cannot authenticate, even with a valid password
///   - Deactivated users may not log in and are excluded from work assignments
///   - Refresh tokens are stored as SHA-256 hashes only
/// </summary>
public sealed class User : AuditableEntity
{
    /// <summary>Unique email address within the organisation.</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>Full display name (e.g. "John Smith").</summary>
    public string DisplayName { get; private set; } = string.Empty;

    /// <summary>BCrypt hash of the user's password. Never expose in responses.</summary>
    public string PasswordHash { get; private set; } = string.Empty;

    public bool IsActive { get; private set; } = true;

    /// <summary>Consecutive failed login attempts since last successful login.</summary>
    public int FailedLoginAttempts { get; private set; }

    /// <summary>Account is locked until this time. Null = not locked.</summary>
    public DateTimeOffset? LockedUntil { get; private set; }

    /// <summary>Last successful authentication timestamp.</summary>
    public DateTimeOffset? LastLoginAt { get; private set; }

    private readonly List<UserRole> _roles = [];
    private readonly List<RefreshToken> _refreshTokens = [];

    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    /// <summary>True if the account lockout period has not yet expired.</summary>
    public bool IsLocked =>
        LockedUntil.HasValue && LockedUntil.Value > DateTimeOffset.UtcNow;

    // EF Core
    private User() { }

    /// <summary>Creates a new platform user.</summary>
    public static User Create(
        string email,
        string displayName,
        string passwordHash,
        Guid organisationId,
        Guid createdBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        var user = new User
        {
            Email = email.Trim().ToLowerInvariant(),
            DisplayName = displayName.Trim(),
            PasswordHash = passwordHash,
            OrganisationId = organisationId,
            CreatedBy = createdBy,
            UpdatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        user.RaiseDomainEvent(new UserCreatedEvent
        {
            ActorId = createdBy,
            OrganisationId = organisationId,
            EntityType = nameof(User),
            EntityId = user.Id,
            EventType = ComplianceEventType.RecordCreated,
            Email = user.Email,
            DisplayName = user.DisplayName,
            Description = $"User '{user.DisplayName}' ({user.Email}) created.",
        });

        return user;
    }

    // ── Roles ────────────────────────────────────────────────────────────────

    /// <summary>Assigns a role with an operational scope. Idempotent per role name.</summary>
    public UserRole AssignRole(string roleName, OperationalScope scope, Guid actorId)
    {
        // Revoke existing assignment for this role before re-assigning
        var existing = _roles.FirstOrDefault(r => r.RoleName == roleName && r.IsActive);
        existing?.Revoke(actorId);

        var userRole = UserRole.Create(Id, roleName, scope, OrganisationId, actorId);
        _roles.Add(userRole);

        RaiseDomainEvent(new UserRoleAssignedEvent
        {
            ActorId = actorId,
            OrganisationId = OrganisationId,
            EntityType = nameof(User),
            EntityId = Id,
            EventType = ComplianceEventType.AuthorisationGranted,
            RoleName = roleName,
            TargetUserId = Id,
            Description = $"Role '{roleName}' assigned to user '{DisplayName}'.",
        });

        return userRole;
    }

    /// <summary>Revokes a role assignment.</summary>
    public void RemoveRole(string roleName, Guid actorId)
    {
        var role = _roles.FirstOrDefault(r => r.RoleName == roleName && r.IsActive);
        if (role is null) return;

        role.Revoke(actorId);

        RaiseDomainEvent(new UserRoleAssignedEvent
        {
            ActorId = actorId,
            OrganisationId = OrganisationId,
            EntityType = nameof(User),
            EntityId = Id,
            EventType = ComplianceEventType.AuthorisationRevoked,
            RoleName = roleName,
            TargetUserId = Id,
            Description = $"Role '{roleName}' revoked from user '{DisplayName}'.",
        });
    }

    // ── Login tracking ───────────────────────────────────────────────────────

    /// <summary>
    /// Records a successful login. Resets the failed-attempt counter and lock.
    /// </summary>
    public void RecordSuccessfulLogin()
    {
        FailedLoginAttempts = 0;
        LockedUntil = null;
        LastLoginAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Increments the failed-attempt counter and locks the account if
    /// <paramref name="maxAttempts"/> is reached.
    /// </summary>
    public void RecordFailedLogin(int maxAttempts, TimeSpan lockDuration)
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= maxAttempts)
        {
            LockedUntil = DateTimeOffset.UtcNow.Add(lockDuration);
        }
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    // ── Refresh tokens ───────────────────────────────────────────────────────

    /// <summary>Adds a new refresh token for a client session.</summary>
    public RefreshToken AddRefreshToken(
        string tokenHash,
        DateTimeOffset expiresAt,
        string? deviceInfo)
    {
        var token = RefreshToken.Create(Id, tokenHash, expiresAt, deviceInfo);
        _refreshTokens.Add(token);
        return token;
    }

    /// <summary>Revokes all active refresh tokens (e.g. on logout-all-devices).</summary>
    public void RevokeAllRefreshTokens(string reason)
    {
        foreach (var token in _refreshTokens.Where(t => t.IsActive))
        {
            token.Revoke(reason);
        }
    }

    /// <summary>Revokes a specific refresh token by its hash.</summary>
    public void RevokeRefreshToken(string tokenHash, string reason)
    {
        var token = _refreshTokens.FirstOrDefault(t => t.TokenHash == tokenHash);
        token?.Revoke(reason);
    }

    // ── Account lifecycle ────────────────────────────────────────────────────

    /// <summary>Deactivates the account. Revokes all refresh tokens.</summary>
    public void Deactivate(Guid actorId, string reason)
    {
        IsActive = false;
        RevokeAllRefreshTokens("Account deactivated");
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new UserDeactivatedEvent
        {
            ActorId = actorId,
            OrganisationId = OrganisationId,
            EntityType = nameof(User),
            EntityId = Id,
            EventType = ComplianceEventType.AuthorisationRevoked,
            Reason = reason,
            Description = $"User '{DisplayName}' ({Email}) deactivated. Reason: {reason}",
        });
    }

    /// <summary>Updates the password hash (after a verified password change flow).</summary>
    public void UpdatePasswordHash(string newPasswordHash, Guid actorId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newPasswordHash);
        PasswordHash = newPasswordHash;
        RevokeAllRefreshTokens("Password changed");
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
