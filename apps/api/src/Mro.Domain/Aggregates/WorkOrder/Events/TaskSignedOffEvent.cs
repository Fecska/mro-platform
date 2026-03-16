using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.WorkOrder.Events;

public sealed record TaskSignedOffEvent : AuditDomainEvent
{
    public required string WoNumber { get; init; }
    public required string TaskNumber { get; init; }
    public required Guid CertifyingStaffUserId { get; init; }
}
