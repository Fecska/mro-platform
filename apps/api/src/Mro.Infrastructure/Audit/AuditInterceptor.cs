using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Mro.Application.Abstractions;
using Mro.Domain.Entities;

namespace Mro.Infrastructure.Audit;

/// <summary>
/// EF Core SaveChanges interceptor that automatically stamps audit columns
/// (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, OrganisationId)
/// on every AuditableEntity before it is written to the database.
///
/// This ensures audit data is always consistent regardless of which
/// application service triggered the save.
/// </summary>
public sealed class AuditInterceptor(ICurrentUserService currentUserService)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        StampAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        StampAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    // ── Private ────────────────────────────────────────────────────────────

    private void StampAuditFields(DbContext? context)
    {
        if (context is null) return;

        var now = DateTimeOffset.UtcNow;
        var userId = currentUserService.UserId ?? Guid.Empty;
        var orgId = currentUserService.OrganisationId ?? Guid.Empty;

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = userId;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = userId;
                    entry.Entity.OrganisationId = orgId;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = userId;
                    // Prevent tampering with creation metadata
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    entry.Property(e => e.CreatedBy).IsModified = false;
                    // Prevent tampering with organisation scope
                    entry.Property(e => e.OrganisationId).IsModified = false;
                    break;
            }
        }
    }
}
