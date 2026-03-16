using Mro.Domain.Aggregates.Inspection.Enums;

namespace Mro.Application.Features.Inspections.Dtos;

public sealed record InspectionSummaryDto(
    Guid Id,
    string InspectionNumber,
    Guid WorkOrderId,
    Guid? WorkOrderTaskId,
    Guid AircraftId,
    InspectionType InspectionType,
    InspectionStatus Status,
    Guid InspectorUserId,
    DateTimeOffset? ScheduledAt,
    DateTimeOffset? CompletedAt);

public sealed record InspectionDetailDto(
    Guid Id,
    string InspectionNumber,
    Guid WorkOrderId,
    Guid? WorkOrderTaskId,
    Guid AircraftId,
    InspectionType InspectionType,
    InspectionStatus Status,
    Guid InspectorUserId,
    DateTimeOffset? ScheduledAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    string? Findings,
    string? OutcomeRemarks,
    string? WaiverReason,
    DateTimeOffset CreatedAt);
