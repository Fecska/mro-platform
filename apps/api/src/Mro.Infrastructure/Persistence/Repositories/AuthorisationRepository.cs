using Microsoft.EntityFrameworkCore;
using Mro.Application.Abstractions;
using Mro.Domain.Aggregates.Employee;

namespace Mro.Infrastructure.Persistence.Repositories;

public sealed class AuthorisationRepository(AppDbContext db) : IAuthorisationRepository
{
    public async Task<Authorisation?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<Authorisation>()
            .FirstOrDefaultAsync(a => a.Id == id && a.OrganisationId == organisationId, ct);

    public async Task UpdateAsync(Authorisation authorisation, CancellationToken ct = default)
    {
        db.Set<Authorisation>().Update(authorisation);
        await db.SaveChangesAsync(ct);
    }
}
