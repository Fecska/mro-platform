using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mro.Domain.Aggregates.Defect;

namespace Mro.Infrastructure.Persistence.Configurations;

public sealed class DefectConfiguration : IEntityTypeConfiguration<Domain.Aggregates.Defect.Defect>
{
    public void Configure(EntityTypeBuilder<Domain.Aggregates.Defect.Defect> builder)
    {
        builder.ToTable("defects");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.DefectNumber).IsRequired().HasMaxLength(30);
        builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(d => d.Severity).HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.Source).HasConversion<string>().HasMaxLength(30);
        builder.Property(d => d.AtaChapter).IsRequired().HasMaxLength(10);
        builder.Property(d => d.Description).IsRequired().HasMaxLength(2000);
        builder.Property(d => d.ClosureReason).HasMaxLength(500);

        builder.HasIndex(d => new { d.DefectNumber, d.OrganisationId })
            .IsUnique()
            .HasDatabaseName("ix_defects_number_org");

        builder.HasIndex(d => new { d.AircraftId, d.Status })
            .HasDatabaseName("ix_defects_aircraft_status");

        builder.Ignore(d => d.Actions);
        builder.Ignore(d => d.Deferrals);
        builder.Ignore(d => d.ActiveDeferral);

        builder.HasMany<DefectAction>("_actions")
            .WithOne()
            .HasForeignKey(a => a.DefectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<DeferredDefect>("_deferrals")
            .WithOne()
            .HasForeignKey(d => d.DefectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class DefectActionConfiguration : IEntityTypeConfiguration<DefectAction>
{
    public void Configure(EntityTypeBuilder<DefectAction> builder)
    {
        builder.ToTable("defect_actions");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.ActionType).HasConversion<string>().HasMaxLength(30);
        builder.Property(a => a.Description).IsRequired().HasMaxLength(2000);
        builder.Property(a => a.AtaReference).HasMaxLength(20);
        builder.Property(a => a.PartNumber).HasMaxLength(50);
        builder.Property(a => a.SerialNumber).HasMaxLength(50);
    }
}

public sealed class DeferredDefectConfiguration : IEntityTypeConfiguration<DeferredDefect>
{
    public void Configure(EntityTypeBuilder<DeferredDefect> builder)
    {
        builder.ToTable("deferred_defects");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.LogReference).HasMaxLength(60);
        builder.Property(d => d.RevocationReason).HasMaxLength(500);

        builder.Ignore(d => d.IsExpired);

        builder.HasOne<MelReference>("_melReference")
            .WithOne()
            .HasForeignKey<MelReference>(m => m.DeferredDefectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class MelReferenceConfiguration : IEntityTypeConfiguration<MelReference>
{
    public void Configure(EntityTypeBuilder<MelReference> builder)
    {
        builder.ToTable("mel_references");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.ItemNumber).IsRequired().HasMaxLength(30);
        builder.Property(m => m.MelRevision).IsRequired().HasMaxLength(20);
        builder.Property(m => m.Category).HasConversion<string>().HasMaxLength(25);
        builder.Property(m => m.OperationalLimitations).HasMaxLength(1000);
        builder.Property(m => m.MaintenanceProcedures).HasMaxLength(1000);

        builder.Ignore(m => m.MaxDeferralDays);
    }
}
