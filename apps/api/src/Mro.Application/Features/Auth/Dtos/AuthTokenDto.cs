namespace Mro.Application.Features.Auth.Dtos;

/// <summary>
/// Token pair returned by login and refresh-token operations.
/// The refresh token is a raw opaque string; only its hash is stored server-side.
/// </summary>
public sealed record AuthTokenDto(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt,
    Guid UserId,
    string DisplayName,
    IReadOnlyList<string> Roles);
