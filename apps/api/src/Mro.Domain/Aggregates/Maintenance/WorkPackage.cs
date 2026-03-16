using Mro.Domain.Aggregates.Maintenance.Enums;
using Mro.Domain.Aggregates.Maintenance.Events;
using Mro.Domain.Application;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Maintenance;

/// <summary>
/// A work package groups related maintenance items for a single maintenance event
/// (e.g. an A-check or scheduled component change).
///
/// Invariants:
///   - Items can only be added in Draft or Released status.
///   - Package cannot Complete unless every item is Accomplished, Deferred, or N/A.
///   - Transitions are one-directional (no going back) except Cancelled from any active state.
///
/// State machine:
///   Draft → Released → InProgress → Completed → Closed
///   Draft | Released | InProgress | Completed → Cancelled
/// </summary>
public sealed class WorkPackage : AuditableEntity
{
    private static readonly IReadOnlyDictionary<WorkPackageStatus, IReadOnlySet<WorkPackageStatus>>
        AllowedTransitions = new Dictionary<WorkPackageStatus, IReadOnlySet<WorkPackageStatus>>
        {
            [WorkPackageStatus.Draft]      = new HashSet<WorkPackageStatus> { WorkPackageStatus.Released,   WorkPackageStatus.Cancelled },
            [WorkPackageStatus.Released]   = new HashSet<WorkPackageStatus> { WorkPackageStatus.InProgress, WorkPackageStatus.Cancelled },
            [WorkPackageStatus.InProgress] = new HashSet<WorkPackageStatus> { WorkPackageStatus.Completed,  WorkPackageStatus.Cancelled },
            [WorkPackageStatus.Completed]  = new HashSet<WorkPackageStatus> { WorkPackageStatus.Closed,     WorkPackageStatus.Cancelled },
            [WorkPackageStatus.Closed]     = new HashSet<WorkPackageStatus>(),
            [WorkPackageStatus.Cancelled]  = new HashSet<WorkPackageStatus>(),
        };

    private readonly List<PackageItem> _items = [];

    public string PackageNumber { get; private set; } = null!;
    public Guid AircraftId { get; private set; }
    public string Description { get; private set; } = null!;
    public WorkPackageStatus Status { get; private set; } = WorkPackageStatus.Draft;
    public DateOnly PlannedStartDate { get; private set; }
    public DateOnly? PlannedEndDate { get; private set; }
    public DateTimeOffset? ActualStartDate { get; private set; }
    public DateTimeOffset? ActualEndDate { get; private set; }
    public Guid? StationId { get; private set; }
    public Guid? RelatedWorkOrderId { get; private set; }

    public IReadOnlyList<PackageItem> Items => _items.AsReadOnly();

    public bool AllItemsResolved =>
        _items.Count > 0 &&
        _items.All(i => i.Status != PackageItemStatus.Pending);

    private WorkPackage() { }

    public static WorkPackage Create(
        string packageNumber,
        Guid aircraftId,
        string description,
        DateOnly plannedStartDate,
        Guid organisationId,
        Guid actorId,
        DateOnly? plannedEndDate = null,
        Guid? stationId = null,
        Guid? relatedWorkOrderId = null)
    {
        var wp = new WorkPackage
        {
            PackageNumber        = packageNumber,
            AircraftId           = aircraftId,
            Description          = description,
            PlannedStartDate     = plannedStartDate,
            PlannedEndDate       = plannedEndDate,
            StationId            = stationId,
            RelatedWorkOrderId   = relatedWorkOrderId,
            OrganisationId       = organisationId,
            CreatedAt            = DateTimeOffset.UtcNow,
            CreatedBy            = actorId,
        };

        wp.RaiseDomainEvent(new WorkPackageCreatedEvent
        {
            ActorId        = actorId,
            OrganisationId = organisationId,
            EntityType     = "WorkPackage",
            EntityId       = wp.Id,
            EventType      = "WORK_PACKAGE_CREATED",
            Description    = $"Work package {packageNumber} created for aircraft {aircraftId}.",
            PackageNumber  = packageNumber,
            AircraftId     = aircraftId,
        });

        return wp;
    }

    // ── Status transitions ─────────────────────────────────────────────────

    private DomainResult Transition(WorkPackageStatus to, Guid actorId)
    {
        if (!AllowedTransitions[Status].Contains(to))
            return DomainResult.Failure($"Cannot transition from {Status} to {to}.");

        var old = Status;
        Status    = to;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = actorId;

        RaiseDomainEvent(new WorkPackageStatusChangedEvent
        {
            ActorId        = actorId,
            OrganisationId = OrganisationId,
            EntityType     = "WorkPackage",
            EntityId       = Id,
            EventType      = "WORK_PACKAGE_STATUS_CHANGED",
            Description    = $"Work package {PackageNumber} transitioned from {old} to {to}.",
            PackageNumber  = PackageNumber,
            OldStatus      = old,
            NewStatus      = to,
        });

        return DomainResult.Ok();
    }

    public DomainResult Release(Guid actorId)   => Transition(WorkPackageStatus.Released,   actorId);
    public DomainResult Start(Guid actorId)
    {
        var r = Transition(WorkPackageStatus.InProgress, actorId);
        if (r.IsSuccess) ActualStartDate = DateTimeOffset.UtcNow;
        return r;
    }
    public DomainResult Complete(Guid actorId)
    {
        if (!AllItemsResolved)
            return DomainResult.Failure("All items must be accomplished, deferred, or marked N/A before completing the package.");
        var r = Transition(WorkPackageStatus.Completed, actorId);
        if (r.IsSuccess) ActualEndDate = DateTimeOffset.UtcNow;
        return r;
    }
    public DomainResult Close(Guid actorId)    => Transition(WorkPackageStatus.Closed,     actorId);
    public DomainResult Cancel(Guid actorId)   => Transition(WorkPackageStatus.Cancelled,  actorId);

    // ── Item management ────────────────────────────────────────────────────

    public DomainResult AddItem(
        string description,
        Guid actorId,
        Guid? dueItemId = null,
        string? taskReference = null,
        decimal? estimatedManHours = null)
    {
        if (Status is not (WorkPackageStatus.Draft or WorkPackageStatus.Released))
            return DomainResult.Failure("Items can only be added to Draft or Released packages.");

        var item = PackageItem.Create(Id, description, OrganisationId, actorId,
            dueItemId, taskReference, estimatedManHours);
        _items.Add(item);
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = actorId;
        return DomainResult.Ok();
    }

    public DomainResult AccomplishItem(Guid itemId, Guid workOrderId, Guid actorId, decimal? actualHours = null)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null) return DomainResult.Failure("Package item not found.");
        return item.Accomplish(workOrderId, actualHours, actorId);
    }

    public DomainResult DeferItem(Guid itemId, string reason, Guid actorId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null) return DomainResult.Failure("Package item not found.");
        return item.Defer(reason, actorId);
    }

    public DomainResult SetItemNotApplicable(Guid itemId, string reason, Guid actorId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null) return DomainResult.Failure("Package item not found.");
        return item.MarkNotApplicable(reason, actorId);
    }
}
