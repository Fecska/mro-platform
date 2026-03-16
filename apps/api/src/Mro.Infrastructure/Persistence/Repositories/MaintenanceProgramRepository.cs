using Microsoft.EntityFrameworkCore;
using Mro.Application.Abstractions;
using Mro.Domain.Aggregates.Maintenance;

namespace Mro.Infrastructure.Persistence.Repositories;

public sealed class MaintenanceProgramRepository(AppDbContext db) : IMaintenanceProgramRepository
{
    public async Task<MaintenanceProgram?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<MaintenanceProgram>()
            .FirstOrDefaultAsync(p => p.Id == id && p.OrganisationId == organisationId, ct);

    public async Task<IReadOnlyList<MaintenanceProgram>> ListAsync(
        Guid organisationId, string? aircraftTypeCode, CancellationToken ct = default)
    {
        var query = db.Set<MaintenanceProgram>().Where(p => p.OrganisationId == organisationId);
        if (!string.IsNullOrEmpty(aircraftTypeCode))
            query = query.Where(p => p.AircraftTypeCode == aircraftTypeCode.ToUpperInvariant());
        return await query.OrderBy(p => p.ProgramNumber).ToListAsync(ct);
    }

    public async Task AddAsync(MaintenanceProgram program, CancellationToken ct = default)
    {
        await db.Set<MaintenanceProgram>().AddAsync(program, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(MaintenanceProgram program, CancellationToken ct = default)
    {
        db.Set<MaintenanceProgram>().Update(program);
        await db.SaveChangesAsync(ct);
    }
}
