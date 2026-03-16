namespace Mro.Application.Features.Users.Dtos;

public sealed record UserRoleDto(
    string RoleName,
    IReadOnlyList<Guid> StationIds,
    IReadOnlyList<string> AircraftTypes,
    IReadOnlyList<string> ReleaseCategories);

public sealed record UserSummaryDto(
    Guid Id,
    string Email,
    string DisplayName,
    bool IsActive,
    DateTimeOffset? LastLoginAt,
    IReadOnlyList<string> Roles);

public sealed record UserDetailDto(
    Guid Id,
    string Email,
    string DisplayName,
    Guid OrganisationId,
    bool IsActive,
    bool IsLocked,
    DateTimeOffset? LastLoginAt,
    DateTimeOffset CreatedAt,
    IReadOnlyList<UserRoleDto> Roles);

public sealed record RoleDto(string Name, string Description);

public sealed record PermissionDto(string Code, string Description);
