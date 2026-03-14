namespace Mro.Domain.Common.Audit;

/// <summary>
/// Categorises compliance-relevant events recorded in the immutable audit log.
///
/// These types are stored as strings in the audit_logs table (not integers) so
/// the schema remains readable without a lookup table and historical records
/// are not affected by future enum reordering.
///
/// Reference: docs/compliance/requirements-traceability.md (SEC category)
/// </summary>
public static class ComplianceEventType
{
    // ── Authentication ──────────────────────────────────────────────────────
    public const string LoginSuccess          = "auth.login_success";
    public const string LoginFailed           = "auth.login_failed";
    public const string LoginBlocked          = "auth.login_blocked";
    public const string Logout                = "auth.logout";
    public const string TokenRefreshed        = "auth.token_refreshed";

    // ── Data changes ────────────────────────────────────────────────────────
    public const string RecordCreated         = "data.created";
    public const string RecordUpdated         = "data.updated";
    public const string RecordDeleted         = "data.soft_deleted";

    // ── Maintenance lifecycle ────────────────────────────────────────────────
    public const string WorkOrderOpened       = "maint.work_order_opened";
    public const string WorkOrderClosed       = "maint.work_order_closed";
    public const string WorkOrderCancelled    = "maint.work_order_cancelled";
    public const string TaskSigned            = "maint.task_signed";

    // ── Defect lifecycle ────────────────────────────────────────────────────
    public const string DefectRaised          = "defect.raised";
    public const string DefectTriaged         = "defect.triaged";
    public const string DefectDeferred        = "defect.deferred";
    public const string DefectCleared         = "defect.cleared";

    // ── Release (CRS) lifecycle ─────────────────────────────────────────────
    public const string ReleaseInitiated      = "release.initiated";
    public const string ReleaseSigned         = "release.signed";
    public const string ReleaseRevoked        = "release.revoked";
    public const string ReleaseSuperseded     = "release.superseded";

    // ── Hard stops ──────────────────────────────────────────────────────────
    /// <summary>Logged every time a hard stop rule blocks an operation.</summary>
    public const string HardStopTriggered     = "compliance.hard_stop_triggered";

    // ── Document access ─────────────────────────────────────────────────────
    public const string DocumentAccessed      = "document.accessed";
    public const string DocumentDownloaded    = "document.downloaded";

    // ── Personnel & qualification ────────────────────────────────────────────
    public const string LicenceAdded          = "personnel.licence_added";
    public const string LicenceExpired        = "personnel.licence_expired";
    public const string AuthorisationGranted  = "personnel.authorisation_granted";
    public const string AuthorisationRevoked  = "personnel.authorisation_revoked";
}
