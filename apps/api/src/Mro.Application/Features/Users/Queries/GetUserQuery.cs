using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Users.Dtos;

namespace Mro.Application.Features.Users.Queries;

public sealed record GetUserQuery(Guid UserId) : IRequest<Result<UserDetailDto>>;

public sealed class GetUserQueryHandler(
    IUserRepository users,
    ICurrentUserService currentUser)
    : IRequestHandler<GetUserQuery, Result<UserDetailDto>>
{
    public async Task<Result<UserDetailDto>> Handle(GetUserQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<UserDetailDto>(Error.Forbidden("No organisation context."));

        var user = await users.GetByIdAsync(request.UserId, ct);

        if (user is null || user.OrganisationId != currentUser.OrganisationId.Value)
            return Result.Failure<UserDetailDto>(Error.NotFound(nameof(Domain.Aggregates.User.User), request.UserId));

        var roleDtos = user.Roles
            .Where(r => r.IsActive)
            .Select(r => new UserRoleDto(
                RoleName: r.RoleName,
                StationIds: r.Scope.StationIds,
                AircraftTypes: r.Scope.AircraftTypes,
                ReleaseCategories: r.Scope.ReleaseCategories))
            .ToList();

        return Result.Success(new UserDetailDto(
            Id: user.Id,
            Email: user.Email,
            DisplayName: user.DisplayName,
            OrganisationId: user.OrganisationId,
            IsActive: user.IsActive,
            IsLocked: user.IsLocked,
            LastLoginAt: user.LastLoginAt,
            CreatedAt: user.CreatedAt,
            Roles: roleDtos));
    }
}
