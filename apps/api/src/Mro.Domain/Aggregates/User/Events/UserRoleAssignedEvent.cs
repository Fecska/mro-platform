using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.User.Events;

public sealed record UserRoleAssignedEvent : AuditDomainEvent
{
    public required string RoleName { get; init; }
    public required Guid TargetUserId { get; init; }
}
