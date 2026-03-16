using Mro.Domain.Aggregates.Release.Enums;

namespace Mro.Application.Features.Release.Dtos;

public sealed record SignatureEventDto(
    Guid Id,
    Guid SignerUserId,
    DateTimeOffset SignedAt,
    string LicenceRef,
    SignatureMethod Method,
    string StatementText);

public sealed record CertificateSummaryDto(
    Guid Id,
    string CertificateNumber,
    CertificateType CertificateType,
    CertificateStatus Status,
    Guid WorkOrderId,
    string WorkOrderNumber,
    Guid AircraftId,
    string AircraftRegistration,
    Guid CertifyingStaffUserId,
    DateTimeOffset? IssuedAt);

public sealed record CertificateDetailDto(
    Guid Id,
    string CertificateNumber,
    CertificateType CertificateType,
    CertificateStatus Status,
    Guid WorkOrderId,
    string WorkOrderNumber,
    Guid AircraftId,
    string AircraftRegistration,
    string Scope,
    string RegulatoryBasis,
    string? LimitationsAndRemarks,
    Guid CertifyingStaffUserId,
    DateTimeOffset? IssuedAt,
    string? VoidReason,
    IReadOnlyList<SignatureEventDto> Signatures,
    DateTimeOffset CreatedAt);
