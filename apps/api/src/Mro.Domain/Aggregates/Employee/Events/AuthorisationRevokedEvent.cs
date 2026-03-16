using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.Employee.Events;

public sealed record AuthorisationRevokedEvent : AuditDomainEvent
{
    public required string EmployeeNumber { get; init; }
    public required string AuthorisationNumber { get; init; }
    public required string Reason { get; init; }
}
