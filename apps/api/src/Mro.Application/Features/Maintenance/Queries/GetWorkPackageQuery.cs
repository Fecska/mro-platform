using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Maintenance.Dtos;

namespace Mro.Application.Features.Maintenance.Queries;

public sealed record GetWorkPackageQuery(Guid Id) : IRequest<Result<WorkPackageDetailDto>>;

public sealed class GetWorkPackageQueryHandler(
    IWorkPackageRepository packages,
    ICurrentUserService currentUser)
    : IRequestHandler<GetWorkPackageQuery, Result<WorkPackageDetailDto>>
{
    public async Task<Result<WorkPackageDetailDto>> Handle(GetWorkPackageQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<WorkPackageDetailDto>(Error.Forbidden("Organisation context is required."));

        var wp = await packages.GetByIdAsync(request.Id, currentUser.OrganisationId.Value, ct);
        if (wp is null)
            return Result.Failure<WorkPackageDetailDto>(Error.NotFound("WorkPackage", request.Id));

        var items = wp.Items.Select(i => new PackageItemDto(
            i.Id, i.DueItemId, i.Description, i.TaskReference, i.Status,
            i.EstimatedManHours, i.ActualManHours, i.RelatedWorkOrderId,
            i.DeferralReason, i.NotApplicableReason))
            .ToList();

        return Result.Success(new WorkPackageDetailDto(
            wp.Id, wp.PackageNumber, wp.AircraftId, wp.Description, wp.Status,
            wp.PlannedStartDate, wp.PlannedEndDate,
            wp.ActualStartDate, wp.ActualEndDate,
            wp.StationId, wp.RelatedWorkOrderId,
            items, wp.CreatedAt));
    }
}
