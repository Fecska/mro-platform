using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Inventory.Commands;

public sealed class AdjustStockCommand : IRequest<Result<Unit>>
{
    public required Guid StockItemId { get; init; }
    public required decimal Delta { get; init; }
    public required string Reason { get; init; }
}

public sealed class AdjustStockCommandHandler(
    IStockItemRepository stockItems,
    ICurrentUserService currentUser)
    : IRequestHandler<AdjustStockCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(AdjustStockCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var actorId = currentUser.UserId!.Value;
        var item    = await stockItems.GetByIdAsync(request.StockItemId, currentUser.OrganisationId.Value, ct);
        if (item is null)
            return Result.Failure<Unit>(Error.NotFound("StockItem", request.StockItemId));

        var result = item.Adjust(request.Delta, request.Reason, actorId);
        if (result.IsFailure)
            return Result.Failure<Unit>(Error.Validation(result.ErrorMessage!));

        await stockItems.UpdateAsync(item, ct);
        return Result.Success(Unit.Value);
    }
}
