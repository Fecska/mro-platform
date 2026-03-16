using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mro.Domain.Aggregates.Employee;

namespace Mro.Infrastructure.Persistence.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EmployeeNumber).IsRequired().HasMaxLength(20);
        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Phone).HasMaxLength(30);
        builder.Property(e => e.NationalityCode).HasMaxLength(2);
        builder.Property(e => e.EmergencyContactName).HasMaxLength(100);
        builder.Property(e => e.EmergencyContactPhone).HasMaxLength(30);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(e => new { e.EmployeeNumber, e.OrganisationId })
            .IsUnique()
            .HasDatabaseName("ix_employees_number_org");

        builder.HasIndex(e => new { e.Email, e.OrganisationId })
            .HasDatabaseName("ix_employees_email_org");

        builder.Ignore(e => e.Licences);
        builder.Ignore(e => e.Authorisations);
        builder.Ignore(e => e.TrainingRecords);
        builder.Ignore(e => e.CompetencyAssessments);
        builder.Ignore(e => e.Shifts);
        builder.Ignore(e => e.Attachments);
        builder.Ignore(e => e.Restrictions);
        builder.Ignore(e => e.Notes);
        builder.Ignore(e => e.FullName);
        builder.Ignore(e => e.IsActive);
        builder.Ignore(e => e.CurrentLicences);
        builder.Ignore(e => e.ActiveAuthorisations);
        builder.Ignore(e => e.ActiveRestrictions);

        builder.HasMany<EmployeeRestriction>("_restrictions")
            .WithOne()
            .HasForeignKey(r => r.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<EmployeeNote>("_notes")
            .WithOne()
            .HasForeignKey(n => n.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<EmployeeAttachment>("_attachments")
            .WithOne()
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<Licence>("_licences")
            .WithOne()
            .HasForeignKey(l => l.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<Authorisation>("_authorisations")
            .WithOne()
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<TrainingRecord>("_trainingRecords")
            .WithOne()
            .HasForeignKey(t => t.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<CompetencyAssessment>("_competencyAssessments")
            .WithOne()
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<Shift>("_shifts")
            .WithOne()
            .HasForeignKey(s => s.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class LicenceConfiguration : IEntityTypeConfiguration<Licence>
{
    public void Configure(EntityTypeBuilder<Licence> builder)
    {
        builder.ToTable("licences");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.LicenceNumber).IsRequired().HasMaxLength(40);
        builder.Property(l => l.Category).HasConversion<string>().HasMaxLength(5);
        builder.Property(l => l.Subcategory).HasMaxLength(10);
        builder.Property(l => l.IssuingAuthority).IsRequired().HasMaxLength(50);
        builder.Property(l => l.TypeRatings).HasMaxLength(500);
        builder.Property(l => l.ScopeNotes).HasMaxLength(1000);
        builder.Property(l => l.AttachmentRef).HasMaxLength(500);

        builder.HasIndex(l => new { l.LicenceNumber, l.IssuingAuthority, l.EmployeeId })
            .IsUnique()
            .HasDatabaseName("ix_licences_number_authority_emp");

        builder.Ignore(l => l.IsExpired);
        builder.Ignore(l => l.IsCurrent);
    }
}

public sealed class AuthorisationConfiguration : IEntityTypeConfiguration<Authorisation>
{
    public void Configure(EntityTypeBuilder<Authorisation> builder)
    {
        builder.ToTable("authorisations");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AuthorisationNumber).IsRequired().HasMaxLength(40);
        builder.Property(a => a.Category).HasConversion<string>().HasMaxLength(5);
        builder.Property(a => a.Scope).IsRequired().HasMaxLength(50);
        builder.Property(a => a.AircraftTypes).HasMaxLength(500);
        builder.Property(a => a.ComponentScope).HasMaxLength(500);
        builder.Property(a => a.StationScope).HasMaxLength(200);
        builder.Property(a => a.IssuingAuthority).HasMaxLength(150);
        builder.Property(a => a.SuspensionReason).HasMaxLength(500);
        builder.Property(a => a.RevocationReason).HasMaxLength(500);

        builder.HasIndex(a => new { a.AuthorisationNumber, a.OrganisationId })
            .IsUnique()
            .HasDatabaseName("ix_authorisations_number_org");

        builder.Ignore(a => a.IsExpired);
        builder.Ignore(a => a.IsCurrent);
        builder.Ignore(a => a.Status);
    }
}

public sealed class TrainingRecordConfiguration : IEntityTypeConfiguration<TrainingRecord>
{
    public void Configure(EntityTypeBuilder<TrainingRecord> builder)
    {
        builder.ToTable("training_records");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.CourseCode).IsRequired().HasMaxLength(30);
        builder.Property(t => t.CourseName).IsRequired().HasMaxLength(150);
        builder.Property(t => t.TrainingProvider).IsRequired().HasMaxLength(100);
        builder.Property(t => t.TrainingType).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Result).HasMaxLength(50);
        builder.Property(t => t.CertificateRef).HasMaxLength(500);

        builder.Ignore(t => t.IsExpired);
        builder.Ignore(t => t.IsRecurring);
    }
}

public sealed class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.ToTable("shifts");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.ShiftType).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.AvailabilityStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.Notes).HasMaxLength(300);

        builder.HasIndex(s => new { s.EmployeeId, s.ShiftDate, s.IsActual })
            .HasDatabaseName("ix_shifts_employee_date");

        builder.Ignore(s => s.Hours);
    }
}

public sealed class CompetencyAssessmentConfiguration : IEntityTypeConfiguration<CompetencyAssessment>
{
    public void Configure(EntityTypeBuilder<CompetencyAssessment> builder)
    {
        builder.ToTable("competency_assessments");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AssessmentType).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Result).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Comments).HasMaxLength(2000);

        builder.HasIndex(a => new { a.EmployeeId, a.AssessmentDate })
            .HasDatabaseName("ix_competency_assessments_employee_date");

        builder.Ignore(a => a.IsReviewOverdue);
        builder.Ignore(a => a.IsCurrent);
    }
}

public sealed class EmployeeAttachmentConfiguration : IEntityTypeConfiguration<EmployeeAttachment>
{
    public void Configure(EntityTypeBuilder<EmployeeAttachment> builder)
    {
        builder.ToTable("employee_attachments");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AttachmentType).HasConversion<string>().HasMaxLength(30);
        builder.Property(a => a.DisplayName).IsRequired().HasMaxLength(200);
        builder.Property(a => a.StoragePath).IsRequired().HasMaxLength(500);
        builder.Property(a => a.ContentType).IsRequired().HasMaxLength(100);

        builder.HasIndex(a => new { a.EmployeeId, a.AttachmentType })
            .HasDatabaseName("ix_employee_attachments_employee_type");
    }
}

public sealed class EmployeeRestrictionConfiguration : IEntityTypeConfiguration<EmployeeRestriction>
{
    public void Configure(EntityTypeBuilder<EmployeeRestriction> builder)
    {
        builder.ToTable("employee_restrictions");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.RestrictionType).HasConversion<string>().HasMaxLength(30);
        builder.Property(r => r.Details).HasMaxLength(500);

        builder.Ignore(r => r.IsActive);

        builder.HasIndex(r => new { r.EmployeeId, r.RestrictionType })
            .HasDatabaseName("ix_employee_restrictions_employee_type");
    }
}

public sealed class EmployeeNoteConfiguration : IEntityTypeConfiguration<EmployeeNote>
{
    public void Configure(EntityTypeBuilder<EmployeeNote> builder)
    {
        builder.ToTable("employee_notes");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.NoteText).IsRequired().HasMaxLength(2000);

        builder.HasIndex(n => new { n.EmployeeId, n.CreatedAt })
            .HasDatabaseName("ix_employee_notes_employee_created");
    }
}
