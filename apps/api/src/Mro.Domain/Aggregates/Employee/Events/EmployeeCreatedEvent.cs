using Mro.Domain.Common.Audit;

namespace Mro.Domain.Aggregates.Employee.Events;

public sealed record EmployeeCreatedEvent : AuditDomainEvent
{
    public required string EmployeeNumber { get; init; }
    public required string FullName { get; init; }
}
