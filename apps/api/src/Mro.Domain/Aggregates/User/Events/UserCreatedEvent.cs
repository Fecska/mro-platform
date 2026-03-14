using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.User.Events;

public sealed record UserCreatedEvent : AuditDomainEvent
{
    public required string Email { get; init; }
    public required string DisplayName { get; init; }
}
