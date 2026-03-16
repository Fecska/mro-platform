using Mro.Domain.Aggregates.Employee.Enums;
using Mro.Domain.Aggregates.Employee.Events;
using Mro.Domain.Application;
using Mro.Domain.Common.Audit;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Employee;

/// <summary>
/// Aggregate root representing a maintenance personnel record.
///
/// Invariants:
///   - EmployeeNumber is unique within the organisation.
///   - Each employee has at most one active authorisation per Category+Scope combination (HS-011).
///   - A new authorisation requires a current (non-expired) licence of equal or broader category.
///   - Terminated employees cannot receive new licences or authorisations.
/// </summary>
public sealed class Employee : AuditableEntity
{
    /// <summary>
    /// Internal HR / technical log reference number (e.g. "EMP-0042").
    /// Unique per organisation.
    /// </summary>
    public string EmployeeNumber { get; private set; } = string.Empty;

    /// <summary>Cross-module link to the system User account, if the employee has platform access.</summary>
    public Guid? UserId { get; private set; }

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>Work email address (may differ from the login email in User).</summary>
    public string Email { get; private set; } = string.Empty;

    public string? Phone { get; private set; }

    public DateOnly DateOfBirth { get; private set; }

    /// <summary>ISO 3166-1 alpha-2 country code (e.g. "DE", "GB", "AE").</summary>
    public string? NationalityCode { get; private set; }

    /// <summary>Default hangar/station where this employee is normally based.</summary>
    public Guid? DefaultStationId { get; private set; }

    public string? EmergencyContactName { get; private set; }

    public string? EmergencyContactPhone { get; private set; }

    public EmployeeStatus Status { get; private set; } = EmployeeStatus.Active;

    /// <summary>Convenience flag — true when Status is Active.</summary>
    public bool IsActive => Status == EmployeeStatus.Active;

    private readonly List<Licence> _licences = [];
    private readonly List<Authorisation> _authorisations = [];
    private readonly List<TrainingRecord> _trainingRecords = [];
    private readonly List<CompetencyAssessment> _competencyAssessments = [];
    private readonly List<Shift> _shifts = [];
    private readonly List<EmployeeAttachment> _attachments = [];
    private readonly List<EmployeeRestriction> _restrictions = [];
    private readonly List<EmployeeNote> _notes = [];

    public IReadOnlyCollection<Licence> Licences => _licences.AsReadOnly();
    public IReadOnlyCollection<Authorisation> Authorisations => _authorisations.AsReadOnly();
    public IReadOnlyCollection<TrainingRecord> TrainingRecords => _trainingRecords.AsReadOnly();
    public IReadOnlyCollection<CompetencyAssessment> CompetencyAssessments => _competencyAssessments.AsReadOnly();
    public IReadOnlyCollection<Shift> Shifts => _shifts.AsReadOnly();
    public IReadOnlyCollection<EmployeeAttachment> Attachments => _attachments.AsReadOnly();
    public IReadOnlyCollection<EmployeeRestriction> Restrictions => _restrictions.AsReadOnly();
    public IReadOnlyCollection<EmployeeNote> Notes => _notes.AsReadOnly();

    public IReadOnlyCollection<EmployeeRestriction> ActiveRestrictions =>
        _restrictions.Where(r => r.IsActive).ToList().AsReadOnly();

    public IReadOnlyCollection<Licence> CurrentLicences =>
        _licences.Where(l => l.IsCurrent).ToList().AsReadOnly();

    public IReadOnlyCollection<Authorisation> ActiveAuthorisations =>
        _authorisations.Where(a => a.IsCurrent).ToList().AsReadOnly();

    // EF Core
    private Employee() { }

    // ── Factory ──────────────────────────────────────────────────────────────

    public static Employee Create(
        string employeeNumber,
        string firstName,
        string lastName,
        string email,
        DateOnly dateOfBirth,
        Guid organisationId,
        Guid actorId,
        Guid? userId = null,
        string? phone = null,
        string? nationalityCode = null,
        Guid? defaultStationId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(employeeNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var emp = new Employee
        {
            EmployeeNumber = employeeNumber.Trim().ToUpperInvariant(),
            UserId = userId,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Phone = phone?.Trim(),
            DateOfBirth = dateOfBirth,
            NationalityCode = nationalityCode?.Trim().ToUpperInvariant(),
            DefaultStationId = defaultStationId,
            OrganisationId = organisationId,
            CreatedBy = actorId,
            UpdatedBy = actorId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        emp.RaiseDomainEvent(new EmployeeCreatedEvent
        {
            ActorId = actorId,
            OrganisationId = organisationId,
            EntityType = nameof(Employee),
            EntityId = emp.Id,
            EventType = ComplianceEventType.RecordCreated,
            EmployeeNumber = emp.EmployeeNumber,
            FullName = emp.FullName,
            Description = $"Employee record '{emp.EmployeeNumber}' created: {emp.FullName}.",
        });

        return emp;
    }

    // ── Update details ────────────────────────────────────────────────────────

    public DomainResult UpdateDetails(
        string? firstName,
        string? lastName,
        string? email,
        string? phone,
        Guid? defaultStationId,
        bool clearDefaultStation,
        string? nationalityCode,
        string? emergencyContactName,
        string? emergencyContactPhone,
        Guid actorId)
    {
        if (Status == EmployeeStatus.Terminated)
            return DomainResult.Failure("Cannot update a terminated employee.");

        if (firstName is not null) FirstName = firstName.Trim();
        if (lastName is not null) LastName = lastName.Trim();
        if (email is not null) Email = email.Trim().ToLowerInvariant();
        if (phone is not null) Phone = phone.Trim();
        if (clearDefaultStation) DefaultStationId = null;
        else if (defaultStationId.HasValue) DefaultStationId = defaultStationId;
        if (nationalityCode is not null) NationalityCode = nationalityCode.Trim().ToUpperInvariant();
        if (emergencyContactName is not null) EmergencyContactName = emergencyContactName.Trim();
        if (emergencyContactPhone is not null) EmergencyContactPhone = emergencyContactPhone.Trim();

        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }

    // ── Status transitions ────────────────────────────────────────────────────

    public DomainResult SetStatus(EmployeeStatus newStatus, Guid actorId)
    {
        if (Status == EmployeeStatus.Terminated)
            return DomainResult.Failure("Terminated employees cannot change status.");

        Status = newStatus;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }

    public DomainResult LinkUserAccount(Guid userId, Guid actorId)
    {
        if (UserId.HasValue)
            return DomainResult.Failure(
                "Employee is already linked to a user account. Unlink first.");

        UserId = userId;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }

    // ── Licences ──────────────────────────────────────────────────────────────

    public DomainResult AddLicence(
        string licenceNumber,
        LicenceCategory category,
        string issuingAuthority,
        DateOnly issuedAt,
        Guid actorId,
        string? subcategory = null,
        DateOnly? expiresAt = null,
        string? typeRatings = null,
        string? scopeNotes = null,
        string? attachmentRef = null)
    {
        if (Status == EmployeeStatus.Terminated)
            return DomainResult.Failure("Cannot add licences to a terminated employee.");

        var duplicate = _licences.FirstOrDefault(
            l => l.LicenceNumber == licenceNumber.Trim().ToUpperInvariant()
              && l.IssuingAuthority.Equals(issuingAuthority.Trim(), StringComparison.OrdinalIgnoreCase));

        if (duplicate is not null)
            return DomainResult.Failure(
                $"Licence '{licenceNumber}' issued by '{issuingAuthority}' already exists for this employee.");

        var licence = Licence.Create(
            Id, licenceNumber, category, issuingAuthority,
            issuedAt, OrganisationId, actorId,
            subcategory, expiresAt, typeRatings, scopeNotes, attachmentRef);

        _licences.Add(licence);
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new LicenceAddedEvent
        {
            ActorId = actorId,
            OrganisationId = OrganisationId,
            EntityType = nameof(Employee),
            EntityId = Id,
            EventType = ComplianceEventType.LicenceAdded,
            EmployeeNumber = EmployeeNumber,
            LicenceNumber = licence.LicenceNumber,
            Category = category,
            IssuingAuthority = issuingAuthority,
            ExpiresAt = expiresAt,
            Description = $"Licence '{licence.LicenceNumber}' ({category}) added to employee '{EmployeeNumber}'.",
        });

        return DomainResult.Ok();
    }

    public DomainResult AddTypeRating(Guid licenceId, string icaoTypeCode, Guid actorId)
    {
        var licence = _licences.FirstOrDefault(l => l.Id == licenceId);
        if (licence is null)
            return DomainResult.Failure($"Licence {licenceId} not found.");

        licence.AddTypeRating(icaoTypeCode, actorId);
        return DomainResult.Ok();
    }

    public DomainResult RevalidateLicence(Guid licenceId, DateOnly newExpiresAt, Guid actorId)
    {
        var licence = _licences.FirstOrDefault(l => l.Id == licenceId);
        if (licence is null)
            return DomainResult.Failure($"Licence {licenceId} not found.");

        licence.Revalidate(newExpiresAt, actorId);
        return DomainResult.Ok();
    }

    // ── Authorisations ────────────────────────────────────────────────────────

    /// <summary>
    /// Grants an organisation authorisation based on an existing licence.
    ///
    /// Hard Stop HS-011:
    ///   The employee must not already hold an active authorisation for the same Category+Scope.
    ///   The underlying licence must be current (non-expired).
    /// </summary>
    public DomainResult GrantAuthorisation(
        string authorisationNumber,
        LicenceCategory category,
        string scope,
        Guid issuingLicenceId,
        Guid issuedByUserId,
        DateOnly validFrom,
        Guid actorId,
        string? aircraftTypes = null,
        DateOnly? validUntil = null,
        string? componentScope = null,
        string? stationScope = null,
        string? issuingAuthority = null)
    {
        if (Status == EmployeeStatus.Terminated)
            return DomainResult.Failure("Cannot grant authorisations to a terminated employee.");

        if (Status == EmployeeStatus.Suspended)
            return DomainResult.Failure("Cannot grant authorisations to a suspended employee.");

        // HS-011: no duplicate active authorisation for same scope
        var conflict = _authorisations.FirstOrDefault(
            a => a.Category == category
              && a.Scope.Equals(scope.Trim(), StringComparison.OrdinalIgnoreCase)
              && a.IsCurrent);

        if (conflict is not null)
            return DomainResult.Failure(
                $"Hard Stop HS-011: Employee '{EmployeeNumber}' already holds an active authorisation " +
                $"for scope '{scope}' (authorisation '{conflict.AuthorisationNumber}'). " +
                "Revoke it before issuing a new one.");

        // Underlying licence must exist and be current
        var licence = _licences.FirstOrDefault(l => l.Id == issuingLicenceId);
        if (licence is null)
            return DomainResult.Failure(
                $"Licence {issuingLicenceId} not found for employee '{EmployeeNumber}'.");

        if (licence.IsExpired)
            return DomainResult.Failure(
                $"Underlying licence '{licence.LicenceNumber}' expired on {licence.ExpiresAt}. " +
                "Revalidate the licence before granting a new authorisation.");

        var auth = Authorisation.Create(
            Id, authorisationNumber, category, scope,
            issuingLicenceId, issuedByUserId, validFrom,
            OrganisationId, actorId, aircraftTypes, validUntil,
            componentScope, stationScope, issuingAuthority);

        _authorisations.Add(auth);
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new AuthorisationGrantedEvent
        {
            ActorId = actorId,
            OrganisationId = OrganisationId,
            EntityType = nameof(Employee),
            EntityId = Id,
            EventType = ComplianceEventType.AuthorisationGranted,
            EmployeeNumber = EmployeeNumber,
            AuthorisationNumber = auth.AuthorisationNumber,
            Category = category,
            Scope = scope,
            ValidFrom = validFrom,
            Description = $"Authorisation '{auth.AuthorisationNumber}' ({scope}) granted to employee '{EmployeeNumber}' from {validFrom:yyyy-MM-dd}.",
        });

        return DomainResult.Ok();
    }

    public DomainResult RevokeAuthorisation(Guid authorisationId, string reason, Guid revokedByUserId, Guid actorId)
    {
        var auth = _authorisations.FirstOrDefault(a => a.Id == authorisationId);
        if (auth is null)
            return DomainResult.Failure($"Authorisation {authorisationId} not found.");

        var result = auth.Revoke(reason, revokedByUserId, actorId);
        if (result.IsFailure) return result;

        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new AuthorisationRevokedEvent
        {
            ActorId = actorId,
            OrganisationId = OrganisationId,
            EntityType = nameof(Employee),
            EntityId = Id,
            EventType = ComplianceEventType.AuthorisationRevoked,
            EmployeeNumber = EmployeeNumber,
            AuthorisationNumber = auth.AuthorisationNumber,
            Reason = reason,
            Description = $"Authorisation '{auth.AuthorisationNumber}' revoked for employee '{EmployeeNumber}': {reason}.",
        });

        return DomainResult.Ok();
    }

    // ── Training records ──────────────────────────────────────────────────────

    public DomainResult RecordTraining(
        string courseCode,
        string courseName,
        string trainingProvider,
        TrainingType trainingType,
        DateOnly completedAt,
        Guid actorId,
        DateOnly? expiresAt = null,
        string? result = null,
        string? certificateRef = null)
    {
        if (Status == EmployeeStatus.Terminated)
            return DomainResult.Failure("Cannot add training records to a terminated employee.");

        var record = TrainingRecord.Create(
            Id, courseCode, courseName, trainingProvider,
            trainingType, completedAt, OrganisationId, actorId,
            expiresAt, result, certificateRef);

        _trainingRecords.Add(record);
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }

    // ── Competency assessments ────────────────────────────────────────────────

    public DomainResult RecordAssessment(
        Guid assessorId,
        DateOnly assessmentDate,
        AssessmentType assessmentType,
        AssessmentResult assessmentResult,
        Guid actorId,
        string? comments = null,
        DateOnly? nextReviewDate = null)
    {
        if (Status == EmployeeStatus.Terminated)
            return DomainResult.Failure("Cannot add assessments to a terminated employee.");

        var assessment = CompetencyAssessment.Create(
            Id, assessorId, assessmentDate, assessmentType,
            assessmentResult, OrganisationId, actorId, comments, nextReviewDate);

        _competencyAssessments.Add(assessment);
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }

    // ── Shifts ────────────────────────────────────────────────────────────────

    public DomainResult RecordShift(
        DateOnly shiftDate,
        TimeOnly startTime,
        TimeOnly endTime,
        ShiftType shiftType,
        Guid actorId,
        AvailabilityStatus availabilityStatus = AvailabilityStatus.Available,
        bool isActual = false,
        Guid? stationId = null,
        string? notes = null)
    {
        if (endTime <= startTime)
            return DomainResult.Failure("Shift end time must be after start time.");

        var newStart = shiftDate.ToDateTime(startTime);
        var newEnd   = shiftDate.ToDateTime(endTime);

        var conflict = _shifts.FirstOrDefault(s => s.OverlapsWith(newStart, newEnd));
        if (conflict is not null)
            return DomainResult.Failure(
                $"Shift overlap: the proposed shift ({shiftDate:yyyy-MM-dd} {startTime:HH:mm}–{endTime:HH:mm}) " +
                $"conflicts with an existing shift ({conflict.ShiftDate:yyyy-MM-dd} " +
                $"{conflict.StartTime:HH:mm}–{conflict.EndTime:HH:mm}).");

        try
        {
            var shift = Shift.Create(
                Id, shiftDate, startTime, endTime,
                shiftType, availabilityStatus, isActual,
                OrganisationId, actorId, stationId, notes);
            _shifts.Add(shift);
        }
        catch (ArgumentException ex)
        {
            return DomainResult.Failure(ex.Message);
        }

        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }

    // ── Attachments ───────────────────────────────────────────────────────────

    public DomainResult AddAttachment(
        AttachmentType attachmentType,
        string displayName,
        string storagePath,
        long fileSizeBytes,
        string contentType,
        Guid actorId,
        Guid? linkedEntityId = null)
    {
        if (Status == EmployeeStatus.Terminated)
            return DomainResult.Failure("Cannot add attachments to a terminated employee.");

        var attachment = EmployeeAttachment.Create(
            Id, attachmentType, displayName, storagePath,
            fileSizeBytes, contentType, OrganisationId, actorId, linkedEntityId);

        _attachments.Add(attachment);
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }

    public DomainResult RemoveAttachment(Guid attachmentId, Guid actorId)
    {
        var att = _attachments.FirstOrDefault(a => a.Id == attachmentId && !a.IsDeleted);
        if (att is null)
            return DomainResult.Failure($"Attachment '{attachmentId}' not found.");

        att.SoftDelete(actorId);
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }

    // ── Restrictions ──────────────────────────────────────────────────────────

    public DomainResult AddRestriction(
        RestrictionType restrictionType,
        Guid raisedByUserId,
        DateOnly activeFrom,
        Guid actorId,
        string? details = null,
        Guid? stationId = null,
        DateOnly? activeUntil = null)
    {
        if (Status == EmployeeStatus.Terminated)
            return DomainResult.Failure("Cannot add restrictions to a terminated employee.");

        if (activeUntil.HasValue && activeUntil.Value < activeFrom)
            return DomainResult.Failure("ActiveUntil must be on or after ActiveFrom.");

        var restriction = EmployeeRestriction.Create(
            Id, restrictionType, raisedByUserId, activeFrom,
            OrganisationId, actorId, details, stationId, activeUntil);

        _restrictions.Add(restriction);
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }

    public DomainResult LiftRestriction(Guid restrictionId, Guid actorId)
    {
        var restriction = _restrictions.FirstOrDefault(r => r.Id == restrictionId && r.IsActive);
        if (restriction is null)
            return DomainResult.Failure($"Active restriction '{restrictionId}' not found.");

        restriction.Lift(actorId);
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }

    // ── Notes ─────────────────────────────────────────────────────────────────

    public DomainResult AddNote(string noteText, bool isConfidential, Guid actorId)
    {
        if (string.IsNullOrWhiteSpace(noteText))
            return DomainResult.Failure("Note text cannot be empty.");

        if (noteText.Length > 2000)
            return DomainResult.Failure("Note text cannot exceed 2000 characters.");

        var note = EmployeeNote.Create(Id, noteText, isConfidential, OrganisationId, actorId);
        _notes.Add(note);
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }

    public DomainResult DeleteNote(Guid noteId, Guid actorId)
    {
        var note = _notes.FirstOrDefault(n => n.Id == noteId && !n.IsDeleted);
        if (note is null)
            return DomainResult.Failure($"Note '{noteId}' not found.");

        note.SoftDelete(actorId);
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }
}
