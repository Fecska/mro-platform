using Mro.Domain.Aggregates.Tool.Enums;

namespace Mro.Application.Features.Tools.Dtos;

public sealed record CalibrationRecordDto(
    Guid Id,
    DateTimeOffset CalibratedAt,
    DateTimeOffset ExpiresAt,
    string PerformedBy,
    string? CertificateRef,
    string? Notes);

public sealed record ToolSummaryDto(
    Guid Id,
    string ToolNumber,
    string Description,
    ToolCategory Category,
    ToolStatus Status,
    bool CalibrationRequired,
    bool IsCalibrationExpired,
    DateTimeOffset? NextCalibrationDue,
    string? Location);

public sealed record ToolDetailDto(
    Guid Id,
    string ToolNumber,
    string Description,
    ToolCategory Category,
    ToolStatus Status,
    bool CalibrationRequired,
    bool IsCalibrationExpired,
    DateTimeOffset? NextCalibrationDue,
    Guid? CheckedOutToWorkOrderTaskId,
    Guid? CheckedOutByUserId,
    DateTimeOffset? CheckedOutAt,
    string? Location,
    IReadOnlyList<CalibrationRecordDto> CalibrationRecords);
