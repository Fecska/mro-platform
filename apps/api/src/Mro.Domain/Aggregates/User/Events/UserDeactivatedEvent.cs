using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.User.Events;

public sealed record UserDeactivatedEvent : AuditDomainEvent
{
    public required string Reason { get; init; }
}
