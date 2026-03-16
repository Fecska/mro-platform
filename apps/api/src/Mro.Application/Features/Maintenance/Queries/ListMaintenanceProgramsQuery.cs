using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Maintenance.Dtos;

namespace Mro.Application.Features.Maintenance.Queries;

public sealed record ListMaintenanceProgramsQuery(string? AircraftTypeCode)
    : IRequest<Result<IReadOnlyList<MaintenanceProgramDto>>>;

public sealed class ListMaintenanceProgramsQueryHandler(
    IMaintenanceProgramRepository programs,
    ICurrentUserService currentUser)
    : IRequestHandler<ListMaintenanceProgramsQuery, Result<IReadOnlyList<MaintenanceProgramDto>>>
{
    public async Task<Result<IReadOnlyList<MaintenanceProgramDto>>> Handle(
        ListMaintenanceProgramsQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<IReadOnlyList<MaintenanceProgramDto>>(
                Error.Forbidden("Organisation context is required."));

        var list = await programs.ListAsync(currentUser.OrganisationId.Value, request.AircraftTypeCode, ct);

        var dtos = list.Select(p => new MaintenanceProgramDto(
            p.Id, p.ProgramNumber, p.AircraftTypeCode, p.Title,
            p.RevisionNumber, p.RevisionDate, p.ApprovalReference, p.IsActive))
            .ToList();

        return Result.Success<IReadOnlyList<MaintenanceProgramDto>>(dtos);
    }
}
