using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.Aircraft.Events;

public sealed record ComponentRemovedEvent : AuditDomainEvent
{
    public required string PartNumber { get; init; }
    public required string SerialNumber { get; init; }
    public required string InstallationPosition { get; init; }
    public required string Reason { get; init; }
}
