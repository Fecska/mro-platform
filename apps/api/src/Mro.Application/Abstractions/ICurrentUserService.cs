using Mro.Domain.Common.Permissions;

namespace Mro.Application.Abstractions;

/// <summary>
/// Provides the identity of the currently authenticated user.
/// Consumed by AuditInterceptor and business logic that needs actor context.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>User ID from JWT claim. Null if request is unauthenticated.</summary>
    Guid? UserId { get; }

    /// <summary>Organisation ID from JWT claim.</summary>
    Guid? OrganisationId { get; }

    /// <summary>Station IDs the user has access to.</summary>
    IReadOnlyList<Guid> StationIds { get; }

    /// <summary>Roles assigned to the current user.</summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>Whether the user holds a specific role.</summary>
    bool IsInRole(string role);

    /// <summary>
    /// Whether the user's current roles grant the specified permission.
    /// Resolved via <see cref="RolePermissions"/> — checks the union of all held roles.
    /// </summary>
    bool HasPermission(Permission permission);

    /// <summary>IP address of the current request, for audit logging.</summary>
    string? IpAddress { get; }

    /// <summary>User-Agent of the current request, for audit logging.</summary>
    string? UserAgent { get; }
}
