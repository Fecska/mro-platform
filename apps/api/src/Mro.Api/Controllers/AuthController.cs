using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mro.Application.Features.Auth.Commands;

namespace Mro.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    // ── POST /api/auth/login ──────────────────────────────────────────────

    public sealed record LoginRequest(
        string Email,
        string Password,
        Guid OrganisationId,
        string? DeviceInfo);

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(new LoginCommand
        {
            Email = request.Email,
            Password = request.Password,
            OrganisationId = request.OrganisationId,
            DeviceInfo = request.DeviceInfo,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
        }, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(result.Error.Message, statusCode: StatusCodes.Status403Forbidden);
    }

    // ── POST /api/auth/refresh ────────────────────────────────────────────

    public sealed record RefreshRequest(string RefreshToken, string? DeviceInfo);

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(new RefreshTokenCommand
        {
            RefreshToken = request.RefreshToken,
            DeviceInfo = request.DeviceInfo,
        }, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(result.Error.Message, statusCode: StatusCodes.Status403Forbidden);
    }

    // ── POST /api/auth/logout ─────────────────────────────────────────────

    public sealed record LogoutRequest(string? RefreshToken);

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out var uid))
            return Unauthorized();

        var result = await sender.Send(new LogoutCommand
        {
            UserId = uid,
            RefreshToken = request.RefreshToken,
        }, ct);

        return result.IsSuccess ? NoContent() : BadRequest(result.Error.Message);
    }
}
