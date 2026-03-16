using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.Defect.Events;

/// <summary>Raised when a defect's rectification is certified (CRS signed).</summary>
public sealed record DefectClearedEvent : AuditDomainEvent
{
    public required string DefectNumber { get; init; }
    public required Guid AircraftId { get; init; }
    /// <summary>User who signed the Certificate of Release to Service.</summary>
    public required Guid CertifyingStaffUserId { get; init; }
}
