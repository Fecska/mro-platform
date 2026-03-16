namespace Mro.Application.Features.Defects.Dtos;

public sealed record DefectSummaryDto(
    Guid Id,
    string DefectNumber,
    Guid AircraftId,
    string Status,
    string Severity,
    string Source,
    string AtaChapter,
    string Description,
    DateTimeOffset DiscoveredAt,
    bool IsAdMandated,
    Guid? AssignedToUserId,
    DateTimeOffset CreatedAt);

public sealed record DefectDetailDto(
    Guid Id,
    string DefectNumber,
    Guid AircraftId,
    string Status,
    string Severity,
    string Source,
    string AtaChapter,
    string Description,
    DateTimeOffset DiscoveredAt,
    Guid? DiscoveredAtStationId,
    bool IsAdMandated,
    Guid? AdDocumentId,
    Guid? AssignedToUserId,
    Guid? WorkOrderId,
    string? ClosureReason,
    IReadOnlyList<DefectActionDto> Actions,
    DeferredDefectDto? ActiveDeferral,
    DateTimeOffset CreatedAt,
    Guid CreatedBy);

public sealed record DefectActionDto(
    Guid Id,
    string ActionType,
    string Description,
    Guid PerformedByUserId,
    DateTimeOffset PerformedAt,
    string? AtaReference,
    string? PartNumber,
    string? SerialNumber,
    Guid? WorkOrderId);

public sealed record DeferredDefectDto(
    Guid Id,
    DateTimeOffset DeferredFrom,
    DateTimeOffset DeferredUntil,
    Guid ApprovedByUserId,
    Guid SignedByUserId,
    string? LogReference,
    bool IsExpired);
