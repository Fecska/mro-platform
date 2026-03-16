using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Inventory.Commands;

public sealed class IssueStockCommand : IRequest<Result<Unit>>
{
    public required Guid StockItemId { get; init; }
    public required decimal Quantity { get; init; }
    public required Guid WorkOrderId { get; init; }
    public required Guid WorkOrderTaskId { get; init; }
    public Guid? ReservationId { get; init; }
    public string? BatchNumber { get; init; }
    public string? SerialNumber { get; init; }
    public string? IssueSlipRef { get; init; }
}

public sealed class IssueStockCommandHandler(
    IStockItemRepository stockItems,
    ICurrentUserService currentUser)
    : IRequestHandler<IssueStockCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(IssueStockCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var actorId = currentUser.UserId!.Value;
        var item    = await stockItems.GetByIdAsync(request.StockItemId, currentUser.OrganisationId.Value, ct);
        if (item is null)
            return Result.Failure<Unit>(Error.NotFound("StockItem", request.StockItemId));

        var result = item.Issue(
            request.ReservationId, request.Quantity,
            request.WorkOrderId, request.WorkOrderTaskId,
            actorId, actorId,
            request.BatchNumber, request.SerialNumber, request.IssueSlipRef);

        if (result.IsFailure)
            return Result.Failure<Unit>(Error.Validation(result.ErrorMessage!));

        await stockItems.UpdateAsync(item, ct);
        return Result.Success(Unit.Value);
    }
}
