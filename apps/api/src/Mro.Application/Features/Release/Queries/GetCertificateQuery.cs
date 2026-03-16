using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Release.Dtos;

namespace Mro.Application.Features.Release.Queries;

public sealed record GetCertificateQuery(Guid Id) : IRequest<Result<CertificateDetailDto>>;

public sealed class GetCertificateQueryHandler(
    IReleaseCertificateRepository certificates,
    ICurrentUserService currentUser)
    : IRequestHandler<GetCertificateQuery, Result<CertificateDetailDto>>
{
    public async Task<Result<CertificateDetailDto>> Handle(GetCertificateQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<CertificateDetailDto>(Error.Forbidden("Organisation context is required."));

        var c = await certificates.GetByIdAsync(request.Id, currentUser.OrganisationId.Value, ct);
        if (c is null)
            return Result.Failure<CertificateDetailDto>(Error.NotFound("ReleaseCertificate", request.Id));

        var signatures = c.Signatures
            .Select(s => new SignatureEventDto(
                s.Id, s.SignerUserId, s.SignedAt, s.LicenceRef, s.Method, s.StatementText))
            .ToList();

        return Result.Success(new CertificateDetailDto(
            c.Id, c.CertificateNumber, c.CertificateType, c.Status,
            c.WorkOrderId, c.WorkOrderNumber, c.AircraftId, c.AircraftRegistration,
            c.Scope, c.RegulatoryBasis, c.LimitationsAndRemarks,
            c.CertifyingStaffUserId, c.IssuedAt, c.VoidReason,
            signatures, c.CreatedAt));
    }
}
