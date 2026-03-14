using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.Organisation.Events;

/// <summary>Raised when a new organisation is registered on the platform.</summary>
public sealed record OrganisationCreatedEvent : AuditDomainEvent
{
    public required string OrganisationName { get; init; }
    public required string Part145CertNumber { get; init; }
}
