namespace Mro.Domain.Events;

/// <summary>
/// Base record for all domain events.
/// Automatically assigns EventId and OccurredAt on construction.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
