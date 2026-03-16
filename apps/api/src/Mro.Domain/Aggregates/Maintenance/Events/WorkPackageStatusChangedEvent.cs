using Mro.Domain.Aggregates.Maintenance.Enums;
using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.Maintenance.Events;

public sealed record WorkPackageStatusChangedEvent : AuditDomainEvent
{
    public required string PackageNumber { get; init; }
    public required WorkPackageStatus OldStatus { get; init; }
    public required WorkPackageStatus NewStatus { get; init; }
}
