using Mro.Domain.Aggregates.Inspection.Enums;
using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.Inspection.Events;

public sealed record InspectionCreatedEvent : AuditDomainEvent
{
    public required string InspectionNumber { get; init; }
    public required Guid WorkOrderId { get; init; }
    public required InspectionType InspectionType { get; init; }
}
