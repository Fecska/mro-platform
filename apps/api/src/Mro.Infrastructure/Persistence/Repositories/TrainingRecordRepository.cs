using Microsoft.EntityFrameworkCore;
using Mro.Application.Abstractions;
using Mro.Domain.Aggregates.Employee;

namespace Mro.Infrastructure.Persistence.Repositories;

public sealed class TrainingRecordRepository(AppDbContext db) : ITrainingRecordRepository
{
    public async Task<TrainingRecord?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<TrainingRecord>()
            .FirstOrDefaultAsync(t => t.Id == id && t.OrganisationId == organisationId, ct);

    public async Task UpdateAsync(TrainingRecord record, CancellationToken ct = default)
    {
        db.Set<TrainingRecord>().Update(record);
        await db.SaveChangesAsync(ct);
    }
}
