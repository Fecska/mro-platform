using Mro.Domain.Aggregates.Aircraft;

namespace Mro.Application.Abstractions;

public interface IAircraftRepository
{
    Task<Aircraft?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default);
    Task<Aircraft?> GetByRegistrationAsync(string registration, Guid organisationId, CancellationToken ct = default);
    Task<IReadOnlyList<Aircraft>> ListAsync(Guid organisationId, int page, int pageSize, CancellationToken ct = default);
    Task<int> CountAsync(Guid organisationId, CancellationToken ct = default);
    Task AddAsync(Aircraft aircraft, CancellationToken ct = default);
    Task UpdateAsync(Aircraft aircraft, CancellationToken ct = default);

    Task<AircraftType?> GetTypeByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default);
    Task AddTypeAsync(AircraftType aircraftType, CancellationToken ct = default);
}
