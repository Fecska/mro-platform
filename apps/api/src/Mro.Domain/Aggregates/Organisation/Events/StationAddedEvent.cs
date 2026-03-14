using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.Organisation.Events;

/// <summary>Raised when a new station is added to an organisation.</summary>
public sealed record StationAddedEvent : AuditDomainEvent
{
    public required Guid StationId { get; init; }
    public required string StationName { get; init; }
    public required string IcaoCode { get; init; }
}
