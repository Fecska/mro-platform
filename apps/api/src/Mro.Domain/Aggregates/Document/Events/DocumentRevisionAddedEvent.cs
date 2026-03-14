using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.Document.Events;

public sealed record DocumentRevisionAddedEvent : AuditDomainEvent
{
    public required string RevisionNumber { get; init; }
    public required Guid RevisionId { get; init; }
    public required DateOnly EffectiveAt { get; init; }
}
