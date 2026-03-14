using Mro.Domain.Aggregates.Document.Enums;
using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.Document.Events;

public sealed record DocumentRegisteredEvent : AuditDomainEvent
{
    public required string DocumentNumber { get; init; }
    public required DocumentType DocumentType { get; init; }
    public required string Title { get; init; }
}
