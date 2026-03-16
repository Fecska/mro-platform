using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mro.Domain.Aggregates.WorkOrder;

namespace Mro.Infrastructure.Persistence.Configurations;

public sealed class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.ToTable("work_orders");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.WoNumber).IsRequired().HasMaxLength(30);
        builder.Property(w => w.WorkOrderType).HasConversion<string>().HasMaxLength(30);
        builder.Property(w => w.Title).IsRequired().HasMaxLength(200);
        builder.Property(w => w.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(w => w.CustomerOrderRef).HasMaxLength(60);

        builder.HasIndex(w => new { w.WoNumber, w.OrganisationId })
            .IsUnique()
            .HasDatabaseName("ix_work_orders_number_org");

        builder.HasIndex(w => new { w.AircraftId, w.Status })
            .HasDatabaseName("ix_work_orders_aircraft_status");

        builder.Ignore(w => w.Tasks);
        builder.Ignore(w => w.Assignments);
        builder.Ignore(w => w.Blockers);
        builder.Ignore(w => w.ActiveBlockers);
        builder.Ignore(w => w.AllTasksSignedOff);

        builder.HasMany<WorkOrderTask>("_tasks")
            .WithOne()
            .HasForeignKey(t => t.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<WorkOrderAssignment>("_assignments")
            .WithOne()
            .HasForeignKey(a => a.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<WorkOrderBlocker>("_blockers")
            .WithOne()
            .HasForeignKey(b => b.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class WorkOrderTaskConfiguration : IEntityTypeConfiguration<WorkOrderTask>
{
    public void Configure(EntityTypeBuilder<WorkOrderTask> builder)
    {
        builder.ToTable("work_order_tasks");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TaskNumber).IsRequired().HasMaxLength(10);
        builder.Property(t => t.Title).IsRequired().HasMaxLength(200);
        builder.Property(t => t.AtaChapter).IsRequired().HasMaxLength(10);
        builder.Property(t => t.Description).IsRequired().HasMaxLength(2000);
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.RequiredLicence).HasMaxLength(20);
        builder.Property(t => t.SignOffRemark).HasMaxLength(500);
        builder.Property(t => t.EstimatedHours).HasPrecision(6, 2);

        builder.HasIndex(t => new { t.WorkOrderId, t.TaskNumber })
            .IsUnique()
            .HasDatabaseName("ix_wo_tasks_wo_number");

        builder.Ignore(t => t.LabourEntries);
        builder.Ignore(t => t.RequiredParts);
        builder.Ignore(t => t.RequiredTools);
        builder.Ignore(t => t.TotalHoursLogged);

        builder.HasMany<LabourEntry>("_labourEntries")
            .WithOne()
            .HasForeignKey(l => l.WorkOrderTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<RequiredPart>("_requiredParts")
            .WithOne()
            .HasForeignKey(p => p.WorkOrderTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<RequiredTool>("_requiredTools")
            .WithOne()
            .HasForeignKey(r => r.WorkOrderTaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class WorkOrderAssignmentConfiguration : IEntityTypeConfiguration<WorkOrderAssignment>
{
    public void Configure(EntityTypeBuilder<WorkOrderAssignment> builder)
    {
        builder.ToTable("work_order_assignments");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Role).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(a => new { a.WorkOrderId, a.UserId, a.Role })
            .IsUnique()
            .HasDatabaseName("ix_wo_assignments_wo_user_role");
    }
}

public sealed class LabourEntryConfiguration : IEntityTypeConfiguration<LabourEntry>
{
    public void Configure(EntityTypeBuilder<LabourEntry> builder)
    {
        builder.ToTable("labour_entries");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Notes).HasMaxLength(500);

        builder.Ignore(l => l.Hours);
    }
}

public sealed class RequiredPartConfiguration : IEntityTypeConfiguration<RequiredPart>
{
    public void Configure(EntityTypeBuilder<RequiredPart> builder)
    {
        builder.ToTable("required_parts");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.PartNumber).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Description).IsRequired().HasMaxLength(200);
        builder.Property(p => p.UnitOfMeasure).IsRequired().HasMaxLength(10);
        builder.Property(p => p.IssueSlipRef).HasMaxLength(30);
        builder.Property(p => p.QuantityRequired).HasPrecision(10, 3);
        builder.Property(p => p.IssuedQuantity).HasPrecision(10, 3);

        builder.Ignore(p => p.IsFullyIssued);
    }
}

public sealed class RequiredToolConfiguration : IEntityTypeConfiguration<RequiredTool>
{
    public void Configure(EntityTypeBuilder<RequiredTool> builder)
    {
        builder.ToTable("required_tools");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.ToolNumber).IsRequired().HasMaxLength(40);
        builder.Property(t => t.Description).IsRequired().HasMaxLength(200);

        builder.Ignore(t => t.IsCalibrationExpired);
    }
}

public sealed class WorkOrderBlockerConfiguration : IEntityTypeConfiguration<WorkOrderBlocker>
{
    public void Configure(EntityTypeBuilder<WorkOrderBlocker> builder)
    {
        builder.ToTable("work_order_blockers");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.BlockerType).HasConversion<string>().HasMaxLength(25);
        builder.Property(b => b.Description).IsRequired().HasMaxLength(500);
        builder.Property(b => b.ResolutionNote).HasMaxLength(500);
    }
}
