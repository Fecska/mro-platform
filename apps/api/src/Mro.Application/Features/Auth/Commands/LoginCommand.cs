using FluentValidation;
using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Auth.Dtos;
using Mro.Domain.Common.Audit;

namespace Mro.Application.Features.Auth.Commands;

// ── Command ──────────────────────────────────────────────────────────────────

public sealed record LoginCommand : IRequest<Result<AuthTokenDto>>
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required Guid OrganisationId { get; init; }
    public string? DeviceInfo { get; init; }
    public string? IpAddress { get; init; }
}

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(128);
        RuleFor(x => x.OrganisationId).NotEmpty();
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class LoginCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IAuditService audit)
    : IRequestHandler<LoginCommand, Result<AuthTokenDto>>
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(15);

    public async Task<Result<AuthTokenDto>> Handle(
        LoginCommand request,
        CancellationToken ct)
    {
        var user = await users.GetByEmailAsync(request.Email, request.OrganisationId, ct);

        if (user is null || !user.IsActive)
        {
            // Do not reveal whether the email exists
            await audit.RecordSecurityEventAsync(
                ComplianceEventType.LoginFailed,
                targetUserId: null,
                context: new { request.Email, request.IpAddress });

            return Result.Failure<AuthTokenDto>(
                Error.Forbidden("Invalid credentials."));
        }

        if (user.IsLocked)
        {
            await audit.RecordSecurityEventAsync(
                ComplianceEventType.LoginBlocked,
                targetUserId: user.Id.ToString(),
                context: new { request.IpAddress, user.LockedUntil });

            return Result.Failure<AuthTokenDto>(
                Error.Forbidden($"Account is locked. Try again after {user.LockedUntil:u}."));
        }

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.RecordFailedLogin(MaxFailedAttempts, LockDuration);
            await users.UpdateAsync(user, ct);

            await audit.RecordSecurityEventAsync(
                ComplianceEventType.LoginFailed,
                targetUserId: user.Id.ToString(),
                context: new { request.IpAddress, user.FailedLoginAttempts });

            return Result.Failure<AuthTokenDto>(
                Error.Forbidden("Invalid credentials."));
        }

        // Success — generate tokens
        var (rawRefresh, tokenHash) = tokenService.GenerateRefreshToken();
        var refreshExpiry = DateTimeOffset.UtcNow.AddDays(tokenService.RefreshTokenExpiryDays);

        user.RecordSuccessfulLogin();
        user.AddRefreshToken(tokenHash, refreshExpiry, request.DeviceInfo);
        await users.UpdateAsync(user, ct);

        await audit.RecordSecurityEventAsync(
            ComplianceEventType.LoginSuccess,
            targetUserId: user.Id.ToString(),
            context: new { request.IpAddress, request.DeviceInfo });

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
}
