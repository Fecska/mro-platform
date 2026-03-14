using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Aircraft.Dtos;

namespace Mro.Application.Features.Aircraft.Queries;

public sealed record GetAircraftQuery(Guid AircraftId) : IRequest<Result<AircraftDetailDto>>;

public sealed class GetAircraftQueryHandler(
    IAircraftRepository repository,
    ICurrentUserService currentUser)
    : IRequestHandler<GetAircraftQuery, Result<AircraftDetailDto>>
{
    public async Task<Result<AircraftDetailDto>> Handle(GetAircraftQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<AircraftDetailDto>(Error.Forbidden("Organisation context is required."));

        var ac = await repository.GetByIdAsync(request.AircraftId, currentUser.OrganisationId.Value, ct);
        if (ac is null)
            return Result.Failure<AircraftDetailDto>(
                Error.NotFound(nameof(Domain.Aggregates.Aircraft.Aircraft), request.AircraftId));

        return Result.Success(ToDetailDto(ac));
    }

    internal static AircraftDetailDto ToDetailDto(Domain.Aggregates.Aircraft.Aircraft ac) =>
        new(
            ac.Id,
            ac.Registration,
            ac.SerialNumber,
            ac.AircraftTypeId,
            ac.AircraftType?.IcaoTypeCode ?? string.Empty,
            ac.AircraftType?.Manufacturer ?? string.Empty,
            ac.AircraftType?.Model ?? string.Empty,
            ac.Status.ToString(),
            ac.ManufactureDate,
            ac.Remarks,
            ac.Counters.Select(c => new CounterDto(c.CounterType.ToString(), c.Value, c.LastUpdatedAt)).ToList(),
            ac.InstalledComponents.Where(c => c.IsInstalled).Select(c =>
                new InstalledComponentDto(c.Id, c.PartNumber, c.SerialNumber, c.Description,
                    c.InstallationPosition, c.InstalledAt, c.InstalledByUserId, c.InstallationWorkOrderId)).ToList(),
            ac.StatusHistory.OrderByDescending(h => h.ChangedAt).Select(h =>
                new StatusHistoryDto(h.FromStatus.ToString(), h.ToStatus.ToString(), h.Reason, h.ActorId, h.ChangedAt)).ToList()
        );
}
