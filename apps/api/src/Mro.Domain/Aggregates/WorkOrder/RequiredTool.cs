using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.WorkOrder;

/// <summary>
/// A calibrated tool or Ground Support Equipment (GSE) required for a task.
///
/// Invariant (HS-010):
///   A tool with an expired calibration may not be checked out.
///   The domain enforces this via CheckOut() — EF also stores the check to allow
///   reporting of near-expiry tools.
/// </summary>
public sealed class RequiredTool : AuditableEntity
{
    public Guid WorkOrderId { get; private set; }

    public Guid WorkOrderTaskId { get; private set; }

    /// <summary>Workshop/toolstore identifier (e.g. "TORQUE-42", "BORESCOPE-B12").</summary>
    public string ToolNumber { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Date after which the calibration is no longer valid.
    /// Null for non-calibrated consumables (e.g. safety wire).
    /// </summary>
    public DateOnly? CalibratedExpiry { get; private set; }

    public bool IsCheckedOut { get; private set; }

    public DateTimeOffset? CheckedOutAt { get; private set; }

    public Guid? CheckedOutByUserId { get; private set; }

    public DateTimeOffset? ReturnedAt { get; private set; }

    public bool IsCalibrationExpired =>
        CalibratedExpiry.HasValue && CalibratedExpiry.Value < DateOnly.FromDateTime(DateTime.UtcNow.Date);

    // EF Core
    private RequiredTool() { }

    internal static RequiredTool Create(
        Guid workOrderId,
        Guid workOrderTaskId,
        string toolNumber,
        string description,
        Guid organisationId,
        Guid actorId,
        DateOnly? calibratedExpiry = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toolNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new RequiredTool
        {
            WorkOrderId = workOrderId,
            WorkOrderTaskId = workOrderTaskId,
            ToolNumber = toolNumber.Trim().ToUpperInvariant(),
            Description = description.Trim(),
            CalibratedExpiry = calibratedExpiry,
            OrganisationId = organisationId,
            CreatedBy = actorId,
            UpdatedBy = actorId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Checks out the tool to a technician.
    /// Hard Stop HS-010: blocked if calibration is expired.
    /// </summary>
    internal Domain.Application.DomainResult CheckOut(Guid userId, Guid actorId)
    {
        if (IsCheckedOut)
            return Domain.Application.DomainResult.Failure(
                $"Tool '{ToolNumber}' is already checked out.");

        if (IsCalibrationExpired)
            return Domain.Application.DomainResult.Failure(
                $"Hard Stop HS-010: Tool '{ToolNumber}' calibration expired on {CalibratedExpiry}. " +
                "Recalibrate before use.");

        IsCheckedOut = true;
        CheckedOutAt = DateTimeOffset.UtcNow;
        CheckedOutByUserId = userId;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Domain.Application.DomainResult.Ok();
    }

    /// <summary>Returns the tool to stores after use.</summary>
    internal void Return(Guid actorId)
    {
        IsCheckedOut = false;
        ReturnedAt = DateTimeOffset.UtcNow;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
