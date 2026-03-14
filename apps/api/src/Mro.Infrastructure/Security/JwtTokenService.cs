using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Mro.Application.Abstractions;
using Mro.Domain.Aggregates.User;

namespace Mro.Infrastructure.Security;

/// <summary>
/// Generates signed JWT access tokens and random opaque refresh tokens.
///
/// JWT claims included:
///   sub              — user ID
///   email            — user email
///   name             — display name
///   organisation_id  — owning org
///   role             — one claim per active role
///   station_id       — one claim per station in scope (from any role)
///
/// Configuration keys (appsettings.json / env overrides):
///   Jwt:SecretKey    — HMAC-SHA256 signing key (min 32 chars)
///   Jwt:Issuer       — token issuer (e.g. "mro-platform")
///   Jwt:Audience     — token audience (e.g. "mro-platform-api")
///   Jwt:ExpiryMinutes      — access token lifetime (default: 15)
///   Jwt:RefreshExpiryDays  — refresh token lifetime (default: 30)
/// </summary>
public sealed class JwtTokenService : ITokenService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryMinutes;

    public int RefreshTokenExpiryDays { get; }

    public JwtTokenService(IConfiguration configuration)
    {
        _secretKey  = configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");
        _issuer     = configuration["Jwt:Issuer"]    ?? "mro-platform";
        _audience   = configuration["Jwt:Audience"]  ?? "mro-platform-api";
        _expiryMinutes      = int.TryParse(configuration["Jwt:ExpiryMinutes"],     out var m) ? m : 15;
        RefreshTokenExpiryDays = int.TryParse(configuration["Jwt:RefreshExpiryDays"], out var d) ? d : 30;
    }

    public string GenerateAccessToken(User user)
    {
        var claims = BuildClaims(user);
        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_expiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (string RawToken, string TokenHash) GenerateRefreshToken()
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        var hash = Convert.ToHexString(hashBytes).ToLowerInvariant();
        return (raw, hash);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static IEnumerable<Claim> BuildClaims(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("name", user.DisplayName),
            new("organisation_id", user.OrganisationId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        // One role claim per active role
        foreach (var role in user.Roles.Where(r => r.IsActive))
        {
            claims.Add(new Claim(ClaimTypes.Role, role.RoleName));

            // Emit station_id claims from scope
            foreach (var stationId in role.Scope.StationIds)
                claims.Add(new Claim("station_id", stationId.ToString()));
        }

        return claims;
    }
}
