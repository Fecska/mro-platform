using Mro.Domain.Aggregates.Document.Enums;
using Mro.Domain.Aggregates.Document.Events;
using Mro.Domain.Application;
using Mro.Domain.Common.Audit;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Document;

/// <summary>
/// Aggregate root for a controlled technical document used in maintenance.
///
/// Invariants:
///   - Document number is unique per type + organisation
///   - Only one revision may be marked IsCurrent at any time
///   - Cancelled and Superseded documents may not accept new revisions
///   - ADs (AirworthinessDirective type) cannot be Cancelled via user action —
///     they may only be Superseded by a newer AD (HS-006 enforcement point)
///   - A document must be Active before it can be linked to a task
/// </summary>
public sealed class MaintenanceDocument : AuditableEntity
{
    /// <summary>
    /// Issuer's document number (e.g. "SB-737-27-1234", "EASA-AD-2024-0042", "AMM-27-11-00").
    /// Not globally unique — scoped to DocumentType + OrganisationId.
    /// </summary>
    public string DocumentNumber { get; private set; } = string.Empty;

    public DocumentType DocumentType { get; private set; }

    /// <summary>Human-readable document title.</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Organisation or authority that issued the document
    /// (e.g. "Boeing Commercial Airplanes", "EASA", "Internal Engineering").
    /// </summary>
    public string Issuer { get; private set; } = string.Empty;

    public DocumentStatus Status { get; private set; } = DocumentStatus.Draft;

    /// <summary>
    /// For ADs: the regulatory reference number (e.g. "EASA AD 2024-0042").
    /// For SBs: the OEM reference. May be null for internal documents.
    /// </summary>
    public string? RegulatoryReference { get; private set; }

    /// <summary>If this document supersedes another, record the predecessor's ID.</summary>
    public Guid? SupersedesDocumentId { get; private set; }

    /// <summary>If this document has been superseded, record the successor's ID.</summary>
    public Guid? SupersededByDocumentId { get; private set; }

    private readonly List<DocumentRevision> _revisions = [];
    private readonly List<DocumentEffectivity> _effectivities = [];
    private readonly List<TaskDocumentLink> _taskLinks = [];

    public IReadOnlyCollection<DocumentRevision> Revisions => _revisions.AsReadOnly();
    public IReadOnlyCollection<DocumentEffectivity> Effectivities => _effectivities.AsReadOnly();
    public IReadOnlyCollection<TaskDocumentLink> TaskLinks => _taskLinks.AsReadOnly();

    public DocumentRevision? CurrentRevision =>
        _revisions.FirstOrDefault(r => r.IsCurrent);

    // EF Core
    private MaintenanceDocument() { }

    // ── Factory ──────────────────────────────────────────────────────────────

    public static MaintenanceDocument Register(
        string documentNumber,
        DocumentType documentType,
        string title,
        string issuer,
        Guid organisationId,
        Guid actorId,
        string? regulatoryReference = null,
        Guid? supersedesDocumentId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(issuer);

        var doc = new MaintenanceDocument
        {
            DocumentNumber = documentNumber.Trim().ToUpperInvariant(),
            DocumentType = documentType,
            Title = title.Trim(),
            Issuer = issuer.Trim(),
            RegulatoryReference = regulatoryReference?.Trim(),
            SupersedesDocumentId = supersedesDocumentId,
            OrganisationId = organisationId,
            CreatedBy = actorId,
            UpdatedBy = actorId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        doc.RaiseDomainEvent(new DocumentRegisteredEvent
        {
            ActorId = actorId,
            OrganisationId = organisationId,
            EntityType = nameof(MaintenanceDocument),
            EntityId = doc.Id,
            EventType = ComplianceEventType.RecordCreated,
            DocumentNumber = doc.DocumentNumber,
            DocumentType = documentType,
            Title = doc.Title,
            Description = $"Document '{doc.DocumentNumber}' ({documentType}) registered: {doc.Title}.",
        });

        return doc;
    }

    // ── Status machine ───────────────────────────────────────────────────────

    private static readonly IReadOnlyDictionary<DocumentStatus, IReadOnlySet<DocumentStatus>> AllowedTransitions =
        new Dictionary<DocumentStatus, IReadOnlySet<DocumentStatus>>
        {
            [DocumentStatus.Draft]      = new HashSet<DocumentStatus> { DocumentStatus.Active, DocumentStatus.Cancelled },
            [DocumentStatus.Active]     = new HashSet<DocumentStatus> { DocumentStatus.Superseded, DocumentStatus.Cancelled },
            [DocumentStatus.Superseded] = new HashSet<DocumentStatus>(),
            [DocumentStatus.Cancelled]  = new HashSet<DocumentStatus>(),
        };

    private DomainResult SetStatus(DocumentStatus newStatus, Guid actorId)
    {
        if (!AllowedTransitions[Status].Contains(newStatus))
            return DomainResult.Failure($"Transition from '{Status}' to '{newStatus}' is not permitted.");

        Status = newStatus;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }

    /// <summary>Approves the document for operational use.</summary>
    public DomainResult Activate(Guid actorId)
    {
        if (!_revisions.Any())
            return DomainResult.Failure("A document must have at least one revision before it can be activated.");

        return SetStatus(DocumentStatus.Active, actorId);
    }

    /// <summary>
    /// Marks this document as superseded by a newer document.
    /// </summary>
    public DomainResult Supersede(Guid supersededByDocumentId, Guid actorId)
    {
        var result = SetStatus(DocumentStatus.Superseded, actorId);
        if (result.IsFailure) return result;

        SupersededByDocumentId = supersededByDocumentId;

        RaiseDomainEvent(new DocumentSupersededEvent
        {
            ActorId = actorId,
            OrganisationId = OrganisationId,
            EntityType = nameof(MaintenanceDocument),
            EntityId = Id,
            EventType = ComplianceEventType.RecordUpdated,
            DocumentNumber = DocumentNumber,
            SupersededByDocumentId = supersededByDocumentId,
            Description = $"Document '{DocumentNumber}' superseded by document {supersededByDocumentId}.",
        });

        return DomainResult.Ok();
    }

    /// <summary>Cancels the document. Not permitted for ADs (use Supersede instead).</summary>
    public DomainResult Cancel(Guid actorId)
    {
        if (DocumentType == DocumentType.AirworthinessDirective)
            return DomainResult.Failure(
                "Airworthiness Directives cannot be cancelled. Use Supersede to replace with a newer AD.");

        return SetStatus(DocumentStatus.Cancelled, actorId);
    }

    // ── Revisions ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Adds a new revision, demoting any existing current revision.
    /// The new revision automatically becomes IsCurrent.
    /// </summary>
    public DomainResult AddRevision(
        string revisionNumber,
        DateOnly issuedAt,
        DateOnly effectiveAt,
        string storagePath,
        long fileSizeBytes,
        string sha256Checksum,
        Guid uploadedByUserId,
        Guid actorId)
    {
        if (Status is DocumentStatus.Superseded or DocumentStatus.Cancelled)
            return DomainResult.Failure($"Cannot add a revision to a '{Status}' document.");

        // Demote existing current revision
        var current = _revisions.FirstOrDefault(r => r.IsCurrent);
        current?.DemoteFromCurrent(actorId);

        var revision = DocumentRevision.Create(
            Id, revisionNumber, issuedAt, effectiveAt,
            storagePath, fileSizeBytes, sha256Checksum,
            uploadedByUserId, OrganisationId, actorId);

        _revisions.Add(revision);
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new DocumentRevisionAddedEvent
        {
            ActorId = actorId,
            OrganisationId = OrganisationId,
            EntityType = nameof(MaintenanceDocument),
            EntityId = Id,
            EventType = ComplianceEventType.RecordUpdated,
            RevisionNumber = revisionNumber,
            RevisionId = revision.Id,
            EffectiveAt = effectiveAt,
            Description = $"Revision '{revisionNumber}' added to '{DocumentNumber}', effective {effectiveAt:yyyy-MM-dd}.",
        });

        return DomainResult.Ok();
    }

    // ── Effectivities ─────────────────────────────────────────────────────────

    /// <summary>Adds an aircraft applicability record.</summary>
    public void AddEffectivity(
        string? icaoTypeCode,
        string? serialFrom,
        string? serialTo,
        string? conditionNote,
        Guid actorId)
    {
        var eff = DocumentEffectivity.Create(
            Id, icaoTypeCode, serialFrom, serialTo, conditionNote, OrganisationId, actorId);
        _effectivities.Add(eff);
    }

    /// <summary>
    /// Returns true if this document applies to the given aircraft.
    /// If no effectivity records are defined, the document is considered universal.
    /// </summary>
    public bool AppliesToAircraft(string icaoTypeCode, string serialNumber)
    {
        if (!_effectivities.Any()) return true;
        return _effectivities.Any(e => e.Covers(icaoTypeCode, serialNumber));
    }

    // ── Task links ────────────────────────────────────────────────────────────

    /// <summary>
    /// Links this document to a maintenance task.
    /// Only Active documents may be linked (enforced here and by HS-007).
    /// </summary>
    public DomainResult LinkToTask(
        Guid taskId,
        DocumentLinkType linkType,
        string? ataReference,
        Guid actorId)
    {
        if (Status != DocumentStatus.Active)
            return DomainResult.Failure($"Only Active documents can be linked to tasks. Current status: {Status}.");

        if (CurrentRevision is null)
            return DomainResult.Failure("Document has no current revision.");

        var duplicate = _taskLinks.FirstOrDefault(l => l.TaskId == taskId);
        if (duplicate is not null)
            return DomainResult.Failure("This document is already linked to the specified task.");

        var link = TaskDocumentLink.Create(
            Id, taskId, CurrentRevision.Id, linkType, ataReference, OrganisationId, actorId);
        _taskLinks.Add(link);
        return DomainResult.Ok();
    }

    /// <summary>Technician acknowledges having read the mandatory document.</summary>
    public DomainResult AcknowledgeLink(Guid taskId, Guid userId, Guid actorId)
    {
        var link = _taskLinks.FirstOrDefault(l => l.TaskId == taskId);
        if (link is null)
            return DomainResult.Failure("Document is not linked to this task.");

        link.Acknowledge(userId, actorId);
        return DomainResult.Ok();
    }
}
