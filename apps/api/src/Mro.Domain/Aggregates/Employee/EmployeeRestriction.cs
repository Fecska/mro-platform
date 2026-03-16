using Mro.Domain.Aggregates.Employee.Enums;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Employee;

/// <summary>
/// An operational restriction placed on an employee.
///
/// Examples: TemporarySuspension, SupervisedWorkOnly, StationRestricted, NoReleasePrivilege.
///
/// A restriction is active when: not soft-deleted AND ActiveFrom &lt;= today AND (ActiveUntil is null OR ActiveUntil &gt;= today).
/// Lifting a restriction sets ActiveUntil to yesterday (preserves audit trail instead of hard-deleting).
/// </summary>
public sealed class EmployeeRestriction : AuditableEntity
{
    public Guid EmployeeId { get; private set; }

    public RestrictionType RestrictionType { get; private set; }

    /// <summary>Free-text detail or justification (e.g. "Pending medical clearance").</summary>
    public string? Details { get; private set; }

    /// <summary>Relevant station for StationRestricted type; null for all other types.</summary>
    public Guid? StationId { get; private set; }

    /// <summary>User ID of the manager/authority who imposed the restriction.</summary>
    public Guid RaisedByUserId { get; private set; }

    public DateOnly ActiveFrom { get; private set; }

    /// <summary>Null means indefinite (until explicitly lifted).</summary>
    public DateOnly? ActiveUntil { get; private set; }

    public bool IsActive =>
        !IsDeleted
        && ActiveFrom <= DateOnly.FromDateTime(DateTime.UtcNow)
        && (ActiveUntil is null || ActiveUntil >= DateOnly.FromDateTime(DateTime.UtcNow));

    // EF Core
    private EmployeeRestriction() { }

    internal static EmployeeRestriction Create(
        Guid employeeId,
        RestrictionType restrictionType,
        Guid raisedByUserId,
        DateOnly activeFrom,
        Guid organisationId,
        Guid actorId,
        string? details = null,
        Guid? stationId = null,
        DateOnly? activeUntil = null)
    {
        return new EmployeeRestriction
        {
            EmployeeId       = employeeId,
            RestrictionType  = restrictionType,
            Details          = details?.Trim(),
            StationId        = stationId,
            RaisedByUserId   = raisedByUserId,
            ActiveFrom       = activeFrom,
            ActiveUntil      = activeUntil,
            OrganisationId   = organisationId,
            CreatedBy        = actorId,
            UpdatedBy        = actorId,
            CreatedAt        = DateTimeOffset.UtcNow,
            UpdatedAt        = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Lifts the restriction by setting ActiveUntil to yesterday.
    /// Preserves audit trail — does not soft-delete the record.
    /// </summary>
    internal void Lift(Guid actorId)
    {
        ActiveUntil = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
        UpdatedBy   = actorId;
        UpdatedAt   = DateTimeOffset.UtcNow;
    }
}
