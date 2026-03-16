using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mro.Domain.Aggregates.Tool;

namespace Mro.Infrastructure.Persistence.Configurations;

public sealed class ToolEntityConfiguration : IEntityTypeConfiguration<Tool>
{
    public void Configure(EntityTypeBuilder<Tool> builder)
    {
        builder.ToTable("tools");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.ToolNumber).IsRequired().HasMaxLength(40);
        builder.Property(t => t.Description).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Category).HasConversion<string>().HasMaxLength(30);
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(25);
        builder.Property(t => t.Location).HasMaxLength(100);

        builder.HasIndex(t => new { t.ToolNumber, t.OrganisationId })
            .IsUnique()
            .HasDatabaseName("ix_tools_number_org");

        builder.Ignore(t => t.CalibrationRecords);
        builder.Ignore(t => t.IsCalibrationExpired);

        builder.HasMany<CalibrationRecord>("_calibrationRecords")
            .WithOne()
            .HasForeignKey(c => c.ToolId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class CalibrationRecordConfiguration : IEntityTypeConfiguration<CalibrationRecord>
{
    public void Configure(EntityTypeBuilder<CalibrationRecord> builder)
    {
        builder.ToTable("calibration_records");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.PerformedBy).IsRequired().HasMaxLength(100);
        builder.Property(c => c.CertificateRef).HasMaxLength(60);
        builder.Property(c => c.Notes).HasMaxLength(500);
    }
}
