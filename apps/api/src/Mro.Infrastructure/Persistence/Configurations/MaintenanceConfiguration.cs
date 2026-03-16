using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mro.Domain.Aggregates.Maintenance;

namespace Mro.Infrastructure.Persistence.Configurations;

public sealed class MaintenanceProgramConfiguration : IEntityTypeConfiguration<MaintenanceProgram>
{
    public void Configure(EntityTypeBuilder<MaintenanceProgram> builder)
    {
        builder.ToTable("maintenance_programs");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.ProgramNumber).IsRequired().HasMaxLength(30);
        builder.Property(p => p.AircraftTypeCode).IsRequired().HasMaxLength(20);
        builder.Property(p => p.Title).IsRequired().HasMaxLength(200);
        builder.Property(p => p.RevisionNumber).IsRequired().HasMaxLength(20);
        builder.Property(p => p.ApprovalReference).HasMaxLength(100);

        builder.HasIndex(p => new { p.ProgramNumber, p.OrganisationId })
            .IsUnique()
            .HasDatabaseName("ix_maintenance_programs_number_org");
    }
}

public sealed class DueItemConfiguration : IEntityTypeConfiguration<DueItem>
{
    public void Configure(EntityTypeBuilder<DueItem> builder)
    {
        builder.ToTable("due_items");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.DueItemRef).IsRequired().HasMaxLength(60);
        builder.Property(d => d.Description).IsRequired().HasMaxLength(500);
        builder.Property(d => d.RegulatoryRef).HasMaxLength(200);
        builder.Property(d => d.DueItemType).HasConversion<string>().HasMaxLength(30);
        builder.Property(d => d.IntervalType).HasConversion<string>().HasMaxLength(30);
        builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.IntervalValue).HasPrecision(10, 2);
        builder.Property(d => d.ToleranceValue).HasPrecision(10, 2);
        builder.Property(d => d.NextDueHours).HasPrecision(10, 2);
        builder.Property(d => d.LastAccomplishedAtHours).HasPrecision(10, 2);

        builder.HasIndex(d => new { d.DueItemRef, d.AircraftId, d.OrganisationId })
            .IsUnique()
            .HasDatabaseName("ix_due_items_ref_aircraft");

        builder.HasIndex(d => new { d.AircraftId, d.Status })
            .HasDatabaseName("ix_due_items_aircraft_status");

        builder.Ignore(d => d.IsRecurring);
    }
}

public sealed class WorkPackageConfiguration : IEntityTypeConfiguration<WorkPackage>
{
    public void Configure(EntityTypeBuilder<WorkPackage> builder)
    {
        builder.ToTable("work_packages");
        builder.HasKey(wp => wp.Id);

        builder.Property(wp => wp.PackageNumber).IsRequired().HasMaxLength(25);
        builder.Property(wp => wp.Description).IsRequired().HasMaxLength(500);
        builder.Property(wp => wp.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(wp => new { wp.PackageNumber, wp.OrganisationId })
            .IsUnique()
            .HasDatabaseName("ix_work_packages_number_org");

        builder.HasIndex(wp => new { wp.AircraftId, wp.Status })
            .HasDatabaseName("ix_work_packages_aircraft_status");

        builder.Ignore(wp => wp.Items);
        builder.Ignore(wp => wp.AllItemsResolved);

        builder.HasMany<PackageItem>("_items")
            .WithOne()
            .HasForeignKey(i => i.WorkPackageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class PackageItemConfiguration : IEntityTypeConfiguration<PackageItem>
{
    public void Configure(EntityTypeBuilder<PackageItem> builder)
    {
        builder.ToTable("package_items");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Description).IsRequired().HasMaxLength(500);
        builder.Property(i => i.TaskReference).HasMaxLength(60);
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.EstimatedManHours).HasPrecision(6, 2);
        builder.Property(i => i.ActualManHours).HasPrecision(6, 2);
        builder.Property(i => i.DeferralReason).HasMaxLength(500);
        builder.Property(i => i.NotApplicableReason).HasMaxLength(500);
    }
}
