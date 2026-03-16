using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Maintenance;

namespace Mro.Application.Features.Maintenance.Commands;

public sealed class CreateWorkPackageCommand : IRequest<Result<Guid>>
{
    public required Guid AircraftId { get; init; }
    public required string Description { get; init; }
    public required DateOnly PlannedStartDate { get; init; }
    public DateOnly? PlannedEndDate { get; init; }
    public Guid? StationId { get; init; }
    public Guid? RelatedWorkOrderId { get; init; }
}

public sealed class CreateWorkPackageCommandHandler(
    IWorkPackageRepository packages,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateWorkPackageCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateWorkPackageCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Guid>(Error.Forbidden("Organisation context is required."));

        var orgId   = currentUser.OrganisationId.Value;
        var actorId = currentUser.UserId!.Value;

        var count  = await packages.CountAsync(orgId, ct);
        var number = $"WP-{DateTimeOffset.UtcNow.Year}-{(count + 1):D5}";

        var wp = WorkPackage.Create(
            number, request.AircraftId, request.Description, request.PlannedStartDate,
            orgId, actorId, request.PlannedEndDate, request.StationId, request.RelatedWorkOrderId);

        await packages.AddAsync(wp, ct);
        return Result.Success(wp.Id);
    }
}
