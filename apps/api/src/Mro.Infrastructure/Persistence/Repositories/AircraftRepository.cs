using Microsoft.EntityFrameworkCore;
using Mro.Application.Abstractions;
using Mro.Domain.Aggregates.Aircraft;

namespace Mro.Infrastructure.Persistence.Repositories;

public sealed class AircraftRepository(AppDbContext db) : IAircraftRepository
{
    public async Task<Aircraft?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<Aircraft>()
            .Include("_counters")
            .Include("_statusHistory")
            .Include("_installedComponents")
            .Include(a => a.AircraftType)
            .FirstOrDefaultAsync(a => a.Id == id && a.OrganisationId == organisationId, ct);

    public async Task<Aircraft?> GetByRegistrationAsync(string registration, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<Aircraft>()
            .FirstOrDefaultAsync(a => a.Registration == registration.ToUpperInvariant()
                                   && a.OrganisationId == organisationId, ct);

    public async Task<IReadOnlyList<Aircraft>> ListAsync(Guid organisationId, int page, int pageSize, CancellationToken ct = default) =>
        await db.Set<Aircraft>()
            .Include(a => a.AircraftType)
            .Where(a => a.OrganisationId == organisationId)
            .OrderBy(a => a.Registration)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<int> CountAsync(Guid organisationId, CancellationToken ct = default) =>
        await db.Set<Aircraft>().CountAsync(a => a.OrganisationId == organisationId, ct);

    public async Task AddAsync(Aircraft aircraft, CancellationToken ct = default)
    {
        await db.Set<Aircraft>().AddAsync(aircraft, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Aircraft aircraft, CancellationToken ct = default)
    {
        db.Set<Aircraft>().Update(aircraft);
        await db.SaveChangesAsync(ct);
    }

    public async Task<AircraftType?> GetTypeByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<AircraftType>()
            .FirstOrDefaultAsync(t => t.Id == id && t.OrganisationId == organisationId, ct);

    public async Task AddTypeAsync(AircraftType aircraftType, CancellationToken ct = default)
    {
        await db.Set<AircraftType>().AddAsync(aircraftType, ct);
        await db.SaveChangesAsync(ct);
    }
}
