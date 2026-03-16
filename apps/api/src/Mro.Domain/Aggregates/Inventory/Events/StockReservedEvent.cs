using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.Inventory.Events;

public sealed record StockReservedEvent : AuditDomainEvent
{
    public required Guid StockItemId { get; init; }
    public required decimal QuantityReserved { get; init; }
    public required Guid WorkOrderId { get; init; }
    public required Guid WorkOrderTaskId { get; init; }
}
