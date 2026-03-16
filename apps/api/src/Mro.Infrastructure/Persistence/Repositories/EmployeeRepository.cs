using Microsoft.EntityFrameworkCore;
using Mro.Application.Abstractions;
using Mro.Domain.Aggregates.Employee;
using Mro.Domain.Aggregates.Employee.Enums;

namespace Mro.Infrastructure.Persistence.Repositories;

public sealed class EmployeeRepository(AppDbContext db) : IEmployeeRepository
{
    public async Task<Employee?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<Employee>()
            .Include("_licences")
            .Include("_authorisations")
            .Include("_trainingRecords")
            .Include("_competencyAssessments")
            .Include("_shifts")
            .FirstOrDefaultAsync(e => e.Id == id && e.OrganisationId == organisationId, ct);

    public async Task<Employee?> GetByUserIdAsync(Guid userId, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<Employee>()
            .Include("_authorisations")
            .Include("_trainingRecords")
            .FirstOrDefaultAsync(
                e => e.UserId == userId && e.OrganisationId == organisationId, ct);

    public async Task<bool> ExistsAsync(string employeeNumber, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<Employee>()
            .AnyAsync(e => e.EmployeeNumber == employeeNumber.ToUpperInvariant()
                        && e.OrganisationId == organisationId, ct);

    public async Task<(IReadOnlyList<Employee> Items, int Total)> ListAsync(
        Guid organisationId,
        EmployeeStatus? status,
        bool? isActive,
        LicenceCategory? licenceCategory,
        Guid? stationId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = db.Set<Employee>()
            .Include("_licences")
            .Include("_authorisations")
            .Where(e => e.OrganisationId == organisationId);

        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);
        else if (isActive.HasValue)
            query = isActive.Value
                ? query.Where(e => e.Status == EmployeeStatus.Active)
                : query.Where(e => e.Status != EmployeeStatus.Active);

        if (licenceCategory.HasValue)
        {
            var cat = licenceCategory.Value;
            query = query.Where(e =>
                db.Set<Licence>().Any(l => l.EmployeeId == e.Id && l.Category == cat));
        }

        if (stationId.HasValue)
            query = query.Where(e => e.DefaultStationId == stationId.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<int> CountAsync(Guid organisationId, CancellationToken ct = default) =>
        await db.Set<Employee>().CountAsync(e => e.OrganisationId == organisationId, ct);

    public async Task<IReadOnlyList<Employee>> ListActiveWithAuthorisationsAsync(
        Guid organisationId, CancellationToken ct = default) =>
        await db.Set<Employee>()
            .Include("_authorisations")
            .Include("_trainingRecords")
            .Where(e => e.OrganisationId == organisationId && e.Status == EmployeeStatus.Active)
            .OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Employee>> ListActiveWithLicencesAndAuthorisationsAsync(
        Guid organisationId, CancellationToken ct = default) =>
        await db.Set<Employee>()
            .Include("_licences")
            .Include("_authorisations")
            .Where(e => e.OrganisationId == organisationId && e.Status == EmployeeStatus.Active)
            .OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
            .ToListAsync(ct);

    public async Task<Employee?> GetWithAttachmentsAsync(
        Guid id, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<Employee>()
            .Include("_attachments")
            .FirstOrDefaultAsync(e => e.Id == id && e.OrganisationId == organisationId, ct);

    public async Task<IReadOnlyList<EmployeeAttachment>> ListAttachmentsAsync(
        Guid employeeId,
        Guid organisationId,
        AttachmentType? type = null,
        CancellationToken ct = default)
    {
        var query = db.Set<EmployeeAttachment>()
            .Where(a => a.EmployeeId == employeeId
                     && a.OrganisationId == organisationId
                     && a.DeletedAt == null);

        if (type.HasValue)
            query = query.Where(a => a.AttachmentType == type.Value);

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Employee?> GetWithRestrictionsAsync(
        Guid id, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<Employee>()
            .Include("_restrictions")
            .FirstOrDefaultAsync(e => e.Id == id && e.OrganisationId == organisationId, ct);

    public async Task<Employee?> GetWithNotesAsync(
        Guid id, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<Employee>()
            .Include("_notes")
            .FirstOrDefaultAsync(e => e.Id == id && e.OrganisationId == organisationId, ct);

    public async Task<IReadOnlyList<EmployeeRestriction>> ListRestrictionsAsync(
        Guid employeeId,
        Guid organisationId,
        bool activeOnly = false,
        CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var query = db.Set<EmployeeRestriction>()
            .Where(r => r.EmployeeId == employeeId
                     && r.OrganisationId == organisationId
                     && r.DeletedAt == null);

        if (activeOnly)
            query = query.Where(r =>
                r.ActiveFrom <= today
                && (r.ActiveUntil == null || r.ActiveUntil >= today));

        return await query
            .OrderByDescending(r => r.ActiveFrom)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<EmployeeNote>> ListNotesAsync(
        Guid employeeId,
        Guid organisationId,
        CancellationToken ct = default) =>
        await db.Set<EmployeeNote>()
            .Where(n => n.EmployeeId == employeeId
                     && n.OrganisationId == organisationId
                     && n.DeletedAt == null)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);

    public async Task<(
        IReadOnlyList<Licence>             Licences,
        IReadOnlyList<Authorisation>       Authorisations,
        IReadOnlyList<TrainingRecord>      TrainingRecords,
        IReadOnlyList<Shift>               Shifts,
        IReadOnlyList<EmployeeRestriction> Restrictions
    )> GetForTimelineAsync(
        Guid employeeId,
        Guid organisationId,
        DateOnly from,
        CancellationToken ct = default)
    {
        var licences = await db.Set<Licence>()
            .Where(l => l.EmployeeId == employeeId && l.OrganisationId == organisationId)
            .OrderBy(l => l.CreatedAt)
            .ToListAsync(ct);

        var authorisations = await db.Set<Authorisation>()
            .Where(a => a.EmployeeId == employeeId && a.OrganisationId == organisationId)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync(ct);

        var trainingRecords = await db.Set<TrainingRecord>()
            .Where(t => t.EmployeeId == employeeId
                     && t.OrganisationId == organisationId
                     && t.CompletedAt >= from)
            .OrderBy(t => t.CompletedAt)
            .ToListAsync(ct);

        var shifts = await db.Set<Shift>()
            .Where(s => s.EmployeeId == employeeId
                     && s.OrganisationId == organisationId
                     && s.ShiftDate >= from)
            .OrderBy(s => s.ShiftDate)
            .ToListAsync(ct);

        var restrictions = await db.Set<EmployeeRestriction>()
            .Where(r => r.EmployeeId == employeeId
                     && r.OrganisationId == organisationId
                     && r.DeletedAt == null)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(ct);

        return (licences, authorisations, trainingRecords, shifts, restrictions);
    }

    public async Task AddAsync(Employee employee, CancellationToken ct = default)
    {
        await db.Set<Employee>().AddAsync(employee, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Employee employee, CancellationToken ct = default)
    {
        // Do NOT use db.Set<Employee>().Update(employee): it recursively marks every
        // reachable entity as Modified — including newly-created child entities
        // (Licence, TrainingRecord, …) that must be Added, not Updated.
        //
        // Root cause: accessing db.Entry() or SaveChangesAsync triggers DetectChanges
        // which, for collection navigations with private backing fields, incorrectly
        // marks new (never-persisted) child entities as Modified instead of Added.
        //
        // Fix: disable auto-detect-changes, explicitly track new children as Added,
        // then run DetectChanges manually (for scalar-property changes on the root),
        // and finally save — without letting SaveChanges re-run DetectChanges.

        db.ChangeTracker.AutoDetectChangesEnabled = false;
        try
        {
            var empEntry = db.ChangeTracker.Entries<Employee>()
                             .FirstOrDefault(e => ReferenceEquals(e.Entity, employee))
                          ?? db.Attach(employee);

            // Fallback: root was somehow detached
            if (empEntry.State == EntityState.Detached)
                empEntry.State = EntityState.Modified;

            // Walk collection navigations; explicitly track new children as Added
            // before DetectChanges can incorrectly classify them as Modified.
            foreach (var collectionEntry in empEntry.Collections)
            {
                if (collectionEntry.CurrentValue is not IEnumerable<object> items) continue;
                foreach (var child in items.ToList())
                {
                    var childState = db.ChangeTracker.Entries()
                                       .FirstOrDefault(e => ReferenceEquals(e.Entity, child))
                                       ?.State ?? EntityState.Detached;
                    if (childState == EntityState.Detached)
                        db.Entry(child).State = EntityState.Added;
                }
            }

            // Now detect scalar-property changes on already-tracked entities
            db.ChangeTracker.DetectChanges();

            await db.SaveChangesAsync(ct);
        }
        finally
        {
            db.ChangeTracker.AutoDetectChangesEnabled = true;
        }
    }
}
