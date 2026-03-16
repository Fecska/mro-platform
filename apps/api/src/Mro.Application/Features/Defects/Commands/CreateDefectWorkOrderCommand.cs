using FluentValidation;
using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.WorkOrder;
using Mro.Domain.Aggregates.WorkOrder.Enums;

namespace Mro.Application.Features.Defects.Commands;

public sealed record CreateDefectWorkOrderCommand : IRequest<Result<Guid>>
{
    public required Guid DefectId { get; init; }
    public required string Title { get; init; }
    public Guid? StationId { get; init; }
    public DateTimeOffset? PlannedStartAt { get; init; }
    public DateTimeOffset? PlannedEndAt { get; init; }
}

public sealed class CreateDefectWorkOrderCommandValidator : AbstractValidator<CreateDefectWorkOrderCommand>
{
    public CreateDefectWorkOrderCommandValidator()
    {
        RuleFor(x => x.DefectId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
    }
}

public sealed class CreateDefectWorkOrderCommandHandler(
    IDefectRepository defects,
    IWorkOrderRepository workOrders,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateDefectWorkOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateDefectWorkOrderCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Guid>(Error.Forbidden("Organisation context is required."));

        var orgId   = currentUser.OrganisationId.Value;
        var actorId = currentUser.UserId!.Value;

        var defect = await defects.GetByIdAsync(request.DefectId, orgId, ct);
        if (defect is null)
            return Result.Failure<Guid>(Error.NotFound("Defect", request.DefectId));

        // Generate WO number
        var count    = await workOrders.CountAsync(orgId, null, null, null, ct);
        var woNumber = $"WO-{DateTimeOffset.UtcNow.Year}-{(count + 1):D5}";

        var wo = WorkOrder.Create(
            woNumber,
            WorkOrderType.DefectRectification,
            request.Title,
            defect.AircraftId,
            orgId,
            actorId,
            request.StationId,
            request.PlannedStartAt,
            request.PlannedEndAt,
            customerOrderRef: null,
            originatingDefectId: defect.Id);

        await workOrders.AddAsync(wo, ct);

        // Link the defect to the work order and transition to RectificationInProgress
        var domainResult = defect.StartRectification(wo.Id, actorId);
        if (domainResult.IsFailure)
            return Result.Failure<Guid>(Error.Validation(domainResult.ErrorMessage!));

        await defects.UpdateAsync(defect, ct);
        return Result.Success(wo.Id);
    }
}
