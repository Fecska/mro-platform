using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Inventory.Dtos;

namespace Mro.Application.Features.Inventory.Queries;

public sealed record ListStockItemsQuery(
    Guid? PartId,
    Guid? BinLocationId,
    int Page,
    int PageSize) : IRequest<Result<IReadOnlyList<StockItemSummaryDto>>>;

public sealed class ListStockItemsQueryHandler(
    IStockItemRepository stockItems,
    IPartRepository parts,
    IBinLocationRepository binLocations,
    ICurrentUserService currentUser)
    : IRequestHandler<ListStockItemsQuery, Result<IReadOnlyList<StockItemSummaryDto>>>
{
    public async Task<Result<IReadOnlyList<StockItemSummaryDto>>> Handle(ListStockItemsQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<IReadOnlyList<StockItemSummaryDto>>(Error.Forbidden("Organisation context is required."));

        var orgId = currentUser.OrganisationId.Value;
        var list  = await stockItems.ListAsync(orgId, request.PartId, request.BinLocationId, request.Page, request.PageSize, ct);

        var dtos = new List<StockItemSummaryDto>();
        foreach (var s in list)
        {
            var part = await parts.GetByIdAsync(s.PartId, orgId, ct);
            var bin  = await binLocations.GetByIdAsync(s.BinLocationId, orgId, ct);
            dtos.Add(new StockItemSummaryDto(
                s.Id, s.PartId,
                part?.PartNumber ?? "?", part?.Description ?? "?",
                s.BinLocationId,
                bin?.Code ?? "?",
                s.QuantityOnHand, s.QuantityReserved, s.QuantityAvailable,
                s.UnitCost, s.BatchNumber, s.SerialNumber, s.ExpiresAt));
        }

        return Result.Success<IReadOnlyList<StockItemSummaryDto>>(dtos);
    }
}
