namespace Mro.Domain.Events;

/// <summary>
/// Marker interface for all domain events.
/// Domain layer has no dependency on MediatR — the Application layer
/// bridges domain events to MediatR notifications via IDomainEventNotification.
/// </summary>
public interface IDomainEvent
{
    /// <summary>Unique identifier of this event instance.</summary>
    Guid EventId { get; }

    /// <summary>UTC timestamp when the event was raised.</summary>
    DateTimeOffset OccurredAt { get; }
}
