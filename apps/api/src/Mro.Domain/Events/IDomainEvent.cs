using MediatR;

namespace Mro.Domain.Events;

/// <summary>
/// Marker interface for all domain events.
/// Implements MediatR INotification so events can be dispatched
/// via the in-process event bus without HTTP overhead.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>Unique identifier of this event instance.</summary>
    Guid EventId { get; }

    /// <summary>UTC timestamp when the event was raised.</summary>
    DateTimeOffset OccurredAt { get; }
}
