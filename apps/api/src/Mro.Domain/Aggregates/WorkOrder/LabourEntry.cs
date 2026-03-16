using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.WorkOrder;

/// <summary>
/// Records actual labour time booked against a work order task.
/// Immutable after creation — corrections must be added as new entries
/// with a reference to the corrected entry.
/// </summary>
public sealed class LabourEntry : AuditableEntity
{
    public Guid WorkOrderId { get; private set; }

    public Guid WorkOrderTaskId { get; private set; }

    /// <summary>User who performed the work (may differ from the user who entered the record).</summary>
    public Guid PerformedByUserId { get; private set; }

    public DateTimeOffset StartAt { get; private set; }

    public DateTimeOffset EndAt { get; private set; }

    /// <summary>Elapsed time in decimal hours (e.g. 1.5 = 90 minutes).</summary>
    public decimal Hours => (decimal)(EndAt - StartAt).TotalHours;

    public string? Notes { get; private set; }

    // EF Core
    private LabourEntry() { }

    internal static LabourEntry Create(
        Guid workOrderId,
        Guid workOrderTaskId,
        Guid performedByUserId,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        Guid organisationId,
        Guid actorId,
        string? notes = null)
    {
        if (endAt <= startAt)
            throw new ArgumentException("EndAt must be after StartAt.", nameof(endAt));

        return new LabourEntry
        {
            WorkOrderId = workOrderId,
            WorkOrderTaskId = workOrderTaskId,
            PerformedByUserId = performedByUserId,
            StartAt = startAt,
            EndAt = endAt,
            Notes = notes?.Trim(),
            OrganisationId = organisationId,
            CreatedBy = actorId,
            UpdatedBy = actorId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }
}
