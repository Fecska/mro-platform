using Mro.Domain.Aggregates.Maintenance;

namespace Mro.Application.Abstractions;

public interface IMaintenanceProgramRepository
{
    Task<MaintenanceProgram?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default);
    Task<IReadOnlyList<MaintenanceProgram>> ListAsync(Guid organisationId, string? aircraftTypeCode, CancellationToken ct = default);
    Task AddAsync(MaintenanceProgram program, CancellationToken ct = default);
    Task UpdateAsync(MaintenanceProgram program, CancellationToken ct = default);
}
