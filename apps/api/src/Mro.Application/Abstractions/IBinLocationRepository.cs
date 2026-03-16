using Mro.Domain.Aggregates.Inventory;

namespace Mro.Application.Abstractions;

public interface IBinLocationRepository
{
    Task<BinLocation?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default);
    Task<IReadOnlyList<BinLocation>> ListAsync(Guid organisationId, CancellationToken ct = default);
    Task AddAsync(BinLocation location, CancellationToken ct = default);
    Task UpdateAsync(BinLocation location, CancellationToken ct = default);
}
