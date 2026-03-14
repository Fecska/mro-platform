using FluentValidation;
using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Auth.Dtos;

namespace Mro.Application.Features.Auth.Commands;

// ── Command ──────────────────────────────────────────────────────────────────

public sealed record RefreshTokenCommand : IRequest<Result<AuthTokenDto>>
{
    public required string RefreshToken { get; init; }
    public string? DeviceInfo { get; init; }
}

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class RefreshTokenCommandHandler(
    IUserRepository users,
    ITokenService tokenService)
    : IRequestHandler<RefreshTokenCommand, Result<AuthTokenDto>>
{
    public async Task<Result<AuthTokenDto>> Handle(
        RefreshTokenCommand request,
        CancellationToken ct)
    {
        // Hash the incoming raw token to look up the stored record
        var incomingHash = ComputeHash(request.RefreshToken);

        var user = await users.GetByRefreshTokenHashAsync(incomingHash, ct);
        if (user is null || !user.IsActive)
            return Result.Failure<AuthTokenDto>(Error.Forbidden("Invalid or expired refresh token."));

        var stored = user.RefreshTokens.FirstOrDefault(t => t.TokenHash == incomingHash);
        if (stored is null || !stored.IsActive)
            return Result.Failure<AuthTokenDto>(Error.Forbidden("Invalid or expired refresh token."));

        // Rotate — revoke old token, issue new pair
        user.RevokeRefreshToken(incomingHash, "Token rotated");

        var (rawRefresh, newHash) = tokenService.GenerateRefreshToken();
        var refreshExpiry = DateTimeOffset.UtcNow.AddDays(tokenService.RefreshTokenExpiryDays);

        user.AddRefreshToken(newHash, refreshExpiry, request.DeviceInfo ?? stored.DeviceInfo);
        await users.UpdateAsync(user, ct);

        var accessToken = tokenService.GenerateAccessToken(user);
        var roles = user.Roles
            .Where(r => r.IsActive)
            .Select(r => r.RoleName)
            .ToList();

        return Result.Success(new AuthTokenDto(
            AccessToken: accessToken,
            RefreshToken: rawRefresh,
            AccessTokenExpiresAt: DateTimeOffset.UtcNow.AddMinutes(15),
            RefreshTokenExpiresAt: refreshExpiry,
            UserId: user.Id,
            DisplayName: user.DisplayName,
            Roles: roles));
    }

    private static string ComputeHash(string raw)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
