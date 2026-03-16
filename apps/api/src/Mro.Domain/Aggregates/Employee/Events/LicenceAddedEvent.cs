using Mro.Domain.Aggregates.Employee.Enums;
using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.Employee.Events;

public sealed record LicenceAddedEvent : AuditDomainEvent
{
    public required string EmployeeNumber { get; init; }
    public required string LicenceNumber { get; init; }
    public required LicenceCategory Category { get; init; }
    public required string IssuingAuthority { get; init; }
    public required DateOnly? ExpiresAt { get; init; }
}
