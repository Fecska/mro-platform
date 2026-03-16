using Mro.Domain.Aggregates.Employee.Enums;
using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.Employee.Events;

public sealed record AuthorisationGrantedEvent : AuditDomainEvent
{
    public required string EmployeeNumber { get; init; }
    public required string AuthorisationNumber { get; init; }
    public required LicenceCategory Category { get; init; }
    public required string Scope { get; init; }
    public required DateOnly ValidFrom { get; init; }
}
