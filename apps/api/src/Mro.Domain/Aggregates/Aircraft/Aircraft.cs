using Mro.Domain.Aggregates.Aircraft.Enums;
using Mro.Domain.Aggregates.Aircraft.Events;
using Mro.Domain.Application;
using Mro.Domain.Common.Audit;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Aircraft;

/// <summary>
/// Aggregate root representing a registered aircraft on the MRO operator's fleet.
///
/// Invariants enforced here:
///   - Registration is unique per organisation (enforced by unique DB index)
///   - Status transitions must follow the defined state machine
///   - Counter values may only increase (monotonic)
///   - A given installation position may hold only one active component at a time
///   - Written-off aircraft may not change status (terminal state)
/// </summary>
public sealed class Aircraft : AuditableEntity
{
    /// <summary>ICAO registration mark (tail number). e.g. "HA-LCB", "G-EUPT".</summary>
    public string Registration { get; private set; } = string.Empty;

    /// <summary>Manufacturer serial number (MSN / construction number).</summary>
    public string SerialNumber { get; private set; } = string.Empty;

    public Guid AircraftTypeId { get; private set; }

    /// <summary>Navigation property — loaded explicitly by the repository.</summary>
    public AircraftType? AircraftType { get; private set; }

    public AircraftStatus Status { get; private set; } = AircraftStatus.Active;

    public DateOnly ManufactureDate { get; private set; }

    /// <summary>Free-text operational remarks (e.g. current MEL items, special configs).</summary>
    public string? Remarks { get; private set; }

    private readonly List<AircraftCounter> _counters = [];
    private readonly List<AircraftStatusHistory> _statusHistory = [];
    private readonly List<InstalledComponent> _installedComponents = [];

    public IReadOnlyCollection<AircraftCounter> Counters => _counters.AsReadOnly();
    public IReadOnlyCollection<AircraftStatusHistory> StatusHistory => _statusHistory.AsReadOnly();
    public IReadOnlyCollection<InstalledComponent> InstalledComponents => _installedComponents.AsReadOnly();

    // EF Core
    private Aircraft() { }

    // ── Factory ──────────────────────────────────────────────────────────────

    public static Aircraft Register(
        string registration,
        string serialNumber,
        Guid aircraftTypeId,
        DateOnly manufactureDate,
        Guid organisationId,
        Guid actorId,
        string? remarks = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(registration);
        ArgumentException.ThrowIfNullOrWhiteSpace(serialNumber);

        var aircraft = new Aircraft
        {
            Registration = registration.Trim().ToUpperInvariant(),
            SerialNumber = serialNumber.Trim().ToUpperInvariant(),
            AircraftTypeId = aircraftTypeId,
            ManufactureDate = manufactureDate,
            Remarks = remarks?.Trim(),
            Status = AircraftStatus.Active,
            OrganisationId = organisationId,
            CreatedBy = actorId,
            UpdatedBy = actorId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        aircraft.RaiseDomainEvent(new AircraftRegisteredEvent
        {
            ActorId = actorId,
            OrganisationId = organisationId,
            EntityType = nameof(Aircraft),
            EntityId = aircraft.Id,
            EventType = ComplianceEventType.RecordCreated,
            Registration = aircraft.Registration,
            SerialNumber = aircraft.SerialNumber,
            AircraftTypeId = aircraftTypeId,
            Description = $"Aircraft {aircraft.Registration} (MSN {aircraft.SerialNumber}) registered.",
        });

        return aircraft;
    }

    // ── Status machine ───────────────────────────────────────────────────────

    private static readonly IReadOnlyDictionary<AircraftStatus, IReadOnlySet<AircraftStatus>> AllowedTransitions =
        new Dictionary<AircraftStatus, IReadOnlySet<AircraftStatus>>
        {
            [AircraftStatus.Active]        = new HashSet<AircraftStatus> { AircraftStatus.Grounded, AircraftStatus.InMaintenance, AircraftStatus.Withdrawn, AircraftStatus.WrittenOff },
            [AircraftStatus.Grounded]      = new HashSet<AircraftStatus> { AircraftStatus.Active, AircraftStatus.InMaintenance, AircraftStatus.Withdrawn },
            [AircraftStatus.InMaintenance] = new HashSet<AircraftStatus> { AircraftStatus.Active, AircraftStatus.Grounded, AircraftStatus.Withdrawn },
            [AircraftStatus.Withdrawn]     = new HashSet<AircraftStatus> { AircraftStatus.Active, AircraftStatus.WrittenOff },
            [AircraftStatus.WrittenOff]    = new HashSet<AircraftStatus>(),
        };

    /// <summary>
    /// Transitions the aircraft to a new operational status.
    /// Returns a domain error if the transition is not permitted.
    /// </summary>
    public DomainResult ChangeStatus(AircraftStatus newStatus, string reason, Guid actorId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (!AllowedTransitions[Status].Contains(newStatus))
            return DomainResult.Failure(
                $"Transition from '{Status}' to '{newStatus}' is not permitted.");

        var previousStatus = Status;
        Status = newStatus;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;

        _statusHistory.Add(AircraftStatusHistory.Create(
            Id, previousStatus, newStatus, reason, actorId, OrganisationId));

        RaiseDomainEvent(new AircraftStatusChangedEvent
        {
            ActorId = actorId,
            OrganisationId = OrganisationId,
            EntityType = nameof(Aircraft),
            EntityId = Id,
            EventType = ComplianceEventType.RecordUpdated,
            FromStatus = previousStatus,
            ToStatus = newStatus,
            Reason = reason,
            Description = $"Aircraft {Registration} status: {previousStatus} → {newStatus}. Reason: {reason}",
        });

        return DomainResult.Ok();
    }

    // ── Counters ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Updates or adds a utilisation counter.
    /// Counter values may not decrease (monotonic invariant).
    /// </summary>
    public DomainResult UpdateCounter(CounterType type, decimal newValue, Guid actorId)
    {
        var counter = _counters.FirstOrDefault(c => c.CounterType == type);

        if (counter is null)
        {
            _counters.Add(AircraftCounter.Create(Id, type, newValue, OrganisationId, actorId));
            return DomainResult.Ok();
        }

        if (!counter.TrySetValue(newValue, actorId))
            return DomainResult.Failure(
                $"Counter '{type}' may not decrease. Current value: {counter.Value}, proposed: {newValue}.");

        return DomainResult.Ok();
    }

    // ── Installed components ─────────────────────────────────────────────────

    /// <summary>
    /// Records a new component installation.
    /// Fails if another component is already active in the same position.
    /// </summary>
    public DomainResult InstallComponent(
        string partNumber,
        string serialNumber,
        string description,
        string position,
        Guid installedByUserId,
        Guid actorId,
        Guid? workOrderId = null,
        Guid? inventoryItemId = null)
    {
        var occupying = _installedComponents
            .FirstOrDefault(c => c.InstallationPosition == position.Trim().ToUpperInvariant()
                              && c.IsInstalled);

        if (occupying is not null)
            return DomainResult.Failure(
                $"Position '{position}' already occupied by {occupying.PartNumber} S/N {occupying.SerialNumber}.");

        var component = InstalledComponent.Create(
            Id, partNumber, serialNumber, description, position,
            installedByUserId, OrganisationId, actorId, workOrderId, inventoryItemId);

        _installedComponents.Add(component);

        RaiseDomainEvent(new ComponentInstalledEvent
        {
            ActorId = actorId,
            OrganisationId = OrganisationId,
            EntityType = nameof(InstalledComponent),
            EntityId = component.Id,
            EventType = ComplianceEventType.RecordCreated,
            PartNumber = component.PartNumber,
            SerialNumber = component.SerialNumber,
            InstallationPosition = component.InstallationPosition,
            InstalledComponentId = component.Id,
            Description = $"Component {component.PartNumber}/{component.SerialNumber} installed at {component.InstallationPosition} on {Registration}.",
        });

        return DomainResult.Ok();
    }

    /// <summary>
    /// Removes a component from an installation position.
    /// </summary>
    public DomainResult RemoveComponent(
        Guid componentId,
        string reason,
        Guid removedByUserId,
        Guid actorId,
        Guid? workOrderId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        var component = _installedComponents.FirstOrDefault(c => c.Id == componentId && c.IsInstalled);
        if (component is null)
            return DomainResult.Failure("Component not found or already removed.");

        component.Remove(removedByUserId, reason, actorId, workOrderId);

        RaiseDomainEvent(new ComponentRemovedEvent
        {
            ActorId = actorId,
            OrganisationId = OrganisationId,
            EntityType = nameof(InstalledComponent),
            EntityId = component.Id,
            EventType = ComplianceEventType.RecordUpdated,
            PartNumber = component.PartNumber,
            SerialNumber = component.SerialNumber,
            InstallationPosition = component.InstallationPosition,
            Reason = reason,
            Description = $"Component {component.PartNumber}/{component.SerialNumber} removed from {component.InstallationPosition} on {Registration}. Reason: {reason}",
        });

        return DomainResult.Ok();
    }

    public void UpdateRemarks(string? remarks, Guid actorId)
    {
        Remarks = remarks?.Trim();
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
