using Mro.Domain.Common.Permissions;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.User;

/// <summary>
/// Associates a User with a named role and an operational scope.
/// A user may hold multiple roles, each with its own scope.
///
/// Example: a certifying staff member may hold both the B1 engineer role
/// scoped to station LHBP and the planner role scoped to all stations.
///
/// Owned by the User aggregate — do not create directly.
/// </summary>
public sealed class UserRole : AuditableEntity
{
    /// <summary>FK to the owning user.</summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// One of the constants from <see cref="Roles"/>.
    /// Stored as a string so DB records survive code renames.
    /// </summary>
    public string RoleName { get; private set; } = string.Empty;

    /// <summary>
    /// Operational boundaries for this role assignment.
    /// Persisted as a JSONB column.
    /// </summary>
    public OperationalScope Scope { get; private set; } = OperationalScope.Unrestricted;

    public bool IsActive { get; private set; } = true;

    // EF Core
    private UserRole() { }

    internal static UserRole Create(
        Guid userId,
        string roleName,
        OperationalScope scope,
        Guid organisationId,
        Guid actorId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName);

        return new UserRole
        {
            UserId = userId,
            RoleName = roleName,
            Scope = scope,
            IsActive = true,
            OrganisationId = organisationId,
            CreatedBy = actorId,
            UpdatedBy = actorId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    internal void Revoke(Guid actorId)
    {
        IsActive = false;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
