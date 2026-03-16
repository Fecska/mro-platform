using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Release.Dtos;
using Mro.Domain.Aggregates.Release.Enums;

namespace Mro.Application.Features.Release.Queries;

public sealed record ListCertificatesQuery(
    Guid? WorkOrderId,
    CertificateStatus? Status,
    int Page,
    int PageSize) : IRequest<Result<IReadOnlyList<CertificateSummaryDto>>>;

public sealed class ListCertificatesQueryHandler(
    IReleaseCertificateRepository certificates,
    ICurrentUserService currentUser)
    : IRequestHandler<ListCertificatesQuery, Result<IReadOnlyList<CertificateSummaryDto>>>
{
    public async Task<Result<IReadOnlyList<CertificateSummaryDto>>> Handle(
        ListCertificatesQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<IReadOnlyList<CertificateSummaryDto>>(
                Error.Forbidden("Organisation context is required."));

        var list = await certificates.ListAsync(
            currentUser.OrganisationId.Value, request.WorkOrderId, request.Status,
            request.Page, request.PageSize, ct);

        var dtos = list.Select(c => new CertificateSummaryDto(
            c.Id, c.CertificateNumber, c.CertificateType, c.Status,
            c.WorkOrderId, c.WorkOrderNumber, c.AircraftId, c.AircraftRegistration,
            c.CertifyingStaffUserId, c.IssuedAt))
            .ToList();

        return Result.Success<IReadOnlyList<CertificateSummaryDto>>(dtos);
    }
}
