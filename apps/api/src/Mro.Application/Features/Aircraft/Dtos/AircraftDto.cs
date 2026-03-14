namespace Mro.Application.Features.Aircraft.Dtos;

public sealed record AircraftSummaryDto(
    Guid Id,
    string Registration,
    string SerialNumber,
    string AircraftTypeCode,
    string Manufacturer,
    string Model,
    string Status,
    DateOnly ManufactureDate,
    string? Remarks);

public sealed record AircraftDetailDto(
    Guid Id,
    string Registration,
    string SerialNumber,
    Guid AircraftTypeId,
    string AircraftTypeCode,
    string Manufacturer,
    string Model,
    string Status,
    DateOnly ManufactureDate,
    string? Remarks,
    IReadOnlyList<CounterDto> Counters,
    IReadOnlyList<InstalledComponentDto> InstalledComponents,
    IReadOnlyList<StatusHistoryDto> StatusHistory);

public sealed record CounterDto(
    string CounterType,
    decimal Value,
    DateTimeOffset LastUpdatedAt);

public sealed record InstalledComponentDto(
    Guid Id,
    string PartNumber,
    string SerialNumber,
    string Description,
    string InstallationPosition,
    DateTimeOffset InstalledAt,
    Guid InstalledByUserId,
    Guid? InstallationWorkOrderId);

public sealed record StatusHistoryDto(
    string FromStatus,
    string ToStatus,
    string Reason,
    Guid ActorId,
    DateTimeOffset ChangedAt);
