using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Inspection;
using Mro.Domain.Aggregates.Inspection.Enums;

namespace Mro.Application.Features.Inspections.Commands;

public sealed class CreateInspectionCommand : IRequest<Result<Guid>>
{
    public required Guid WorkOrderId { get; init; }
    public required Guid AircraftId { get; init; }
    public required InspectionType InspectionType { get; init; }
    public required Guid InspectorUserId { get; init; }
    public Guid? WorkOrderTaskId { get; init; }
    public DateTimeOffset? ScheduledAt { get; init; }
}

public sealed class CreateInspectionCommandHandler(
    IInspectionRepository inspections,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateInspectionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateInspectionCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Guid>(Error.Forbidden("Organisation context is required."));

        var orgId   = currentUser.OrganisationId.Value;
        var actorId = currentUser.UserId!.Value;

        var count  = await inspections.CountAsync(orgId, ct);
        var number = $"INS-{DateTimeOffset.UtcNow.Year}-{(count + 1):D5}";

        var inspection = Inspection.Create(
            number, request.WorkOrderId, request.AircraftId, request.InspectionType,
            request.InspectorUserId, orgId, actorId,
            request.WorkOrderTaskId, request.ScheduledAt);

        await inspections.AddAsync(inspection, ct);
        return Result.Success(inspection.Id);
    }
}
