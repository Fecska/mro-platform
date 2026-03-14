namespace Mro.Domain.Common.Permissions;

/// <summary>
/// Canonical role name constants used throughout the system.
/// Source of truth for JWT claim values, RBAC checks, and UI display.
///
/// Roles are not hierarchical — permissions are additive and scoped.
/// A user can hold multiple roles simultaneously.
/// </summary>
public static class Roles
{
    // ── Maintenance execution ───────────────────────────────────────────────

    /// <summary>
    /// Holds a Part-66 licence. Can sign off completed maintenance tasks
    /// and issue Certificates of Release to Service (CRS).
    /// Scope: specific aircraft types + licence categories (A/B1/B2/C/D).
    /// </summary>
    public const string CertifyingStaff = "certifying_staff";

    /// <summary>
    /// Performs maintenance tasks. Cannot issue CRS.
    /// Scope: aircraft types + station(s).
    /// </summary>
    public const string Engineer = "engineer";

    // ── Quality & oversight ─────────────────────────────────────────────────

    /// <summary>
    /// Quality assurance / independent inspector.
    /// Can raise defects, sign off inspections, view all records.
    /// Cannot be the same person who performed the work (enforced by hard stop HS-003).
    /// </summary>
    public const string Inspector = "inspector";

    // ── Planning & production ───────────────────────────────────────────────

    /// <summary>
    /// Creates and manages work packages, assigns tasks, monitors readiness.
    /// No technical sign-off capability.
    /// </summary>
    public const string Planner = "planner";

    // ── Supply chain ────────────────────────────────────────────────────────

    /// <summary>
    /// Manages parts, consumables, and tools.
    /// Can issue/receive stock items, perform shelf-life checks.
    /// </summary>
    public const string StoreKeeper = "store_keeper";

    // ── Administration ──────────────────────────────────────────────────────

    /// <summary>
    /// Organisation-level administrator.
    /// Manages users, stations, and organisation settings within one org.
    /// Cannot access other organisations' data.
    /// </summary>
    public const string OrgAdmin = "org_admin";

    /// <summary>
    /// Platform-level system administrator.
    /// Manages organisations, has read access to all data for support.
    /// Cannot sign off maintenance work (hard stop HS-013).
    /// </summary>
    public const string SystemAdmin = "system_admin";

    /// <summary>All defined role names, for validation purposes.</summary>
    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        CertifyingStaff, Engineer, Inspector, Planner, StoreKeeper, OrgAdmin, SystemAdmin
    };
}
