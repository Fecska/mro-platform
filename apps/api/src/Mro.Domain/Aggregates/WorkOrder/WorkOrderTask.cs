using Mro.Domain.Aggregates.WorkOrder.Enums;
using Mro.Domain.Application;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.WorkOrder;

/// <summary>
/// An individual task card within a Work Order.
///
/// Invariants:
///   - A task may only be signed off once; subsequent changes require a new task (HS-009).
///   - CRS sign-off requires a certifying staff user ID to be provided explicitly.
///   - RequiredTool checkout enforces calibration expiry (HS-010 via RequiredTool.CheckOut).
/// </summary>
public sealed class WorkOrderTask : AuditableEntity
{
    public Guid WorkOrderId { get; private set; }

    /// <summary>Sequential task number within the WO (e.g. "T01", "T02").</summary>
    public string TaskNumber { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string AtaChapter { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public WorkOrderTaskStatus Status { get; private set; } = WorkOrderTaskStatus.Pending;

    /// <summary>Required licence category (e.g. "B1", "B2", "Cat A", "Cat C"). Nullable = any.</summary>
    public string? RequiredLicence { get; private set; }

    /// <summary>Planned duration in decimal hours.</summary>
    public decimal EstimatedHours { get; private set; }

    /// <summary>
    /// Reference document ID (cross-module FK to MaintenanceDocument).
    /// Null if no AMM/SRM/AD reference is attached.
    /// </summary>
    public Guid? DocumentId { get; private set; }

    // CRS sign-off fields
    public bool IsSignedOff { get; private set; }
    public Guid? SignedOffByUserId { get; private set; }
    public DateTimeOffset? SignedOffAt { get; private set; }
    public string? SignOffRemark { get; private set; }

    private readonly List<LabourEntry> _labourEntries = [];
    private readonly List<RequiredPart> _requiredParts = [];
    private readonly List<RequiredTool> _requiredTools = [];

    public IReadOnlyCollection<LabourEntry> LabourEntries => _labourEntries.AsReadOnly();
    public IReadOnlyCollection<RequiredPart> RequiredParts => _requiredParts.AsReadOnly();
    public IReadOnlyCollection<RequiredTool> RequiredTools => _requiredTools.AsReadOnly();

    public decimal TotalHoursLogged => _labourEntries.Sum(e => e.Hours);

    // EF Core
    private WorkOrderTask() { }

    internal static WorkOrderTask Create(
        Guid workOrderId,
        string taskNumber,
        string title,
        string ataChapter,
        string description,
        decimal estimatedHours,
        Guid organisationId,
        Guid actorId,
        string? requiredLicence = null,
        Guid? documentId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(ataChapter);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        if (estimatedHours <= 0)
            throw new ArgumentException("Estimated hours must be greater than zero.", nameof(estimatedHours));

        return new WorkOrderTask
        {
            WorkOrderId = workOrderId,
            TaskNumber = taskNumber.Trim().ToUpperInvariant(),
            Title = title.Trim(),
            AtaChapter = ataChapter.Trim(),
            Description = description.Trim(),
            EstimatedHours = estimatedHours,
            RequiredLicence = requiredLicence?.Trim(),
            DocumentId = documentId,
            OrganisationId = organisationId,
            CreatedBy = actorId,
            UpdatedBy = actorId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    // ── Task lifecycle ────────────────────────────────────────────────────────

    internal DomainResult StartWork(Guid actorId)
    {
        if (Status != WorkOrderTaskStatus.Pending)
            return DomainResult.Failure($"Task '{TaskNumber}' is already '{Status}'.");

        Status = WorkOrderTaskStatus.InProgress;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }

    internal DomainResult CompleteWork(Guid actorId)
    {
        if (Status != WorkOrderTaskStatus.InProgress)
            return DomainResult.Failure($"Task '{TaskNumber}' must be InProgress to complete.");

        Status = WorkOrderTaskStatus.Completed;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }

    /// <summary>
    /// CRS sign-off by a certifying staff member.
    /// Hard Stop HS-009: task must be Completed before sign-off.
    /// </summary>
    internal DomainResult SignOff(Guid certifyingStaffUserId, string remark, Guid actorId)
    {
        if (IsSignedOff)
            return DomainResult.Failure($"Task '{TaskNumber}' is already signed off (HS-009).");

        if (Status != WorkOrderTaskStatus.Completed)
            return DomainResult.Failure(
                $"Hard Stop HS-009: Task '{TaskNumber}' must be Completed before CRS sign-off.");

        IsSignedOff = true;
        SignedOffByUserId = certifyingStaffUserId;
        SignedOffAt = DateTimeOffset.UtcNow;
        SignOffRemark = remark.Trim();
        Status = WorkOrderTaskStatus.SignedOff;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }

    internal DomainResult Cancel(Guid actorId)
    {
        if (Status == WorkOrderTaskStatus.SignedOff)
            return DomainResult.Failure($"Signed-off task '{TaskNumber}' cannot be cancelled.");

        Status = WorkOrderTaskStatus.Cancelled;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }

    // ── Labour ────────────────────────────────────────────────────────────────

    internal DomainResult AddLabourEntry(
        Guid performedByUserId,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        Guid actorId,
        string? notes = null)
    {
        if (Status is WorkOrderTaskStatus.SignedOff or WorkOrderTaskStatus.Cancelled)
            return DomainResult.Failure($"Cannot log labour against a '{Status}' task.");

        try
        {
            var entry = LabourEntry.Create(
                WorkOrderId, Id, performedByUserId, startAt, endAt, OrganisationId, actorId, notes);
            _labourEntries.Add(entry);
        }
        catch (ArgumentException ex)
        {
            return DomainResult.Failure(ex.Message);
        }

        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }

    // ── Parts ─────────────────────────────────────────────────────────────────

    internal RequiredPart AddRequiredPart(
        string partNumber,
        string description,
        decimal quantity,
        string unitOfMeasure,
        Guid actorId)
    {
        var part = RequiredPart.Create(
            WorkOrderId, Id, partNumber, description, quantity, unitOfMeasure, OrganisationId, actorId);
        _requiredParts.Add(part);
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return part;
    }

    internal DomainResult IssuePartFromStores(
        Guid partId,
        string issueSlipRef,
        decimal issuedQuantity,
        Guid issuedByUserId,
        Guid actorId)
    {
        var part = _requiredParts.FirstOrDefault(p => p.Id == partId);
        if (part is null)
            return DomainResult.Failure($"Required part {partId} not found on task '{TaskNumber}'.");

        try
        {
            part.RecordIssue(issueSlipRef, issuedQuantity, issuedByUserId, actorId);
        }
        catch (ArgumentException ex)
        {
            return DomainResult.Failure(ex.Message);
        }

        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }

    // ── Tools ─────────────────────────────────────────────────────────────────

    internal RequiredTool AddRequiredTool(
        string toolNumber,
        string description,
        Guid actorId,
        DateOnly? calibratedExpiry = null)
    {
        var tool = RequiredTool.Create(
            WorkOrderId, Id, toolNumber, description, OrganisationId, actorId, calibratedExpiry);
        _requiredTools.Add(tool);
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return tool;
    }

    /// <summary>
    /// Checks out a tool to a technician.
    /// Hard Stop HS-010: blocked if calibration is expired.
    /// </summary>
    internal DomainResult CheckOutTool(Guid toolId, Guid userId, Guid actorId)
    {
        var tool = _requiredTools.FirstOrDefault(t => t.Id == toolId);
        if (tool is null)
            return DomainResult.Failure($"Tool {toolId} not found on task '{TaskNumber}'.");

        return tool.CheckOut(userId, actorId);
    }

    internal DomainResult ReturnTool(Guid toolId, Guid actorId)
    {
        var tool = _requiredTools.FirstOrDefault(t => t.Id == toolId);
        if (tool is null)
            return DomainResult.Failure($"Tool {toolId} not found on task '{TaskNumber}'.");

        tool.Return(actorId);
        return DomainResult.Ok();
    }
}
