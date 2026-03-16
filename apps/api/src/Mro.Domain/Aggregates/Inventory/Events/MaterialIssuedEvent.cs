using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.Inventory.Events;

public sealed record MaterialIssuedEvent : AuditDomainEvent
{
    public required Guid StockItemId { get; init; }
    public required decimal QuantityIssued { get; init; }
    public required Guid WorkOrderId { get; init; }
    public required Guid WorkOrderTaskId { get; init; }
}
