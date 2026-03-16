using Mro.Domain.Aggregates.Maintenance.Enums;

namespace Mro.Application.Features.Maintenance.Dtos;

public sealed record MaintenanceProgramDto(
    Guid Id,
    string ProgramNumber,
    string AircraftTypeCode,
    string Title,
    string RevisionNumber,
    DateOnly RevisionDate,
    string? ApprovalReference,
    bool IsActive);

public sealed record DueItemSummaryDto(
    Guid Id,
    string DueItemRef,
    Guid AircraftId,
    DueItemType DueItemType,
    IntervalType IntervalType,
    string Description,
    DueStatus Status,
    DateTimeOffset? NextDueDate,
    decimal? NextDueHours,
    int? NextDueCycles,
    DateTimeOffset? LastAccomplishedAt);

public sealed record DueItemDetailDto(
    Guid Id,
    string DueItemRef,
    Guid AircraftId,
    Guid? MaintenanceProgramId,
    DueItemType DueItemType,
    IntervalType IntervalType,
    string Description,
    string? RegulatoryRef,
    decimal? IntervalValue,
    int? IntervalDays,
    decimal? ToleranceValue,
    DueStatus Status,
    DateTimeOffset? NextDueDate,
    decimal? NextDueHours,
    int? NextDueCycles,
    DateTimeOffset? LastAccomplishedAt,
    decimal? LastAccomplishedAtHours,
    int? LastAccomplishedAtCycles,
    Guid? LastAccomplishedWorkOrderId);

public sealed record PackageItemDto(
    Guid Id,
    Guid? DueItemId,
    string Description,
    string? TaskReference,
    PackageItemStatus Status,
    decimal? EstimatedManHours,
    decimal? ActualManHours,
    Guid? RelatedWorkOrderId,
    string? DeferralReason,
    string? NotApplicableReason);

public sealed record WorkPackageSummaryDto(
    Guid Id,
    string PackageNumber,
    Guid AircraftId,
    string Description,
    WorkPackageStatus Status,
    DateOnly PlannedStartDate,
    DateOnly? PlannedEndDate,
    int ItemCount,
    int PendingItemCount);

public sealed record WorkPackageDetailDto(
    Guid Id,
    string PackageNumber,
    Guid AircraftId,
    string Description,
    WorkPackageStatus Status,
    DateOnly PlannedStartDate,
    DateOnly? PlannedEndDate,
    DateTimeOffset? ActualStartDate,
    DateTimeOffset? ActualEndDate,
    Guid? StationId,
    Guid? RelatedWorkOrderId,
    IReadOnlyList<PackageItemDto> Items,
    DateTimeOffset CreatedAt);
