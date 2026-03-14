using Microsoft.EntityFrameworkCore;
using Mro.Application.Abstractions;
using Mro.Domain.Entities;
using Mro.Infrastructure.Audit;

namespace Mro.Infrastructure.Persistence;

/// <summary>
/// Single EF Core DbContext for the modular monolith.
/// Each module registers its own entity configurations via IEntityTypeConfiguration.
///
/// Direct table access from outside the owning module is prohibited.
/// Modules interact via Application layer contracts only.
/// </summary>
public sealed class AppDbContext(
    DbContextOptions<AppDbContext> options,
    ICurrentUserService currentUserService,
    AuditInterceptor auditInterceptor)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Auto-discover all IEntityTypeConfiguration implementations in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Global query filter: exclude soft-deleted records by default
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(BuildIsNotDeletedFilter(entityType.ClrType));
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(auditInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static System.Linq.Expressions.LambdaExpression BuildIsNotDeletedFilter(Type entityType)
    {
        var param = System.Linq.Expressions.Expression.Parameter(entityType, "e");
        var prop = System.Linq.Expressions.Expression.Property(param, nameof(AuditableEntity.DeletedAt));
        var isNull = System.Linq.Expressions.Expression.Equal(
            prop,
            System.Linq.Expressions.Expression.Constant(null, typeof(DateTimeOffset?)));
        return System.Linq.Expressions.Expression.Lambda(isNull, param);
    }
}
