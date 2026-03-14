namespace Mro.Application.Features.Documents.Dtos;

public sealed record DocumentSummaryDto(
    Guid Id,
    string DocumentNumber,
    string DocumentType,
    string Title,
    string Issuer,
    string Status,
    string? CurrentRevisionNumber,
    DateOnly? CurrentRevisionEffectiveAt);

public sealed record DocumentDetailDto(
    Guid Id,
    string DocumentNumber,
    string DocumentType,
    string Title,
    string Issuer,
    string Status,
    string? RegulatoryReference,
    Guid? SupersedesDocumentId,
    Guid? SupersededByDocumentId,
    IReadOnlyList<RevisionDto> Revisions,
    IReadOnlyList<EffectivityDto> Effectivities);

public sealed record RevisionDto(
    Guid Id,
    string RevisionNumber,
    DateOnly IssuedAt,
    DateOnly EffectiveAt,
    long FileSizeBytes,
    string Sha256Checksum,
    bool IsCurrent,
    Guid UploadedByUserId,
    DateTimeOffset CreatedAt);

public sealed record EffectivityDto(
    Guid Id,
    string? IcaoTypeCode,
    string? SerialFrom,
    string? SerialTo,
    string? ConditionNote);
