using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.WorkOrder;

/// <summary>
/// A part or material required for a specific work order task.
/// When issued from stores, the issue slip reference, quantity, and
/// issuing user are recorded for traceability.
/// </summary>
public sealed class RequiredPart : AuditableEntity
{
    public Guid WorkOrderId { get; private set; }

    public Guid WorkOrderTaskId { get; private set; }

    /// <summary>Manufacturer's part number (ATA spec 2200).</summary>
    public string PartNumber { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public decimal QuantityRequired { get; private set; }

    /// <summary>Unit of measure (e.g. "EA", "M", "L", "KG").</summary>
    public string UnitOfMeasure { get; private set; } = string.Empty;

    /// <summary>
    /// Stores issue slip / GRN reference.
    /// Null until parts are physically issued.
    /// </summary>
    public string? IssueSlipRef { get; private set; }

    public decimal IssuedQuantity { get; private set; }

    public DateTimeOffset? IssuedAt { get; private set; }

    public Guid? IssuedByUserId { get; private set; }

    public bool IsFullyIssued => IssuedQuantity >= QuantityRequired;

    // EF Core
    private RequiredPart() { }

    internal static RequiredPart Create(
        Guid workOrderId,
        Guid workOrderTaskId,
        string partNumber,
        string description,
        decimal quantityRequired,
        string unitOfMeasure,
        Guid organisationId,
        Guid actorId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(unitOfMeasure);

        if (quantityRequired <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantityRequired));

        return new RequiredPart
        {
            WorkOrderId = workOrderId,
            WorkOrderTaskId = workOrderTaskId,
            PartNumber = partNumber.Trim().ToUpperInvariant(),
            Description = description.Trim(),
            QuantityRequired = quantityRequired,
            UnitOfMeasure = unitOfMeasure.Trim().ToUpperInvariant(),
            OrganisationId = organisationId,
            CreatedBy = actorId,
            UpdatedBy = actorId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>Records that parts have been issued from stores against this requirement.</summary>
    internal void RecordIssue(
        string issueSlipRef,
        decimal issuedQuantity,
        Guid issuedByUserId,
        Guid actorId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(issueSlipRef);

        if (issuedQuantity <= 0)
            throw new ArgumentException("Issued quantity must be greater than zero.", nameof(issuedQuantity));

        IssueSlipRef = issueSlipRef.Trim().ToUpperInvariant();
        IssuedQuantity += issuedQuantity;
        IssuedAt = DateTimeOffset.UtcNow;
        IssuedByUserId = issuedByUserId;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
