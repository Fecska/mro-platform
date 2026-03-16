using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.WorkOrder.Events;

/// <summary>Raised when a WO reaches Completed (CRS issued).</summary>
public sealed record WorkOrderCompletedEvent : AuditDomainEvent
{
    public required string WoNumber { get; init; }
    public required Guid AircraftId { get; init; }
    public required Guid CertifyingStaffUserId { get; init; }
}
