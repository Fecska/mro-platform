using Mro.Domain.Aggregates.Defect.Enums;
using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.Defect.Events;

public sealed record DefectDeferredEvent : AuditDomainEvent
{
    public required string DefectNumber { get; init; }
    public required Guid DeferredDefectId { get; init; }
    public required DeferralCategory Category { get; init; }
    public required DateTimeOffset DeferredUntil { get; init; }
}
