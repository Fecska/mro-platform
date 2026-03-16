using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.Release.Events;

public sealed record CertificateVoidedEvent : AuditDomainEvent
{
    public required string CertificateNumber { get; init; }
    public required string VoidReason { get; init; }
}
