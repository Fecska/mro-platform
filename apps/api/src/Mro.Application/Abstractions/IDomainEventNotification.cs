using MediatR;
using Mro.Domain.Events;

namespace Mro.Application.Abstractions;

/// <summary>
/// Wraps a domain event as a MediatR INotification so it can be dispatched
/// via the in-process event bus after SaveChanges completes.
///
/// This keeps the Domain layer free of any framework dependencies.
/// Usage: publish DomainEventNotification{T}(domainEvent) via IPublisher.
/// </summary>
public interface IDomainEventNotification<out TDomainEvent> : INotification
    where TDomainEvent : IDomainEvent
{
    TDomainEvent DomainEvent { get; }
}

/// <summary>Default implementation.</summary>
public sealed record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent)
    : IDomainEventNotification<TDomainEvent>
    where TDomainEvent : IDomainEvent;
