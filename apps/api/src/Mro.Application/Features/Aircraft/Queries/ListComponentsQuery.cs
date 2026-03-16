using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Aircraft.Dtos;

namespace Mro.Application.Features.Aircraft.Queries;

public sealed record ListComponentsQuery(
    Guid AircraftId,
    bool InstalledOnly = true) : IRequest<Result<IReadOnlyList<InstalledComponentDto>>>;

public sealed class ListComponentsQueryHandler(
    IAircraftRepository repository,
    ICurrentUserService currentUser)
    : IRequestHandler<ListComponentsQuery, Result<IReadOnlyList<InstalledComponentDto>>>
{
    public async Task<Result<IReadOnlyList<InstalledComponentDto>>> Handle(
        ListComponentsQuery request,
        CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<IReadOnlyList<InstalledComponentDto>>(Error.Forbidden("Organisation context is required."));

        var ac = await repository.GetByIdAsync(request.AircraftId, currentUser.OrganisationId.Value, ct);
        if (ac is null)
            return Result.Failure<IReadOnlyList<InstalledComponentDto>>(
                Error.NotFound(nameof(Domain.Aggregates.Aircraft.Aircraft), request.AircraftId));

        var components = request.InstalledOnly
            ? ac.InstalledComponents.Where(c => c.IsInstalled)
            : ac.InstalledComponents;

        var dtos = components
            .Select(c => new InstalledComponentDto(
                c.Id,
                c.PartNumber,
                c.SerialNumber,
                c.Description,
                c.InstallationPosition,
                c.InstalledAt,
                c.InstalledByUserId,
                c.InstallationWorkOrderId))
            .ToList();

        return Result.Success<IReadOnlyList<InstalledComponentDto>>(dtos);
    }
}
