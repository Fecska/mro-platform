using Mro.Domain.Aggregates.Aircraft.Enums;
using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.Aircraft.Events;

public sealed record AircraftStatusChangedEvent : AuditDomainEvent
{
    public required AircraftStatus FromStatus { get; init; }
    public required AircraftStatus ToStatus { get; init; }
    public required string Reason { get; init; }
}
