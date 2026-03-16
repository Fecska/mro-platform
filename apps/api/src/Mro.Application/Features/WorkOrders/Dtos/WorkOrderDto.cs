namespace Mro.Application.Features.WorkOrders.Dtos;

public sealed record WorkOrderSummaryDto(
    Guid Id,
    string WoNumber,
    string WorkOrderType,
    string Title,
    string Status,
    Guid AircraftId,
    Guid? StationId,
    DateTimeOffset? PlannedStartAt,
    DateTimeOffset? PlannedEndAt,
    int TaskCount,
    int SignedOffTaskCount,
    DateTimeOffset CreatedAt);

public sealed record WorkOrderDetailDto(
    Guid Id,
    string WoNumber,
    string WorkOrderType,
    string Title,
    string Status,
    Guid AircraftId,
    Guid? StationId,
    DateTimeOffset? PlannedStartAt,
    DateTimeOffset? PlannedEndAt,
    DateTimeOffset? ActualStartAt,
    DateTimeOffset? ActualEndAt,
    string? CustomerOrderRef,
    Guid? OriginatingDefectId,
    bool AllTasksSignedOff,
    IReadOnlyList<WorkOrderTaskDto> Tasks,
    IReadOnlyList<WorkOrderAssignmentDto> Assignments,
    IReadOnlyList<WorkOrderBlockerDto> ActiveBlockers,
    DateTimeOffset CreatedAt,
    Guid CreatedBy);

public sealed record WorkOrderTaskDto(
    Guid Id,
    string TaskNumber,
    string Title,
    string AtaChapter,
    string Description,
    string Status,
    string? RequiredLicence,
    decimal EstimatedHours,
    decimal TotalHoursLogged,
    bool IsSignedOff,
    Guid? SignedOffByUserId,
    DateTimeOffset? SignedOffAt,
    Guid? DocumentId,
    IReadOnlyList<LabourEntryDto> LabourEntries,
    IReadOnlyList<RequiredPartDto> RequiredParts,
    IReadOnlyList<RequiredToolDto> RequiredTools);

public sealed record LabourEntryDto(
    Guid Id,
    Guid PerformedByUserId,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    decimal Hours,
    string? Notes);

public sealed record RequiredPartDto(
    Guid Id,
    string PartNumber,
    string Description,
    decimal QuantityRequired,
    string UnitOfMeasure,
    string? IssueSlipRef,
    decimal IssuedQuantity,
    bool IsFullyIssued);

public sealed record RequiredToolDto(
    Guid Id,
    string ToolNumber,
    string Description,
    DateOnly? CalibratedExpiry,
    bool IsCheckedOut,
    DateTimeOffset? CheckedOutAt,
    bool IsCalibrationExpired);

public sealed record WorkOrderAssignmentDto(
    Guid UserId,
    string Role,
    DateTimeOffset AssignedAt);

public sealed record WorkOrderBlockerDto(
    Guid Id,
    string BlockerType,
    string Description,
    Guid RaisedByUserId,
    DateTimeOffset RaisedAt);
