using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.Aircraft.Events;

public sealed record ComponentInstalledEvent : AuditDomainEvent
{
    public required string PartNumber { get; init; }
    public required string SerialNumber { get; init; }
    public required string InstallationPosition { get; init; }
    public required Guid InstalledComponentId { get; init; }
}
