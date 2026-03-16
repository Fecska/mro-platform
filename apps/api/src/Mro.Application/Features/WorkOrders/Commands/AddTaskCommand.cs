using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.WorkOrders.Commands;

public sealed class AddTaskCommand : IRequest<Result<Unit>>
{
    public required Guid WorkOrderId { get; init; }
    public required string Title { get; init; }
    public required string AtaChapter { get; init; }
    public required string Description { get; init; }
    public required decimal EstimatedHours { get; init; }
    public string? RequiredLicence { get; init; }
    public Guid? DocumentId { get; init; }
}

public sealed class AddTaskCommandHandler(
    IWorkOrderRepository workOrders,
    ICurrentUserService currentUser)
    : IRequestHandler<AddTaskCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(AddTaskCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var wo = await workOrders.GetByIdAsync(request.WorkOrderId, currentUser.OrganisationId.Value, ct);
        if (wo is null)
            return Result.Failure<Unit>(Error.NotFound("WorkOrder", request.WorkOrderId));

        var result = wo.AddTask(
            request.Title, request.AtaChapter, request.Description,
            request.EstimatedHours, currentUser.UserId!.Value,
            request.RequiredLicence, request.DocumentId);

        if (result.IsFailure)
            return Result.Failure<Unit>(Error.Validation(result.ErrorMessage!));

        await workOrders.UpdateAsync(wo, ct);
        return Result.Success(Unit.Value);
    }
}
