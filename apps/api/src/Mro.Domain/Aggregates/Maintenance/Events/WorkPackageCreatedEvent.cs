using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.Maintenance.Events;

public sealed record WorkPackageCreatedEvent : AuditDomainEvent
{
    public required string PackageNumber { get; init; }
    public required Guid AircraftId { get; init; }
}
