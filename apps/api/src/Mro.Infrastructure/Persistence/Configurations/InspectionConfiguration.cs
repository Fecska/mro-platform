using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mro.Domain.Aggregates.Inspection;

namespace Mro.Infrastructure.Persistence.Configurations;

public sealed class InspectionConfiguration : IEntityTypeConfiguration<Inspection>
{
    public void Configure(EntityTypeBuilder<Inspection> builder)
    {
        builder.ToTable("inspections");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.InspectionNumber).IsRequired().HasMaxLength(25);
        builder.Property(i => i.InspectionType).HasConversion<string>().HasMaxLength(30);
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.Findings).HasMaxLength(2000);
        builder.Property(i => i.OutcomeRemarks).HasMaxLength(1000);
        builder.Property(i => i.WaiverReason).HasMaxLength(500);

        builder.HasIndex(i => new { i.InspectionNumber, i.OrganisationId })
            .IsUnique()
            .HasDatabaseName("ix_inspections_number_org");

        builder.HasIndex(i => new { i.WorkOrderId, i.Status })
            .HasDatabaseName("ix_inspections_wo_status");
    }
}
