using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.WorkOrder.Enums;
using DomainResult = Mro.Domain.Application.DomainResult;

namespace Mro.Application.Features.WorkOrders.Commands;

public sealed class ChangeWorkOrderStatusCommand : IRequest<Result<Unit>>
{
    public required Guid WorkOrderId { get; init; }
    public required WorkOrderStatus NewStatus { get; init; }
    /// <summary>Required for Cancelled.</summary>
    public string? CancellationReason { get; init; }
    /// <summary>Required for Completed.</summary>
    public Guid? CertifyingStaffUserId { get; init; }
}

public sealed class ChangeWorkOrderStatusCommandHandler(
    IWorkOrderRepository workOrders,
    ICurrentUserService currentUser)
    : IRequestHandler<ChangeWorkOrderStatusCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ChangeWorkOrderStatusCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var wo = await workOrders.GetByIdAsync(request.WorkOrderId, currentUser.OrganisationId.Value, ct);
        if (wo is null)
            return Result.Failure<Unit>(Error.NotFound("WorkOrder", request.WorkOrderId));

        var actorId = currentUser.UserId!.Value;

        var domainResult = request.NewStatus switch
        {
            WorkOrderStatus.Planned              => wo.Plan(actorId),
            WorkOrderStatus.Issued               => wo.Issue(actorId),
            WorkOrderStatus.InProgress           => wo.StartWork(actorId),
            WorkOrderStatus.WaitingInspection    => wo.SubmitForInspection(actorId),
            WorkOrderStatus.WaitingCertification => wo.SubmitForCertification(actorId),
            WorkOrderStatus.Completed            => request.CertifyingStaffUserId.HasValue
                ? wo.Complete(request.CertifyingStaffUserId.Value, actorId)
                : DomainResult.Failure("CertifyingStaffUserId is required to complete a work order."),
            WorkOrderStatus.Closed               => wo.Close(actorId),
            WorkOrderStatus.Cancelled            => !string.IsNullOrWhiteSpace(request.CancellationReason)
                ? wo.Cancel(request.CancellationReason, actorId)
                : DomainResult.Failure("CancellationReason is required to cancel a work order."),
            _ => DomainResult.Failure($"Use a dedicated command for '{request.NewStatus}' transitions."),
        };

        if (domainResult.IsFailure)
            return Result.Failure<Unit>(Error.Validation(domainResult.ErrorMessage!));

        await workOrders.UpdateAsync(wo, ct);
        return Result.Success(Unit.Value);
    }
}
