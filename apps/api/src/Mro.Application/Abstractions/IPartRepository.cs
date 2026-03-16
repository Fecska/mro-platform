using Mro.Domain.Aggregates.Inventory;
using Mro.Domain.Aggregates.Inventory.Enums;

namespace Mro.Application.Abstractions;

public interface IPartRepository
{
    Task<Part?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default);
    Task<bool> ExistsAsync(string partNumber, Guid organisationId, CancellationToken ct = default);
    Task<IReadOnlyList<Part>> ListAsync(Guid organisationId, PartStatus? status, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(Part part, CancellationToken ct = default);
    Task UpdateAsync(Part part, CancellationToken ct = default);
}
