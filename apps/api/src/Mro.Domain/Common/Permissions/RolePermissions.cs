namespace Mro.Domain.Common.Permissions;

/// <summary>
/// Defines which permissions each role grants.
///
/// Roles are not hierarchical — a user's effective permissions are the union of
/// all permissions granted by every role they hold simultaneously.
///
/// This is the single source of truth for RBAC permission resolution.
/// Checked at the Application layer via ICurrentUserService.HasPermission().
/// </summary>
public static class RolePermissions
{
    private static readonly Dictionary<string, HashSet<string>> _map = new()
    {
        // ── Maintenance execution ──────────────────────────────────────────
        [Roles.CertifyingStaff] =
        [
            Permission.WorkOrderUpdate.Code,
            Permission.WorkOrderClose.Code,
            Permission.DefectRaise.Code,
            Permission.DefectClear.Code,
            Permission.ReleaseInitiate.Code,
            Permission.ReleaseSign.Code,
            Permission.InventoryIssue.Code,
            Permission.AircraftView.Code,
            Permission.PersonnelView.Code,
            Permission.AuditRead.Code,
        ],

        [Roles.Engineer] =
        [
            Permission.WorkOrderUpdate.Code,
            Permission.DefectRaise.Code,
            Permission.InventoryIssue.Code,
            Permission.AircraftView.Code,
            Permission.PersonnelView.Code,
        ],

        // ── Quality & oversight ───────────────────────────────────────────
        [Roles.Inspector] =
        [
            Permission.WorkOrderUpdate.Code,
            Permission.WorkOrderClose.Code,
            Permission.DefectRaise.Code,
            Permission.DefectTriage.Code,
            Permission.DefectDefer.Code,
            Permission.DefectClear.Code,
            Permission.ReleaseSign.Code,
            Permission.AircraftView.Code,
            Permission.PersonnelView.Code,
            Permission.AuditRead.Code,
        ],

        // ── Planning & production ─────────────────────────────────────────
        [Roles.Planner] =
        [
            Permission.WorkOrderCreate.Code,
            Permission.WorkOrderUpdate.Code,
            Permission.WorkOrderClose.Code,
            Permission.WorkOrderCancel.Code,
            Permission.DefectRaise.Code,
            Permission.DefectTriage.Code,
            Permission.DefectDefer.Code,
            Permission.InventoryIssue.Code,
            Permission.AircraftView.Code,
            Permission.PersonnelView.Code,
        ],

        // ── Supply chain ──────────────────────────────────────────────────
        [Roles.StoreKeeper] =
        [
            Permission.InventoryIssue.Code,
            Permission.InventoryReceive.Code,
            Permission.InventoryAdjust.Code,
            Permission.AircraftView.Code,
        ],

        // ── HR / Personnel ────────────────────────────────────────────────
        [Roles.PersonnelAdmin] =
        [
            Permission.PersonnelView.Code,
            Permission.PersonnelManage.Code,
            Permission.PersonnelRestrict.Code,
            Permission.AircraftView.Code,
        ],

        // ── Administration ────────────────────────────────────────────────
        [Roles.OrgAdmin] =
        [
            Permission.PersonnelView.Code,
            Permission.PersonnelManage.Code,
            Permission.PersonnelRestrict.Code,
            Permission.OrgManage.Code,
            Permission.OrgUserManage.Code,
            Permission.AuditRead.Code,
            Permission.AircraftView.Code,
            Permission.AircraftManage.Code,
        ],

        [Roles.SystemAdmin] =
        [
            Permission.AircraftView.Code,
            Permission.PersonnelView.Code,
            Permission.OrgManage.Code,
            Permission.AuditRead.Code,
        ],
    };

    private static readonly IReadOnlySet<string> _empty = new HashSet<string>();

    /// <summary>
    /// Returns the set of permission codes granted to the specified role.
    /// Returns an empty set for unknown roles.
    /// </summary>
    public static IReadOnlySet<string> For(string role) =>
        _map.TryGetValue(role, out var perms) ? perms : _empty;

    /// <summary>
    /// Returns the effective permission codes for a user holding multiple roles.
    /// Result is the union of all role permission sets.
    /// </summary>
    public static IReadOnlySet<string> Effective(IEnumerable<string> roles)
    {
        var result = new HashSet<string>();
        foreach (var role in roles)
        {
            if (_map.TryGetValue(role, out var perms))
                result.UnionWith(perms);
        }
        return result;
    }
}
