using Mro.Domain.Aggregates.Defect.Enums;
using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.Defect.Events;

public sealed record DefectStatusChangedEvent : AuditDomainEvent
{
    public required string DefectNumber { get; init; }
    public required DefectStatus FromStatus { get; init; }
    public required DefectStatus ToStatus { get; init; }
}
