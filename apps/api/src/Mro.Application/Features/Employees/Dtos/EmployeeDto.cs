namespace Mro.Application.Features.Employees.Dtos;

public sealed record EmployeeSummaryDto(
    Guid Id,
    string EmployeeNumber,
    string FullName,
    string Email,
    string? Phone,
    string Status,
    bool IsActive,
    Guid? UserId,
    Guid? DefaultStationId,
    int LicenceCount,
    int ActiveAuthorisationCount);

public sealed record EmployeeDetailDto(
    Guid Id,
    string EmployeeNumber,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string Status,
    bool IsActive,
    DateOnly DateOfBirth,
    string? NationalityCode,
    Guid? UserId,
    Guid? DefaultStationId,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    IReadOnlyList<LicenceDto> Licences,
    IReadOnlyList<AuthorisationDto> ActiveAuthorisations,
    IReadOnlyList<TrainingRecordDto> TrainingRecords,
    IReadOnlyList<CompetencyAssessmentDto> CompetencyAssessments,
    DateTimeOffset CreatedAt);

public sealed record LicenceDto(
    Guid Id,
    string LicenceNumber,
    string Category,
    string? Subcategory,
    string IssuingAuthority,
    DateOnly IssuedAt,
    DateOnly? ExpiresAt,
    string TypeRatings,
    string? ScopeNotes,
    string? AttachmentRef,
    bool IsExpired,
    bool IsCurrent);

public sealed record AuthorisationDto(
    Guid Id,
    string AuthorisationNumber,
    string Category,
    string Scope,
    string AircraftTypes,
    string? ComponentScope,
    string? StationScope,
    string IssuingAuthority,
    DateOnly ValidFrom,
    DateOnly? ValidUntil,
    Guid IssuedByUserId,
    string Status,
    int RevisionNumber,
    string? SuspensionReason,
    bool IsExpired,
    bool IsCurrent);

public sealed record TrainingRecordDto(
    Guid Id,
    string CourseCode,
    string CourseName,
    string TrainingProvider,
    string TrainingType,
    DateOnly CompletedAt,
    DateOnly? ExpiresAt,
    string? Result,
    string? CertificateRef,
    bool IsExpired,
    bool IsRecurring);

public sealed record CompetencyAssessmentDto(
    Guid Id,
    Guid AssessorId,
    DateOnly AssessmentDate,
    string AssessmentType,
    string Result,
    string? Comments,
    DateOnly? NextReviewDate,
    bool IsCurrent,
    bool IsReviewOverdue);
