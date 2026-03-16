using Mro.Application.Features.Employees.Commands;
using Mro.Application.Features.Employees.Queries;
using Mro.Domain.Common.Permissions;

namespace Mro.Application.Common;

/// <summary>
/// Centralised registry of which Permission each command or query requires.
/// Enforced automatically by <see cref="Behaviors.PermissionBehaviour{TRequest,TResponse}"/>.
///
/// Requests not listed here are considered public within the [Authorize] boundary —
/// they only require a valid JWT, not a specific permission.
/// </summary>
public static class PermissionRequirements
{
    private static readonly Dictionary<Type, Permission> _map = new()
    {
        // ── Employee CRUD ─────────────────────────────────────────────────
        [typeof(CreateEmployeeCommand)]           = Permission.PersonnelManage,
        [typeof(UpdateEmployeeCommand)]           = Permission.PersonnelManage,
        [typeof(SetEmployeeStatusCommand)]        = Permission.PersonnelManage,

        // ── Licences ──────────────────────────────────────────────────────
        [typeof(AddLicenceCommand)]               = Permission.PersonnelManage,
        [typeof(UpdateLicenceCommand)]            = Permission.PersonnelManage,

        // ── Authorisations ────────────────────────────────────────────────
        [typeof(GrantAuthorisationCommand)]       = Permission.PersonnelManage,
        [typeof(AmendAuthorisationCommand)]       = Permission.PersonnelManage,
        [typeof(SuspendAuthorisationCommand)]     = Permission.PersonnelManage,
        [typeof(RevokeAuthorisationCommand)]      = Permission.PersonnelManage,

        // ── Training ──────────────────────────────────────────────────────
        [typeof(RecordTrainingCommand)]           = Permission.PersonnelManage,
        [typeof(UpdateTrainingRecordCommand)]     = Permission.PersonnelManage,

        // ── Shifts ────────────────────────────────────────────────────────
        [typeof(RecordShiftCommand)]              = Permission.PersonnelManage,

        // ── Attachments ───────────────────────────────────────────────────
        [typeof(RegisterEmployeeAttachmentCommand)] = Permission.PersonnelManage,
        [typeof(DeleteEmployeeAttachmentCommand)]   = Permission.PersonnelManage,

        // ── Notes ─────────────────────────────────────────────────────────
        [typeof(AddEmployeeNoteCommand)]          = Permission.PersonnelManage,
        [typeof(DeleteEmployeeNoteCommand)]       = Permission.PersonnelManage,

        // ── Restrictions (elevated) ───────────────────────────────────────
        [typeof(AddEmployeeRestrictionCommand)]   = Permission.PersonnelRestrict,
        [typeof(LiftEmployeeRestrictionCommand)]  = Permission.PersonnelRestrict,

        // ── Read queries ──────────────────────────────────────────────────
        [typeof(GetEmployeeQuery)]                = Permission.PersonnelView,
        [typeof(ListEmployeesQuery)]              = Permission.PersonnelView,
        [typeof(GetEmployeeLicencesQuery)]        = Permission.PersonnelView,
        [typeof(GetEmployeeAuthorisationsQuery)]  = Permission.PersonnelView,
        [typeof(GetEmployeeTrainingRecordsQuery)] = Permission.PersonnelView,
        [typeof(GetEmployeeCurrencyQuery)]        = Permission.PersonnelView,
        [typeof(GetEmployeeCurrencyStatusQuery)]  = Permission.PersonnelView,
        [typeof(GetExpiringCredentialsQuery)]     = Permission.PersonnelView,
        [typeof(ListEmployeeAttachmentsQuery)]    = Permission.PersonnelView,
        [typeof(GetEmployeeAttachmentUploadUrlQuery)] = Permission.PersonnelManage,
        [typeof(ListEmployeeRestrictionsQuery)]   = Permission.PersonnelView,
        [typeof(ListEmployeeNotesQuery)]          = Permission.PersonnelView,
        [typeof(GetEmployeeTimelineQuery)]        = Permission.PersonnelView,
    };

    /// <summary>
    /// Returns the required permission for the given request type,
    /// or null if no permission restriction applies.
    /// </summary>
    public static Permission? For(Type requestType) =>
        _map.TryGetValue(requestType, out var perm) ? perm : null;
}
