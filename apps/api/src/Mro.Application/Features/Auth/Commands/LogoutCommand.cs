using FluentValidation;
using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Common.Audit;

namespace Mro.Application.Features.Auth.Commands;

// ── Command ──────────────────────────────────────────────────────────────────

public sealed record LogoutCommand : IRequest<Result>
{
    public required Guid UserId { get; init; }

    /// <summary>
    /// The refresh token to revoke. If null, all tokens are revoked
    /// (logout from all devices).
    /// </summary>
    public string? RefreshToken { get; init; }
}

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class LogoutCommandHandler(
    IUserRepository users,
    IAuditService audit)
    : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(request.UserId, ct);
        if (user is null)
            return Result.Failure(Error.NotFound(nameof(Domain.Aggregates.User.User), request.UserId));

        if (request.RefreshToken is not null)
        {
            var hash = ComputeHash(request.RefreshToken);
            user.RevokeRefreshToken(hash, "User logout");
        }
        else
        {
            user.RevokeAllRefreshTokens("User logout (all devices)");
        }

        await users.UpdateAsync(user, ct);

        await audit.RecordSecurityEventAsync(
            ComplianceEventType.Logout,
            targetUserId: user.Id.ToString(),
            context: new { LogoutAllDevices = request.RefreshToken is null });

        return Result.Success();
    }

    private static string ComputeHash(string raw)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
