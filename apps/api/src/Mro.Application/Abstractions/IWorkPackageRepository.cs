using Mro.Domain.Aggregates.Maintenance;
using Mro.Domain.Aggregates.Maintenance.Enums;

namespace Mro.Application.Abstractions;

public interface IWorkPackageRepository
{
    Task<WorkPackage?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default);
    Task<int> CountAsync(Guid organisationId, CancellationToken ct = default);
    Task<IReadOnlyList<WorkPackage>> ListAsync(
        Guid organisationId,
        Guid? aircraftId,
        WorkPackageStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default);
    Task AddAsync(WorkPackage package, CancellationToken ct = default);
    Task UpdateAsync(WorkPackage package, CancellationToken ct = default);
}
