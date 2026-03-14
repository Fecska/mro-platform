using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.Document.Events;

public sealed record DocumentSupersededEvent : AuditDomainEvent
{
    public required string DocumentNumber { get; init; }
    public required Guid SupersededByDocumentId { get; init; }
}
