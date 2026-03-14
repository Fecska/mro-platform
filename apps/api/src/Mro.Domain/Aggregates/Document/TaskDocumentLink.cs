using Mro.Domain.Aggregates.Document.Enums;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Document;

/// <summary>
/// Associates a maintenance task (by ID — in the WorkOrder module) with a document
/// and specifies whether the document must be consulted before sign-off.
///
/// Lifecycle:
///   Created when a planner attaches a document to a task during work-package planning.
///   The technician executing the task must acknowledge Mandatory links (HS-007).
///   Acknowledged = they confirm having read the current revision.
///
/// Note: TaskId refers to the Task entity in the WorkOrder aggregate.
///       Cross-module coupling is intentional (document → task reference is read-only).
/// </summary>
public sealed class TaskDocumentLink : AuditableEntity
{
    public Guid DocumentId { get; private set; }

    /// <summary>FK to the task within the WorkOrder module. No navigation property — cross-module.</summary>
    public Guid TaskId { get; private set; }

    /// <summary>The document revision that was current when the link was created.</summary>
    public Guid RevisionId { get; private set; }

    public DocumentLinkType LinkType { get; private set; }

    /// <summary>Optional ATA chapter reference (e.g. "27-11-00").</summary>
    public string? AtaReference { get; private set; }

    /// <summary>Whether the technician has acknowledged reading this revision. Only meaningful for Mandatory links.</summary>
    public bool IsAcknowledged { get; private set; }

    public Guid? AcknowledgedByUserId { get; private set; }
    public DateTimeOffset? AcknowledgedAt { get; private set; }

    // EF Core
    private TaskDocumentLink() { }

    internal static TaskDocumentLink Create(
        Guid documentId,
        Guid taskId,
        Guid revisionId,
        DocumentLinkType linkType,
        string? ataReference,
        Guid organisationId,
        Guid actorId)
    {
        return new TaskDocumentLink
        {
            DocumentId = documentId,
            TaskId = taskId,
            RevisionId = revisionId,
            LinkType = linkType,
            AtaReference = ataReference?.Trim(),
            IsAcknowledged = linkType != DocumentLinkType.Mandatory, // non-mandatory auto-acknowledged
            OrganisationId = organisationId,
            CreatedBy = actorId,
            UpdatedBy = actorId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>Technician acknowledges having read this document revision.</summary>
    internal void Acknowledge(Guid userId, Guid actorId)
    {
        if (IsAcknowledged) return;
        IsAcknowledged = true;
        AcknowledgedByUserId = userId;
        AcknowledgedAt = DateTimeOffset.UtcNow;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
