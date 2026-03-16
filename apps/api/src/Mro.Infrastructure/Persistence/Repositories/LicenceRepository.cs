using Microsoft.EntityFrameworkCore;
using Mro.Application.Abstractions;
using Mro.Domain.Aggregates.Employee;

namespace Mro.Infrastructure.Persistence.Repositories;

public sealed class LicenceRepository(AppDbContext db) : ILicenceRepository
{
    public async Task<Licence?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<Licence>()
            .FirstOrDefaultAsync(l => l.Id == id && l.OrganisationId == organisationId, ct);

    public async Task UpdateAsync(Licence licence, CancellationToken ct = default)
    {
        db.Set<Licence>().Update(licence);
        await db.SaveChangesAsync(ct);
    }
}
