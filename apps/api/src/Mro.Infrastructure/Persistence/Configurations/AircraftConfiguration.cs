using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mro.Domain.Aggregates.Aircraft;
using Mro.Domain.Aggregates.Aircraft.Enums;

namespace Mro.Infrastructure.Persistence.Configurations;

public sealed class AircraftTypeConfiguration : IEntityTypeConfiguration<AircraftType>
{
    public void Configure(EntityTypeBuilder<AircraftType> builder)
    {
        builder.ToTable("aircraft_types");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.IcaoTypeCode).IsRequired().HasMaxLength(4);
        builder.Property(t => t.Manufacturer).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Model).IsRequired().HasMaxLength(80);
        builder.HasIndex(t => new { t.IcaoTypeCode, t.OrganisationId })
            .HasDatabaseName("ix_aircraft_types_code_org");
    }
}

public sealed class AircraftConfiguration : IEntityTypeConfiguration<Aircraft>
{
    public void Configure(EntityTypeBuilder<Aircraft> builder)
    {
        builder.ToTable("aircraft");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Registration).IsRequired().HasMaxLength(10);
        builder.HasIndex(a => new { a.Registration, a.OrganisationId })
            .IsUnique()
            .HasDatabaseName("ix_aircraft_registration_org");

        builder.Property(a => a.SerialNumber).IsRequired().HasMaxLength(20);
        builder.Property(a => a.Remarks).HasMaxLength(2000);

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasOne(a => a.AircraftType)
            .WithMany()
            .HasForeignKey(a => a.AircraftTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany<AircraftCounter>("_counters")
            .WithOne()
            .HasForeignKey(c => c.AircraftId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<AircraftStatusHistory>("_statusHistory")
            .WithOne()
            .HasForeignKey(h => h.AircraftId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<InstalledComponent>("_installedComponents")
            .WithOne()
            .HasForeignKey(c => c.AircraftId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class AircraftCounterConfiguration : IEntityTypeConfiguration<AircraftCounter>
{
    public void Configure(EntityTypeBuilder<AircraftCounter> builder)
    {
        builder.ToTable("aircraft_counters");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.CounterType).HasConversion<string>().HasMaxLength(30);
        builder.Property(c => c.Value).HasPrecision(12, 2);
        builder.HasIndex(c => new { c.AircraftId, c.CounterType })
            .IsUnique()
            .HasDatabaseName("ix_aircraft_counters_aircraft_type");
    }
}

public sealed class AircraftStatusHistoryConfiguration : IEntityTypeConfiguration<AircraftStatusHistory>
{
    public void Configure(EntityTypeBuilder<AircraftStatusHistory> builder)
    {
        builder.ToTable("aircraft_status_history");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.FromStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(h => h.ToStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(h => h.Reason).IsRequired().HasMaxLength(500);
    }
}

public sealed class InstalledComponentConfiguration : IEntityTypeConfiguration<InstalledComponent>
{
    public void Configure(EntityTypeBuilder<InstalledComponent> builder)
    {
        builder.ToTable("installed_components");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.PartNumber).IsRequired().HasMaxLength(50);
        builder.Property(c => c.SerialNumber).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Description).IsRequired().HasMaxLength(200);
        builder.Property(c => c.InstallationPosition).IsRequired().HasMaxLength(30);
        builder.Property(c => c.RemovalReason).HasMaxLength(500);

        // Ignore computed property
        builder.Ignore(c => c.IsInstalled);
    }
}
