using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Inventory.Dtos;

namespace Mro.Application.Features.Inventory.Queries;

public sealed record GetStockItemQuery(Guid Id) : IRequest<Result<StockItemDetailDto>>;

public sealed class GetStockItemQueryHandler(
    IStockItemRepository stockItems,
    IPartRepository parts,
    IBinLocationRepository binLocations,
    ICurrentUserService currentUser)
    : IRequestHandler<GetStockItemQuery, Result<StockItemDetailDto>>
{
    public async Task<Result<StockItemDetailDto>> Handle(GetStockItemQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<StockItemDetailDto>(Error.Forbidden("Organisation context is required."));

        var orgId = currentUser.OrganisationId.Value;
        var s     = await stockItems.GetByIdAsync(request.Id, orgId, ct);
        if (s is null)
            return Result.Failure<StockItemDetailDto>(Error.NotFound("StockItem", request.Id));

        var part = await parts.GetByIdAsync(s.PartId, orgId, ct);
        var bin  = await binLocations.GetByIdAsync(s.BinLocationId, orgId, ct);

        var reservations = s.Reservations.Select(r => new MaterialReservationDto(
            r.Id, r.QuantityReserved, r.QuantityIssued, r.QuantityOutstanding,
            r.WorkOrderId, r.WorkOrderTaskId, r.Status, r.ReservedAt)).ToList();

        var issues = s.Issues.Select(i => new MaterialIssueDto(
            i.Id, i.QuantityIssued, i.BatchNumber, i.SerialNumber, i.IssueSlipRef,
            i.WorkOrderId, i.WorkOrderTaskId, i.IssuedAt)).ToList();

        return Result.Success(new StockItemDetailDto(
            s.Id, s.PartId,
            part?.PartNumber ?? "?", part?.Description ?? "?",
            s.BinLocationId, bin?.Code ?? "?",
            s.QuantityOnHand, s.QuantityReserved, s.QuantityAvailable,
            s.UnitCost, s.BatchNumber, s.SerialNumber, s.ExpiresAt,
            reservations, issues));
    }
}
