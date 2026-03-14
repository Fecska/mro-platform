using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mro.Domain.Aggregates.Document;

namespace Mro.Infrastructure.Persistence.Configurations;

public sealed class MaintenanceDocumentConfiguration : IEntityTypeConfiguration<MaintenanceDocument>
{
    public void Configure(EntityTypeBuilder<MaintenanceDocument> builder)
    {
        builder.ToTable("maintenance_documents");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.DocumentNumber).IsRequired().HasMaxLength(80);
        builder.Property(d => d.DocumentType).HasConversion<string>().HasMaxLength(30);
        builder.Property(d => d.Title).IsRequired().HasMaxLength(250);
        builder.Property(d => d.Issuer).IsRequired().HasMaxLength(150);
        builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.RegulatoryReference).HasMaxLength(100);

        builder.HasIndex(d => new { d.DocumentNumber, d.DocumentType, d.OrganisationId })
            .IsUnique()
            .HasDatabaseName("ix_docs_number_type_org");

        builder.HasMany<DocumentRevision>("_revisions")
            .WithOne()
            .HasForeignKey(r => r.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<DocumentEffectivity>("_effectivities")
            .WithOne()
            .HasForeignKey(e => e.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<TaskDocumentLink>("_taskLinks")
            .WithOne()
            .HasForeignKey(l => l.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(d => d.CurrentRevision);
    }
}

public sealed class DocumentRevisionConfiguration : IEntityTypeConfiguration<DocumentRevision>
{
    public void Configure(EntityTypeBuilder<DocumentRevision> builder)
    {
        builder.ToTable("document_revisions");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.RevisionNumber).IsRequired().HasMaxLength(30);
        builder.Property(r => r.StoragePath).IsRequired().HasMaxLength(500);
        builder.Property(r => r.Sha256Checksum).IsRequired().HasMaxLength(64);

        builder.HasIndex(r => new { r.DocumentId, r.IsCurrent })
            .HasDatabaseName("ix_doc_revisions_current");
    }
}

public sealed class DocumentEffectivityConfiguration : IEntityTypeConfiguration<DocumentEffectivity>
{
    public void Configure(EntityTypeBuilder<DocumentEffectivity> builder)
    {
        builder.ToTable("document_effectivities");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.IcaoTypeCode).HasMaxLength(4);
        builder.Property(e => e.SerialFrom).HasMaxLength(20);
        builder.Property(e => e.SerialTo).HasMaxLength(20);
        builder.Property(e => e.ConditionNote).HasMaxLength(300);

        // Ignore computed method
        builder.Ignore("Covers");
    }
}

public sealed class TaskDocumentLinkConfiguration : IEntityTypeConfiguration<TaskDocumentLink>
{
    public void Configure(EntityTypeBuilder<TaskDocumentLink> builder)
    {
        builder.ToTable("task_document_links");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.LinkType).HasConversion<string>().HasMaxLength(20);
        builder.Property(l => l.AtaReference).HasMaxLength(20);

        builder.HasIndex(l => new { l.DocumentId, l.TaskId })
            .IsUnique()
            .HasDatabaseName("ix_task_doc_links_doc_task");
    }
}
