using MediatR;
using Mro.Application.Common;
using Mro.Application.Features.Users.Dtos;
using Mro.Domain.Common.Permissions;

namespace Mro.Application.Features.Users.Queries;

public sealed record ListPermissionsQuery : IRequest<Result<IReadOnlyList<PermissionDto>>>;

public sealed class ListPermissionsQueryHandler
    : IRequestHandler<ListPermissionsQuery, Result<IReadOnlyList<PermissionDto>>>
{
    private static readonly IReadOnlyList<PermissionDto> _permissions =
    [
        new(Permission.WorkOrderCreate.Code,    "Create new work orders"),
        new(Permission.WorkOrderUpdate.Code,    "Update work order details and tasks"),
        new(Permission.WorkOrderClose.Code,     "Close completed work orders"),
        new(Permission.WorkOrderCancel.Code,    "Cancel work orders"),
        new(Permission.DefectRaise.Code,        "Raise a new defect"),
        new(Permission.DefectTriage.Code,       "Triage and categorise defects"),
        new(Permission.DefectDefer.Code,        "Defer a defect with MEL/CDL reference"),
        new(Permission.DefectClear.Code,        "Clear a resolved defect"),
        new(Permission.ReleaseInitiate.Code,    "Initiate a release certificate"),
        new(Permission.ReleaseSign.Code,        "Sign a release certificate (CRS)"),
        new(Permission.ReleaseRevoke.Code,      "Void a release certificate"),
        new(Permission.InventoryIssue.Code,     "Issue stock items to a work order"),
        new(Permission.InventoryReceive.Code,   "Receive stock into store"),
        new(Permission.InventoryAdjust.Code,    "Adjust stock quantities"),
        new(Permission.AircraftView.Code,       "View aircraft records"),
        new(Permission.AircraftManage.Code,     "Create and manage aircraft records"),
        new(Permission.PersonnelView.Code,      "View employee and licence records"),
        new(Permission.PersonnelManage.Code,    "Manage employee records and authorisations"),
        new(Permission.PersonnelRestrict.Code,  "Apply and lift operational restrictions on employees"),
        new(Permission.OrgManage.Code,          "Manage organisation settings"),
        new(Permission.OrgUserManage.Code,      "Manage users within the organisation"),
        new(Permission.AuditRead.Code,          "Read audit and compliance events"),
    ];

    public Task<Result<IReadOnlyList<PermissionDto>>> Handle(
        ListPermissionsQuery request,
        CancellationToken ct) =>
        Task.FromResult(Result.Success(_permissions));
}
