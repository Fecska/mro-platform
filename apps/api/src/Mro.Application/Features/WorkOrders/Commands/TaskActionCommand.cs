using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using DomainResult = Mro.Domain.Application.DomainResult;

namespace Mro.Application.Features.WorkOrders.Commands;

/// <summary>
/// Handles task lifecycle transitions: Start, Complete, and CRS SignOff.
/// </summary>
public sealed class TaskActionCommand : IRequest<Result<Unit>>
{
    public required Guid WorkOrderId { get; init; }
    public required Guid TaskId { get; init; }
    public required TaskAction Action { get; init; }
    /// <summary>Required for SignOff action.</summary>
    public Guid? CertifyingStaffUserId { get; init; }
    /// <summary>Required for SignOff action.</summary>
    public string? SignOffRemark { get; init; }
}

public enum TaskAction { Start, Complete, SignOff }

public sealed class TaskActionCommandHandler(
    IWorkOrderRepository workOrders,
    ICurrentUserService currentUser)
    : IRequestHandler<TaskActionCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(TaskActionCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var wo = await workOrders.GetByIdAsync(request.WorkOrderId, currentUser.OrganisationId.Value, ct);
        if (wo is null)
            return Result.Failure<Unit>(Error.NotFound("WorkOrder", request.WorkOrderId));

        var actorId = currentUser.UserId!.Value;

        var domainResult = request.Action switch
        {
            TaskAction.Start    => wo.StartTask(request.TaskId, actorId),
            TaskAction.Complete => wo.CompleteTask(request.TaskId, actorId),
            TaskAction.SignOff  => request.CertifyingStaffUserId.HasValue
                ? wo.SignOffTask(request.TaskId, request.CertifyingStaffUserId.Value,
                      request.SignOffRemark ?? string.Empty, actorId)
                : DomainResult.Failure("CertifyingStaffUserId is required for sign-off."),
            _ => DomainResult.Failure("Unknown task action."),
        };

        if (domainResult.IsFailure)
            return Result.Failure<Unit>(Error.Validation(domainResult.ErrorMessage!));

        await workOrders.UpdateAsync(wo, ct);
        return Result.Success(Unit.Value);
    }
}
