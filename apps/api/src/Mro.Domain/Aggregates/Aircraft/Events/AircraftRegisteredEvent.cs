using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.Aircraft.Events;

public sealed record AircraftRegisteredEvent : AuditDomainEvent
{
    public required string Registration { get; init; }
    public required string SerialNumber { get; init; }
    public required Guid AircraftTypeId { get; init; }
}
