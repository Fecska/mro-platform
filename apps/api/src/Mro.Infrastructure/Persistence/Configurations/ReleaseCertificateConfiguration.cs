using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mro.Domain.Aggregates.Release;

namespace Mro.Infrastructure.Persistence.Configurations;

public sealed class ReleaseCertificateConfiguration : IEntityTypeConfiguration<ReleaseCertificate>
{
    public void Configure(EntityTypeBuilder<ReleaseCertificate> builder)
    {
        builder.ToTable("release_certificates");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CertificateNumber).IsRequired().HasMaxLength(30);
        builder.Property(c => c.CertificateType).HasConversion<string>().HasMaxLength(15);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.AircraftRegistration).IsRequired().HasMaxLength(20);
        builder.Property(c => c.WorkOrderNumber).IsRequired().HasMaxLength(30);
        builder.Property(c => c.Scope).IsRequired().HasMaxLength(2000);
        builder.Property(c => c.RegulatoryBasis).IsRequired().HasMaxLength(500);
        builder.Property(c => c.LimitationsAndRemarks).HasMaxLength(1000);
        builder.Property(c => c.VoidReason).HasMaxLength(500);

        builder.HasIndex(c => new { c.CertificateNumber, c.OrganisationId })
            .IsUnique()
            .HasDatabaseName("ix_release_certs_number_org");

        builder.HasIndex(c => new { c.WorkOrderId, c.Status })
            .HasDatabaseName("ix_release_certs_wo_status");

        builder.Ignore(c => c.Signatures);

        builder.HasMany<SignatureEvent>("_signatures")
            .WithOne()
            .HasForeignKey(s => s.CertificateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class SignatureEventConfiguration : IEntityTypeConfiguration<SignatureEvent>
{
    public void Configure(EntityTypeBuilder<SignatureEvent> builder)
    {
        builder.ToTable("signature_events");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.LicenceRef).IsRequired().HasMaxLength(30);
        builder.Property(s => s.Method).HasConversion<string>().HasMaxLength(15);
        builder.Property(s => s.StatementText).IsRequired().HasMaxLength(1000);
        builder.Property(s => s.IpAddress).HasMaxLength(45);
    }
}
