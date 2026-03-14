using Mro.Domain.Aggregates.User;

namespace Mro.Application.Abstractions;

/// <summary>
/// Generates JWT access tokens and opaque refresh tokens.
/// Implemented in Mro.Infrastructure using JwtBearer.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a signed JWT access token containing the user's id,
    /// organisation, roles, and station scope claims.
    /// </summary>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generates a cryptographically random refresh token.
    /// Returns both the raw token (to send to the client) and its
    /// SHA-256 hash (to persist in the database).
    /// </summary>
    (string RawToken, string TokenHash) GenerateRefreshToken();

    /// <summary>How many days a refresh token is valid.</summary>
    int RefreshTokenExpiryDays { get; }
}
