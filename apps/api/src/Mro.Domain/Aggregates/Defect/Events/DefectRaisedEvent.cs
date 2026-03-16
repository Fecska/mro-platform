using Mro.Domain.Aggregates.Defect.Enums;
using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.Defect.Events;

public sealed record DefectRaisedEvent : AuditDomainEvent
{
    public required string DefectNumber { get; init; }
    public required Guid AircraftId { get; init; }
    public required DefectSeverity Severity { get; init; }
    public required DefectSource Source { get; init; }
}
