using Mro.Domain.Aggregates.WorkOrder.Enums;
using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.WorkOrder.Events;

public sealed record WorkOrderCreatedEvent : AuditDomainEvent
{
    public required string WoNumber { get; init; }
    public required Guid AircraftId { get; init; }
    public required WorkOrderType WorkOrderType { get; init; }
}
