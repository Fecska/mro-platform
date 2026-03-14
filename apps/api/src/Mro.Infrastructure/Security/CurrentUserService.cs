using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Mro.Application.Abstractions;

namespace Mro.Infrastructure.Security;

/// <summary>
/// Resolves the current authenticated user from the HTTP context JWT claims.
/// Registered as Scoped — one instance per HTTP request.
/// </summary>
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserService
{
    private ClaimsPrincipal? Principal =>
        httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var value = Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? Principal?.FindFirstValue("sub");
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public Guid? OrganisationId
    {
        get
        {
            var value = Principal?.FindFirstValue("organisation_id");
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public IReadOnlyList<Guid> StationIds
    {
        get
        {
            var claims = Principal?.FindAll("station_id") ?? [];
            return claims
                .Select(c => Guid.TryParse(c.Value, out var id) ? id : (Guid?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();
        }
    }

    public IReadOnlyList<string> Roles =>
        Principal?.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList()
        ?? [];

    public bool IsInRole(string role) =>
        Principal?.IsInRole(role) ?? false;

    public string? IpAddress =>
        httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent =>
        httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
}
