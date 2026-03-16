using Mro.Domain.Aggregates.WorkOrder.Enums;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.WorkOrder;

/// <summary>
/// Records an active impediment to progressing a Work Order.
/// Blockers are orthogonal to WO status — multiple blockers may exist simultaneously.
/// Resolving all blockers is required before the WO can resume InProgress.
/// </summary>
public sealed class WorkOrderBlocker : AuditableEntity
{
    public Guid WorkOrderId { get; private set; }

    public BlockerType BlockerType { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public Guid RaisedByUserId { get; private set; }

    public DateTimeOffset RaisedAt { get; private set; }

    public bool IsResolved { get; private set; }

    public DateTimeOffset? ResolvedAt { get; private set; }

    public Guid? ResolvedByUserId { get; private set; }

    public string? ResolutionNote { get; private set; }

    // EF Core
    private WorkOrderBlocker() { }

    internal static WorkOrderBlocker Create(
        Guid workOrderId,
        BlockerType blockerType,
        string description,
        Guid raisedByUserId,
        Guid organisationId,
        Guid actorId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new WorkOrderBlocker
        {
            WorkOrderId = workOrderId,
            BlockerType = blockerType,
            Description = description.Trim(),
            RaisedByUserId = raisedByUserId,
            RaisedAt = DateTimeOffset.UtcNow,
            IsResolved = false,
            OrganisationId = organisationId,
            CreatedBy = actorId,
            UpdatedBy = actorId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    internal void Resolve(string resolutionNote, Guid resolvedByUserId, Guid actorId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resolutionNote);

        IsResolved = true;
        ResolvedAt = DateTimeOffset.UtcNow;
        ResolvedByUserId = resolvedByUserId;
        ResolutionNote = resolutionNote.Trim();
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
