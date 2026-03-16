using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Auth.Queries;

// ── DTO ───────────────────────────────────────────────────────────────────────

public sealed record MeDto(
    Guid UserId,
    string Email,
    string DisplayName,
    Guid OrganisationId,
    bool IsActive,
    DateTimeOffset? LastLoginAt,
    IReadOnlyList<string> Roles);

// ── Query ─────────────────────────────────────────────────────────────────────

public sealed record GetMeQuery(Guid UserId) : IRequest<Result<MeDto>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class GetMeQueryHandler(IUserRepository users)
    : IRequestHandler<GetMeQuery, Result<MeDto>>
{
    public async Task<Result<MeDto>> Handle(GetMeQuery request, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(request.UserId, ct);

        if (user is null || !user.IsActive)
            return Result.Failure<MeDto>(Error.NotFound(nameof(Domain.Aggregates.User.User), request.UserId));

        var roles = user.Roles
            .Where(r => r.IsActive)
            .Select(r => r.RoleName)
            .ToList();

        return Result.Success(new MeDto(
            UserId: user.Id,
            Email: user.Email,
            DisplayName: user.DisplayName,
            OrganisationId: user.OrganisationId,
            IsActive: user.IsActive,
            LastLoginAt: user.LastLoginAt,
            Roles: roles));
    }
}
