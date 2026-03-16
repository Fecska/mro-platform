using Mro.Domain.Aggregates.Defect.Enums;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Defect;

/// <summary>
/// An individual maintenance action performed against a defect.
/// Immutable after creation — corrections must be recorded as new actions.
///
/// EF Core owned-entity navigated via shadow property "_actions" on Defect.
/// </summary>
public sealed class DefectAction : AuditableEntity
{
    public Guid DefectId { get; private set; }

    public ActionType ActionType { get; private set; }

    /// <summary>Free-text description of work performed, findings, or investigation notes.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>User who physically performed the work (may differ from the actor who recorded it).</summary>
    public Guid PerformedByUserId { get; private set; }

    /// <summary>UTC timestamp when the physical work was completed (may be earlier than CreatedAt).</summary>
    public DateTimeOffset PerformedAt { get; private set; }

    /// <summary>
    /// ATA chapter/section reference (e.g. "27-11" for flight controls, rudder).
    /// Nullable — not always applicable (e.g. for investigation-type actions).
    /// </summary>
    public string? AtaReference { get; private set; }

    /// <summary>
    /// Part number of replaced / installed component, if applicable.
    /// </summary>
    public string? PartNumber { get; private set; }

    /// <summary>
    /// Serial number of replaced / installed component, if applicable.
    /// </summary>
    public string? SerialNumber { get; private set; }

    /// <summary>
    /// Work Order ID this action was performed under, if applicable.
    /// Cross-module FK — plain Guid, no navigation property.
    /// </summary>
    public Guid? WorkOrderId { get; private set; }

    // EF Core
    private DefectAction() { }

    internal static DefectAction Create(
        Guid defectId,
        ActionType actionType,
        string description,
        Guid performedByUserId,
        DateTimeOffset performedAt,
        Guid organisationId,
        Guid actorId,
        string? ataReference = null,
        string? partNumber = null,
        string? serialNumber = null,
        Guid? workOrderId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new DefectAction
        {
            DefectId = defectId,
            ActionType = actionType,
            Description = description.Trim(),
            PerformedByUserId = performedByUserId,
            PerformedAt = performedAt,
            AtaReference = ataReference?.Trim().ToUpperInvariant(),
            PartNumber = partNumber?.Trim().ToUpperInvariant(),
            SerialNumber = serialNumber?.Trim().ToUpperInvariant(),
            WorkOrderId = workOrderId,
            OrganisationId = organisationId,
            CreatedBy = actorId,
            UpdatedBy = actorId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }
}
