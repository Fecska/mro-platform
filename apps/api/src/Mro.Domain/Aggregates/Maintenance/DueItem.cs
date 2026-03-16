using Mro.Domain.Aggregates.Maintenance.Enums;
using Mro.Domain.Aggregates.Maintenance.Events;
using Mro.Domain.Application;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Maintenance;

/// <summary>
/// Tracks the due status of a single recurring maintenance requirement for a specific aircraft.
///
/// Invariants:
///   - A NotApplicable item cannot be accomplished.
///   - A OneTime item transitions to Accomplished permanently after first accomplishment.
///   - Deferral requires justification and may not exceed the regulatory maximum.
///   - DueStatus is updated on every accomplishment or deferral.
/// </summary>
public sealed class DueItem : AuditableEntity
{
    /// <summary>Task card number, AD number, or SB reference (e.g. "A-CHK-01-001", "AD 2024-0001").</summary>
    public string DueItemRef { get; private set; } = null!;

    public Guid AircraftId { get; private set; }

    /// <summary>Optional link to the governing AMP document.</summary>
    public Guid? MaintenanceProgramId { get; private set; }

    public DueItemType DueItemType { get; private set; }
    public IntervalType IntervalType { get; private set; }

    public string Description { get; private set; } = null!;

    /// <summary>Regulatory or document reference (e.g. "AMM 5-20-00, EASA AD 2024-001").</summary>
    public string? RegulatoryRef { get; private set; }

    // ── Interval ────────────────────────────────────────────────────────────

    /// <summary>Primary interval value (hours or days depending on IntervalType).</summary>
    public decimal? IntervalValue { get; private set; }

    /// <summary>Interval in calendar days (used when IntervalType includes CalendarDays).</summary>
    public int? IntervalDays { get; private set; }

    /// <summary>Tolerance window — task may be performed this many days/hours early or late.</summary>
    public decimal? ToleranceValue { get; private set; }

    // ── Last accomplishment ─────────────────────────────────────────────────

    public DateTimeOffset? LastAccomplishedAt { get; private set; }
    public decimal? LastAccomplishedAtHours { get; private set; }
    public int? LastAccomplishedAtCycles { get; private set; }

    /// <summary>Work order that accomplished this item.</summary>
    public Guid? LastAccomplishedWorkOrderId { get; private set; }

    // ── Next due ─────────────────────────────────────────────────────────────

    public DateTimeOffset? NextDueDate { get; private set; }
    public decimal? NextDueHours { get; private set; }
    public int? NextDueCycles { get; private set; }

    public DueStatus Status { get; private set; } = DueStatus.Current;

    public bool IsRecurring => IntervalType != IntervalType.OneTime;

    private DueItem() { }

    public static DueItem Create(
        string dueItemRef,
        Guid aircraftId,
        DueItemType dueItemType,
        IntervalType intervalType,
        string description,
        Guid organisationId,
        Guid actorId,
        Guid? maintenanceProgramId = null,
        string? regulatoryRef = null,
        decimal? intervalValue = null,
        int? intervalDays = null,
        decimal? toleranceValue = null,
        DateTimeOffset? nextDueDate = null,
        decimal? nextDueHours = null,
        int? nextDueCycles = null) => new()
    {
        DueItemRef           = dueItemRef.ToUpperInvariant(),
        AircraftId           = aircraftId,
        MaintenanceProgramId = maintenanceProgramId,
        DueItemType          = dueItemType,
        IntervalType         = intervalType,
        Description          = description,
        RegulatoryRef        = regulatoryRef,
        IntervalValue        = intervalValue,
        IntervalDays         = intervalDays,
        ToleranceValue       = toleranceValue,
        NextDueDate          = nextDueDate,
        NextDueHours         = nextDueHours,
        NextDueCycles        = nextDueCycles,
        OrganisationId       = organisationId,
        CreatedAt            = DateTimeOffset.UtcNow,
        CreatedBy            = actorId,
    };

    // ── Record accomplishment ───────────────────────────────────────────────

    public DomainResult RecordAccomplishment(
        DateTimeOffset accomplishedAt,
        Guid workOrderId,
        Guid actorId,
        decimal? atHours = null,
        int? atCycles = null)
    {
        if (Status == DueStatus.NotApplicable)
            return DomainResult.Failure("Cannot accomplish a N/A due item.");

        LastAccomplishedAt             = accomplishedAt;
        LastAccomplishedAtHours        = atHours;
        LastAccomplishedAtCycles       = atCycles;
        LastAccomplishedWorkOrderId    = workOrderId;

        // Compute next due from accomplishment date + interval
        if (IsRecurring)
        {
            NextDueDate   = IntervalDays.HasValue
                ? accomplishedAt.AddDays(IntervalDays.Value)
                : (DateTimeOffset?)null;
            NextDueHours  = (atHours.HasValue && IntervalValue.HasValue)
                ? atHours.Value + IntervalValue.Value
                : (decimal?)null;
            NextDueCycles = (atCycles.HasValue && IntervalValue.HasValue)
                ? atCycles.Value + (int)IntervalValue.Value
                : (int?)null;
            Status = DueStatus.Current;
        }
        else
        {
            NextDueDate   = null;
            NextDueHours  = null;
            NextDueCycles = null;
            Status        = DueStatus.Accomplished;
        }

        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = actorId;

        RaiseDomainEvent(new DueItemAccomplishedEvent
        {
            ActorId        = actorId,
            OrganisationId = OrganisationId,
            EntityType     = "DueItem",
            EntityId       = Id,
            EventType      = "DUE_ITEM_ACCOMPLISHED",
            Description    = $"Due item {DueItemRef} accomplished on {accomplishedAt:yyyy-MM-dd}. Next due: {NextDueDate?.ToString("yyyy-MM-dd") ?? "N/A"}.",
            DueItemRef     = DueItemRef,
            AircraftId     = AircraftId,
            AccomplishedAt = accomplishedAt,
            NextDueDate    = NextDueDate,
        });

        return DomainResult.Ok();
    }

    // ── Defer ───────────────────────────────────────────────────────────────

    public DomainResult Defer(DateTimeOffset newDueDate, string justification, Guid actorId)
    {
        if (Status == DueStatus.NotApplicable)
            return DomainResult.Failure("Cannot defer a N/A due item.");
        if (Status == DueStatus.Accomplished)
            return DomainResult.Failure("Cannot defer an already accomplished one-time item.");
        if (string.IsNullOrWhiteSpace(justification))
            return DomainResult.Failure("Deferral justification is required.");

        NextDueDate = newDueDate;
        Status      = DueStatus.Current;
        UpdatedAt   = DateTimeOffset.UtcNow;
        UpdatedBy   = actorId;
        return DomainResult.Ok();
    }

    // ── Mark N/A ────────────────────────────────────────────────────────────

    public DomainResult MarkNotApplicable(string reason, Guid actorId)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return DomainResult.Failure("Reason is required to mark a due item as N/A.");
        Status    = DueStatus.NotApplicable;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = actorId;
        return DomainResult.Ok();
    }

    // ── Refresh status ──────────────────────────────────────────────────────

    /// <summary>
    /// Recomputes <see cref="Status"/> relative to the current date.
    /// Call from a background job or on read if staleness is a concern.
    /// Warning threshold: 30 calendar days.
    /// </summary>
    public void RefreshStatus(DateTimeOffset now, int dueSoonDays = 30)
    {
        if (Status is DueStatus.NotApplicable or DueStatus.Accomplished)
            return;

        if (NextDueDate is null)
        {
            Status = DueStatus.Current;
            return;
        }

        var daysRemaining = (NextDueDate.Value - now).TotalDays;
        Status = daysRemaining < 0      ? DueStatus.Overdue
               : daysRemaining <= dueSoonDays ? DueStatus.DueSoon
               : DueStatus.Current;
    }
}
