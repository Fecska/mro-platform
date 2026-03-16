using Mro.Domain.Aggregates.Inventory.Enums;

namespace Mro.Application.Features.Inventory.Dtos;

public sealed record PartSummaryDto(
    Guid Id,
    string PartNumber,
    string Description,
    string? AtaChapter,
    string UnitOfMeasure,
    string? Manufacturer,
    PartStatus Status);

public sealed record PartDetailDto(
    Guid Id,
    string PartNumber,
    string Description,
    string? AtaChapter,
    string UnitOfMeasure,
    string? Manufacturer,
    string? ManufacturerPartNumber,
    bool TraceabilityRequired,
    decimal MinStockLevel,
    PartStatus Status);

public sealed record BinLocationDto(
    Guid Id,
    string Code,
    string? Description,
    string? StoreRoom,
    bool IsActive);

public sealed record MaterialReservationDto(
    Guid Id,
    decimal QuantityReserved,
    decimal QuantityIssued,
    decimal QuantityOutstanding,
    Guid WorkOrderId,
    Guid WorkOrderTaskId,
    ReservationStatus Status,
    DateTimeOffset ReservedAt);

public sealed record MaterialIssueDto(
    Guid Id,
    decimal QuantityIssued,
    string? BatchNumber,
    string? SerialNumber,
    string? IssueSlipRef,
    Guid WorkOrderId,
    Guid WorkOrderTaskId,
    DateTimeOffset IssuedAt);

public sealed record StockItemSummaryDto(
    Guid Id,
    Guid PartId,
    string PartNumber,
    string PartDescription,
    Guid BinLocationId,
    string BinCode,
    decimal QuantityOnHand,
    decimal QuantityReserved,
    decimal QuantityAvailable,
    decimal UnitCost,
    string? BatchNumber,
    string? SerialNumber,
    DateTimeOffset? ExpiresAt);

public sealed record StockItemDetailDto(
    Guid Id,
    Guid PartId,
    string PartNumber,
    string PartDescription,
    Guid BinLocationId,
    string BinCode,
    decimal QuantityOnHand,
    decimal QuantityReserved,
    decimal QuantityAvailable,
    decimal UnitCost,
    string? BatchNumber,
    string? SerialNumber,
    DateTimeOffset? ExpiresAt,
    IReadOnlyList<MaterialReservationDto> Reservations,
    IReadOnlyList<MaterialIssueDto> Issues);
