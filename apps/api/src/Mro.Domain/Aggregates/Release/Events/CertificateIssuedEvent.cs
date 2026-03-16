using Mro.Domain.Aggregates.Release.Enums;
using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.Release.Events;

public sealed record CertificateIssuedEvent : AuditDomainEvent
{
    public required string CertificateNumber { get; init; }
    public required CertificateType CertificateType { get; init; }
    public required Guid WorkOrderId { get; init; }
    public required Guid SignerUserId { get; init; }
}
