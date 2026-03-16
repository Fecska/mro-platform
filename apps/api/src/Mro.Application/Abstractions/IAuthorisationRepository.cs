using Mro.Domain.Aggregates.Employee;

namespace Mro.Application.Abstractions;

public interface IAuthorisationRepository
{
    Task<Authorisation?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default);
    Task UpdateAsync(Authorisation authorisation, CancellationToken ct = default);
}
