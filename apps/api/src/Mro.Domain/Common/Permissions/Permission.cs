namespace Mro.Domain.Common.Permissions;

/// <summary>
/// Value object representing a single permission code.
/// Permissions are checked at the Application layer before executing commands.
/// Format: "{module}:{action}" — e.g. "workorder:create", "defect:clear".
///
/// The Permission struct is immutable and comparable; use the static members
/// as the canonical definitions rather than constructing raw strings.
/// </summary>
public readonly record struct Permission(string Code)
{
    // ── Work Orders ─────────────────────────────────────────────────────────
    public static readonly Permission WorkOrderCreate         = new("workorder:create");
    public static readonly Permission WorkOrderUpdate         = new("workorder:update");
    public static readonly Permission WorkOrderClose          = new("workorder:close");
    public static readonly Permission WorkOrderCancel         = new("workorder:cancel");

    // ── Defects ─────────────────────────────────────────────────────────────
    public static readonly Permission DefectRaise             = new("defect:raise");
    public static readonly Permission DefectTriage            = new("defect:triage");
    public static readonly Permission DefectDefer             = new("defect:defer");
    public static readonly Permission DefectClear             = new("defect:clear");

    // ── Releases (CRS) ──────────────────────────────────────────────────────
    public static readonly Permission ReleaseInitiate         = new("release:initiate");
    public static readonly Permission ReleaseSign             = new("release:sign");
    public static readonly Permission ReleaseRevoke           = new("release:revoke");

    // ── Inventory ───────────────────────────────────────────────────────────
    public static readonly Permission InventoryIssue          = new("inventory:issue");
    public static readonly Permission InventoryReceive        = new("inventory:receive");
    public static readonly Permission InventoryAdjust         = new("inventory:adjust");

    // ── Aircraft ────────────────────────────────────────────────────────────
    public static readonly Permission AircraftView            = new("aircraft:view");
    public static readonly Permission AircraftManage          = new("aircraft:manage");

    // ── Personnel ───────────────────────────────────────────────────────────
    public static readonly Permission PersonnelView           = new("personnel:view");
    public static readonly Permission PersonnelManage         = new("personnel:manage");

    // ── Organisation administration ─────────────────────────────────────────
    public static readonly Permission OrgManage               = new("org:manage");
    public static readonly Permission OrgUserManage           = new("org:user_manage");

    // ── Audit / compliance read-only ────────────────────────────────────────
    public static readonly Permission AuditRead               = new("audit:read");

    public override string ToString() => Code;
}
