using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Inventory;

namespace Mro.Application.Features.Inventory.Commands;

/// <summary>
/// Receive new stock into a bin. If a matching StockItem already exists (same Part + Bin + Batch),
/// increase quantity; otherwise create a new StockItem.
/// </summary>
public sealed class ReceiveStockCommand : IRequest<Result<Guid>>
{
    public required Guid PartId { get; init; }
    public required Guid BinLocationId { get; init; }
    public required decimal Quantity { get; init; }
    public required decimal UnitCost { get; init; }
    public string? BatchNumber { get; init; }
    public string? SerialNumber { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
}

public sealed class ReceiveStockCommandHandler(
    IStockItemRepository stockItems,
    ICurrentUserService currentUser)
    : IRequestHandler<ReceiveStockCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(ReceiveStockCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Guid>(Error.Forbidden("Organisation context is required."));

        var orgId   = currentUser.OrganisationId.Value;
        var actorId = currentUser.UserId!.Value;

        // Always create a new StockItem per receipt (traceability requires separate lines for batches)
        var item = StockItem.Create(
            request.PartId, request.BinLocationId, request.Quantity, request.UnitCost,
            orgId, actorId, request.BatchNumber, request.SerialNumber, request.ExpiresAt);

        await stockItems.AddAsync(item, ct);
        return Result.Success(item.Id);
    }
}
