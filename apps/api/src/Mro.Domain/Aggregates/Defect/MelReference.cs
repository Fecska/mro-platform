using Mro.Domain.Aggregates.Defect.Enums;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Defect;

/// <summary>
/// Captures which MEL / CDL item authorises deferral of a defect.
///
/// Invariants:
///   - Item number must be provided (e.g. "27-51-1").
///   - Revision of the MEL document used at time of deferral must be recorded
///     for traceability (regulatory requirement).
///   - For Cat A items the specific interval is set by the operator and stored here;
///     for B/C/D the calendar interval is implied by DeferralCategory.
/// </summary>
public sealed class MelReference : AuditableEntity
{
    public Guid DeferredDefectId { get; private set; }

    /// <summary>MEL or CDL item number (e.g. "27-51-1", "CDL-57-1").</summary>
    public string ItemNumber { get; private set; } = string.Empty;

    /// <summary>Revision / Amendment number of the MEL/CDL in effect at time of deferral.</summary>
    public string MelRevision { get; private set; } = string.Empty;

    public DeferralCategory Category { get; private set; }

    /// <summary>
    /// For Cat A / Engineering Order items: operator-specified maximum interval in days.
    /// Null for B/C/D categories whose intervals are fixed by regulation.
    /// </summary>
    public int? OperatorIntervalDays { get; private set; }

    /// <summary>
    /// Operational limitations or crew procedures required while deferred
    /// (extracted from MEL remarks section).
    /// </summary>
    public string? OperationalLimitations { get; private set; }

    /// <summary>
    /// Maintenance procedures required while deferred
    /// (extracted from MEL maintenance section).
    /// </summary>
    public string? MaintenanceProcedures { get; private set; }

    // EF Core
    private MelReference() { }

    internal static MelReference Create(
        Guid deferredDefectId,
        string itemNumber,
        string melRevision,
        DeferralCategory category,
        Guid organisationId,
        Guid actorId,
        int? operatorIntervalDays = null,
        string? operationalLimitations = null,
        string? maintenanceProcedures = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(melRevision);

        if (category == DeferralCategory.MelA && operatorIntervalDays is null)
            throw new ArgumentException(
                "Cat A items require an operator-specified interval in days.", nameof(operatorIntervalDays));

        return new MelReference
        {
            DeferredDefectId = deferredDefectId,
            ItemNumber = itemNumber.Trim().ToUpperInvariant(),
            MelRevision = melRevision.Trim(),
            Category = category,
            OperatorIntervalDays = operatorIntervalDays,
            OperationalLimitations = operationalLimitations?.Trim(),
            MaintenanceProcedures = maintenanceProcedures?.Trim(),
            OrganisationId = organisationId,
            CreatedBy = actorId,
            UpdatedBy = actorId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Returns the maximum deferral period in days for this item,
    /// based on category or operator-specified interval.
    /// </summary>
    public int MaxDeferralDays => Category switch
    {
        DeferralCategory.MelB => 3,
        DeferralCategory.MelC => 10,
        DeferralCategory.MelD => 120,
        DeferralCategory.MelA or DeferralCategory.Cdl or DeferralCategory.EngineeringOrder
            => OperatorIntervalDays
               ?? throw new InvalidOperationException(
                   $"OperatorIntervalDays must be set for category '{Category}'."),
        _ => throw new InvalidOperationException($"Unknown deferral category '{Category}'."),
    };
}
