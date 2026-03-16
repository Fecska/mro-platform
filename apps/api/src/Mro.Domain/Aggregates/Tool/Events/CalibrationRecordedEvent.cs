using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.Tool.Events;

public sealed record CalibrationRecordedEvent : AuditDomainEvent
{
    public required string ToolNumber { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
}
