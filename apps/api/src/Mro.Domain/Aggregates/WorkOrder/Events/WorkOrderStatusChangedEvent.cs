using Mro.Domain.Aggregates.WorkOrder.Enums;
using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.WorkOrder.Events;

public sealed record WorkOrderStatusChangedEvent : AuditDomainEvent
{
    public required string WoNumber { get; init; }
    public required WorkOrderStatus FromStatus { get; init; }
    public required WorkOrderStatus ToStatus { get; init; }
}
