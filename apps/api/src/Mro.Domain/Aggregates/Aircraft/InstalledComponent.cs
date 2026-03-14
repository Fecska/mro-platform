using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Aircraft;

/// <summary>
/// A life-tracked component installed at a specific position on an aircraft.
/// Provides traceability from component → aircraft position → work order.
///
/// Installation is recorded by Aircraft.InstallComponent().
/// Removal is recorded by Aircraft.RemoveComponent() which sets RemovedAt/By.
///
/// A position can hold only one active component at a time — enforced by
/// the Aircraft aggregate before calling InstallComponent.
/// </summary>
public sealed class InstalledComponent : AuditableEntity
{
    public Guid AircraftId { get; private set; }

    /// <summary>Part number as printed on the component (ATA 100 / CAGE code).</summary>
    public string PartNumber { get; private set; } = string.Empty;

    /// <summary>Manufacturer serial number stamped on the component.</summary>
    public string SerialNumber { get; private set; } = string.Empty;

    /// <summary>Human-readable description (e.g. "Main Landing Gear Shock Absorber").</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// ATA-style installation position code (e.g. "32-10-LH", "72-00-ENG1").
    /// Used to enforce one-component-per-position invariant.
    /// </summary>
    public string InstallationPosition { get; private set; } = string.Empty;

    public DateTimeOffset InstalledAt { get; private set; }
    public Guid InstalledByUserId { get; private set; }

    /// <summary>FK to the work order under which this component was installed (optional).</summary>
    public Guid? InstallationWorkOrderId { get; private set; }

    /// <summary>Null while the component is still installed.</summary>
    public DateTimeOffset? RemovedAt { get; private set; }
    public Guid? RemovedByUserId { get; private set; }
    public Guid? RemovalWorkOrderId { get; private set; }
    public string? RemovalReason { get; private set; }

    /// <summary>Reference to the stock/inventory item from which this was issued (optional).</summary>
    public Guid? InventoryItemId { get; private set; }

    public bool IsInstalled => RemovedAt is null;

    // EF Core
    private InstalledComponent() { }

    internal static InstalledComponent Create(
        Guid aircraftId,
        string partNumber,
        string serialNumber,
        string description,
        string installationPosition,
        Guid installedByUserId,
        Guid organisationId,
        Guid actorId,
        Guid? workOrderId = null,
        Guid? inventoryItemId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(partNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(serialNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(installationPosition);

        return new InstalledComponent
        {
            AircraftId = aircraftId,
            PartNumber = partNumber.Trim().ToUpperInvariant(),
            SerialNumber = serialNumber.Trim().ToUpperInvariant(),
            Description = description.Trim(),
            InstallationPosition = installationPosition.Trim().ToUpperInvariant(),
            InstalledAt = DateTimeOffset.UtcNow,
            InstalledByUserId = installedByUserId,
            InstallationWorkOrderId = workOrderId,
            InventoryItemId = inventoryItemId,
            OrganisationId = organisationId,
            CreatedBy = actorId,
            UpdatedBy = actorId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    internal void Remove(Guid removedByUserId, string reason, Guid actorId, Guid? workOrderId = null)
    {
        RemovedAt = DateTimeOffset.UtcNow;
        RemovedByUserId = removedByUserId;
        RemovalReason = reason;
        RemovalWorkOrderId = workOrderId;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
