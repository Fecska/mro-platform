using Mro.Domain.Aggregates.Inspection.Enums;
using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.Inspection.Events;

public sealed record InspectionOutcomeRecordedEvent : AuditDomainEvent
{
    public required string InspectionNumber { get; init; }
    public required InspectionStatus Outcome { get; init; }
}
