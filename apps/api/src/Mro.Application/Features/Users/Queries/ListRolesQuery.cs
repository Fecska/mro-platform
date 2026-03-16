using MediatR;
using Mro.Application.Common;
using Mro.Application.Features.Users.Dtos;
using Mro.Domain.Common.Permissions;

namespace Mro.Application.Features.Users.Queries;

public sealed record ListRolesQuery : IRequest<Result<IReadOnlyList<RoleDto>>>;

public sealed class ListRolesQueryHandler
    : IRequestHandler<ListRolesQuery, Result<IReadOnlyList<RoleDto>>>
{
    private static readonly IReadOnlyList<RoleDto> _roles =
    [
        new(Roles.CertifyingStaff, "Holds a Part-66 licence; can issue CRS"),
        new(Roles.Engineer,        "Performs maintenance tasks; cannot issue CRS"),
        new(Roles.Inspector,       "Quality assurance / independent inspector"),
        new(Roles.Planner,         "Creates work packages, assigns tasks"),
        new(Roles.StoreKeeper,     "Manages parts, consumables, and tools"),
        new(Roles.OrgAdmin,        "Organisation-level administrator"),
        new(Roles.SystemAdmin,     "Platform-level system administrator"),
        new(Roles.PersonnelAdmin,  "HR / Personnel administrator — full employee lifecycle management"),
    ];

    public Task<Result<IReadOnlyList<RoleDto>>> Handle(
        ListRolesQuery request,
        CancellationToken ct) =>
        Task.FromResult(Result.Success(_roles));
}
