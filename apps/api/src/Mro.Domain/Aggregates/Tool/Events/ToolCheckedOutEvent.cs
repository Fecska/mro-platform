using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.Tool.Events;

public sealed record ToolCheckedOutEvent : AuditDomainEvent
{
    public required string ToolNumber { get; init; }
    public required Guid WorkOrderTaskId { get; init; }
}
