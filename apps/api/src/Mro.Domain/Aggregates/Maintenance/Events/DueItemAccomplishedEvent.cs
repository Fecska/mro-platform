using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.Maintenance.Events;

public sealed record DueItemAccomplishedEvent : AuditDomainEvent
{
    public required string DueItemRef { get; init; }
    public required Guid AircraftId { get; init; }
    public required DateTimeOffset AccomplishedAt { get; init; }
    public required DateTimeOffset? NextDueDate { get; init; }
}
