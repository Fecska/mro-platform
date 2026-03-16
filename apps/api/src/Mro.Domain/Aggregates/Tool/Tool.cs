using Mro.Domain.Aggregates.Tool.Enums;
using Mro.Domain.Aggregates.Tool.Events;
using Mro.Domain.Application;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Tool;

/// <summary>
/// A physical tool managed in the tool store.
///
/// Invariants:
///   - HS-010: tool with expired/missing calibration cannot be checked out.
///   - Retired tools cannot be checked out or sent for calibration.
///   - Checked-out tool must be checked in before calibration or retirement.
/// </summary>
public sealed class Tool : AuditableEntity
{
    private readonly List<CalibrationRecord> _calibrationRecords = [];

    public string ToolNumber { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public ToolCategory Category { get; private set; }
    public ToolStatus Status { get; private set; } = ToolStatus.Available;
    public bool CalibrationRequired { get; private set; }
    public DateTimeOffset? NextCalibrationDue { get; private set; }
    public Guid? CheckedOutToWorkOrderTaskId { get; private set; }
    public Guid? CheckedOutByUserId { get; private set; }
    public DateTimeOffset? CheckedOutAt { get; private set; }
    public string? Location { get; private set; }

    public IReadOnlyList<CalibrationRecord> CalibrationRecords => _calibrationRecords.AsReadOnly();

    public bool IsCalibrationExpired =>
        CalibrationRequired &&
        (NextCalibrationDue is null || NextCalibrationDue.Value < DateTimeOffset.UtcNow);

    private Tool() { }

    public static Tool Create(
        string toolNumber,
        string description,
        ToolCategory category,
        bool calibrationRequired,
        Guid organisationId,
        Guid actorId,
        string? location = null) => new()
    {
        ToolNumber           = toolNumber.ToUpperInvariant(),
        Description          = description,
        Category             = category,
        CalibrationRequired  = calibrationRequired,
        OrganisationId       = organisationId,
        Location             = location,
        CreatedAt            = DateTimeOffset.UtcNow,
        CreatedBy            = actorId,
    };

    // ── Check out (HS-010) ─────────────────────────────────────────────────

    public DomainResult CheckOut(Guid workOrderTaskId, Guid checkedOutByUserId, Guid actorId)
    {
        if (Status == ToolStatus.Retired)
            return DomainResult.Failure("Retired tools cannot be checked out.");
        if (Status != ToolStatus.Available)
            return DomainResult.Failure($"Tool is not available (current status: {Status}).");
        if (IsCalibrationExpired)
            return DomainResult.Failure(
                "HS-010: Tool calibration is expired or missing. Calibrate before use.");

        Status                      = ToolStatus.CheckedOut;
        CheckedOutToWorkOrderTaskId = workOrderTaskId;
        CheckedOutByUserId          = checkedOutByUserId;
        CheckedOutAt                = DateTimeOffset.UtcNow;
        UpdatedAt                   = DateTimeOffset.UtcNow;
        UpdatedBy                   = actorId;

        RaiseDomainEvent(new ToolCheckedOutEvent
        {
            ActorId         = actorId,
            OrganisationId  = OrganisationId,
            EntityType      = "Tool",
            EntityId        = Id,
            EventType       = "TOOL_CHECKED_OUT",
            Description     = $"Tool {ToolNumber} checked out for task {workOrderTaskId}.",
            ToolNumber      = ToolNumber,
            WorkOrderTaskId = workOrderTaskId,
        });

        return DomainResult.Ok();
    }

    // ── Check in ──────────────────────────────────────────────────────────

    public DomainResult CheckIn(Guid actorId)
    {
        if (Status != ToolStatus.CheckedOut)
            return DomainResult.Failure("Tool is not checked out.");

        Status                      = ToolStatus.Available;
        CheckedOutToWorkOrderTaskId = null;
        CheckedOutByUserId          = null;
        CheckedOutAt                = null;
        UpdatedAt                   = DateTimeOffset.UtcNow;
        UpdatedBy                   = actorId;
        return DomainResult.Ok();
    }

    // ── Send for calibration ───────────────────────────────────────────────

    public DomainResult SendForCalibration(Guid actorId)
    {
        if (Status == ToolStatus.Retired)
            return DomainResult.Failure("Retired tools cannot be sent for calibration.");
        if (Status == ToolStatus.CheckedOut)
            return DomainResult.Failure("Check in the tool before sending for calibration.");

        Status    = ToolStatus.UnderCalibration;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = actorId;
        return DomainResult.Ok();
    }

    // ── Record calibration ─────────────────────────────────────────────────

    public DomainResult RecordCalibration(
        DateTimeOffset calibratedAt,
        DateTimeOffset expiresAt,
        string performedBy,
        Guid actorId,
        string? certificateRef = null,
        string? notes = null)
    {
        if (expiresAt <= calibratedAt)
            return DomainResult.Failure("Calibration expiry must be after calibration date.");

        var record = CalibrationRecord.Create(
            Id, calibratedAt, expiresAt, performedBy, OrganisationId, actorId, certificateRef, notes);
        _calibrationRecords.Add(record);

        NextCalibrationDue = expiresAt;
        Status             = ToolStatus.Available;
        UpdatedAt          = DateTimeOffset.UtcNow;
        UpdatedBy          = actorId;

        RaiseDomainEvent(new CalibrationRecordedEvent
        {
            ActorId        = actorId,
            OrganisationId = OrganisationId,
            EntityType     = "Tool",
            EntityId       = Id,
            EventType      = "CALIBRATION_RECORDED",
            Description    = $"Tool {ToolNumber} calibration recorded. Expires {expiresAt:yyyy-MM-dd}.",
            ToolNumber     = ToolNumber,
            ExpiresAt      = expiresAt,
        });

        return DomainResult.Ok();
    }

    // ── Retire ─────────────────────────────────────────────────────────────

    public DomainResult Retire(Guid actorId)
    {
        if (Status == ToolStatus.CheckedOut)
            return DomainResult.Failure("Check in the tool before retiring.");
        Status    = ToolStatus.Retired;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = actorId;
        return DomainResult.Ok();
    }
}
