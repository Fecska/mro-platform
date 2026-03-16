using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Maintenance.Dtos;
using Mro.Domain.Aggregates.Maintenance.Enums;

namespace Mro.Application.Features.Maintenance.Queries;

public sealed record ListWorkPackagesQuery(
    Guid? AircraftId,
    WorkPackageStatus? Status,
    int Page,
    int PageSize) : IRequest<Result<IReadOnlyList<WorkPackageSummaryDto>>>;

public sealed class ListWorkPackagesQueryHandler(
    IWorkPackageRepository packages,
    ICurrentUserService currentUser)
    : IRequestHandler<ListWorkPackagesQuery, Result<IReadOnlyList<WorkPackageSummaryDto>>>
{
    public async Task<Result<IReadOnlyList<WorkPackageSummaryDto>>> Handle(
        ListWorkPackagesQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<IReadOnlyList<WorkPackageSummaryDto>>(
                Error.Forbidden("Organisation context is required."));

        var list = await packages.ListAsync(
            currentUser.OrganisationId.Value, request.AircraftId,
            request.Status, request.Page, request.PageSize, ct);

        var dtos = list.Select(wp => new WorkPackageSummaryDto(
            wp.Id, wp.PackageNumber, wp.AircraftId, wp.Description, wp.Status,
            wp.PlannedStartDate, wp.PlannedEndDate,
            wp.Items.Count,
            wp.Items.Count(i => i.Status == PackageItemStatus.Pending)))
            .ToList();

        return Result.Success<IReadOnlyList<WorkPackageSummaryDto>>(dtos);
    }
}
