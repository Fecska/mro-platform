namespace Mro.Domain.Common.Permissions;

/// <summary>
/// Defines the operational boundaries within which a user may act.
/// Scope is orthogonal to role: a user's role determines *what* they can do;
/// their scope determines *where* (stations) and *on what* (aircraft types,
/// licence categories) they may do it.
///
/// Empty collections mean "no restriction" (all items in the organisation).
/// This matches Part-145 practice where a senior AME may not be station-restricted,
/// while a contract engineer may only work at one station on one aircraft type.
/// </summary>
public sealed record OperationalScope
{
    /// <summary>
    /// Station IDs the user may access.
    /// Empty = access to all stations within the organisation.
    /// </summary>
    public IReadOnlyList<Guid> StationIds { get; init; } = [];

    /// <summary>
    /// Aircraft type designators (ICAO, e.g. "B738", "A320", "CRJ9") the user
    /// is authorised to work on.
    /// Empty = all aircraft types.
    /// </summary>
    public IReadOnlyList<string> AircraftTypes { get; init; } = [];

    /// <summary>
    /// Part-66 licence categories the certifying staff member holds.
    /// Only relevant for the CertifyingStaff role.
    /// Standard values: A, B1, B2, C, D
    /// Sub-categories are stored as strings (e.g. "B1.1", "B2.2").
    /// Empty = no release authority.
    /// </summary>
    public IReadOnlyList<string> ReleaseCategories { get; init; } = [];

    /// <summary>Unrestricted scope — use for org_admin / system_admin.</summary>
    public static readonly OperationalScope Unrestricted = new();

    /// <summary>
    /// Returns true if this scope grants access to the given station.
    /// </summary>
    public bool IncludesStation(Guid stationId) =>
        StationIds.Count == 0 || StationIds.Contains(stationId);

    /// <summary>
    /// Returns true if this scope grants work authority on the given aircraft type.
    /// </summary>
    public bool IncludesAircraftType(string icaoType) =>
        AircraftTypes.Count == 0 || AircraftTypes.Contains(icaoType, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Returns true if the certifying staff member holds the specified licence category.
    /// </summary>
    public bool HoldsReleaseCategory(string category) =>
        ReleaseCategories.Contains(category, StringComparer.OrdinalIgnoreCase);
}
