using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Users.Dtos;

namespace Mro.Application.Features.Users.Queries;

public sealed record ListUsersQuery(
    string? Role,
    bool? IsActive,
    int Page = 1,
    int PageSize = 50) : IRequest<Result<IReadOnlyList<UserSummaryDto>>>;

public sealed class ListUsersQueryHandler(
    IUserRepository users,
    ICurrentUserService currentUser)
    : IRequestHandler<ListUsersQuery, Result<IReadOnlyList<UserSummaryDto>>>
{
    public async Task<Result<IReadOnlyList<UserSummaryDto>>> Handle(
        ListUsersQuery request,
        CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<IReadOnlyList<UserSummaryDto>>(Error.Forbidden("No organisation context."));

        var list = await users.ListAsync(
            currentUser.OrganisationId.Value,
            request.Role,
            request.IsActive,
            request.Page,
            request.PageSize,
            ct);

        var dtos = list.Select(u => new UserSummaryDto(
            Id: u.Id,
            Email: u.Email,
            DisplayName: u.DisplayName,
            IsActive: u.IsActive,
            LastLoginAt: u.LastLoginAt,
            Roles: u.Roles.Where(r => r.IsActive).Select(r => r.RoleName).ToList()))
            .ToList();

        return Result.Success<IReadOnlyList<UserSummaryDto>>(dtos);
    }
}
