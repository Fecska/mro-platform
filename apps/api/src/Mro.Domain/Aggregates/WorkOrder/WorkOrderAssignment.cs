using Mro.Domain.Aggregates.WorkOrder.Enums;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.WorkOrder;

/// <summary>
/// Records that a specific user has been assigned to a Work Order in a given role.
/// One user may hold multiple roles on the same WO (e.g. Mechanic + Inspector on different tasks).
/// </summary>
public sealed class WorkOrderAssignment : AuditableEntity
{
    public Guid WorkOrderId { get; private set; }

    public Guid UserId { get; private set; }

    public AssignmentRole Role { get; private set; }

    public DateTimeOffset AssignedAt { get; private set; }

    public Guid AssignedByUserId { get; private set; }

    // EF Core
    private WorkOrderAssignment() { }

    internal static WorkOrderAssignment Create(
        Guid workOrderId,
        Guid userId,
        AssignmentRole role,
        Guid assignedByUserId,
        Guid organisationId,
        Guid actorId)
    {
        return new WorkOrderAssignment
        {
            WorkOrderId = workOrderId,
            UserId = userId,
            Role = role,
            AssignedAt = DateTimeOffset.UtcNow,
            AssignedByUserId = assignedByUserId,
            OrganisationId = organisationId,
            CreatedBy = actorId,
            UpdatedBy = actorId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }
}
