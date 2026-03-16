using Mro.Domain.Aggregates.Employee;

namespace Mro.Application.Abstractions;

public interface ILicenceRepository
{
    Task<Licence?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default);
    Task UpdateAsync(Licence licence, CancellationToken ct = default);
}
