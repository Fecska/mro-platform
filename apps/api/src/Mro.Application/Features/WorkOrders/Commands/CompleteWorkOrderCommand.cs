using FluentValidation;
using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.WorkOrders.Commands;

public sealed class CompleteWorkOrderCommand : IRequest<Result<Unit>>
{
    public required Guid WorkOrderId { get; init; }
    public required Guid CertifyingStaffUserId { get; init; }
}

public sealed class CompleteWorkOrderCommandValidator : AbstractValidator<CompleteWorkOrderCommand>
{
    public CompleteWorkOrderCommandValidator()
    {
        RuleFor(x => x.WorkOrderId).NotEmpty();
        RuleFor(x => x.CertifyingStaffUserId).NotEmpty();
    }
}

public sealed class CompleteWorkOrderCommandHandler(
    IWorkOrderRepository workOrders,
    IEmployeeRepository employees,
    ICurrentUserService currentUser)
    : IRequestHandler<CompleteWorkOrderCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(CompleteWorkOrderCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var orgId = currentUser.OrganisationId.Value;

        // ── HS-001 / HS-014: Certifying engineer compliance checks ───────────
        var signer = await employees.GetByUserIdAsync(request.CertifyingStaffUserId, orgId, ct);
        if (signer is not null)
        {
            // HS-001: Must have at least one current (non-expired, non-suspended) authorisation
            var currentAuths = signer.Authorisations.Where(a => a.IsCurrent).ToList();
            if (currentAuths.Count == 0)
            {
                var detail = signer.Authorisations.Any()
                    ? string.Join(", ", signer.Authorisations
                        .Where(a => a.IsActive)
                        .Select(a => a.IsSuspended
                            ? $"'{a.AuthorisationNumber}' (suspended)"
                            : $"'{a.AuthorisationNumber}' (expired {a.ValidUntil:yyyy-MM-dd})"))
                    : "no authorisations on record";
                return Result.Failure<Unit>(Error.HardStop("HS-001",
                    $"Sign-off blocked: certifying engineer has no current authorisation — {detail}."));
            }

            // HS-014: All recurring training must be current
            var expiredTraining = signer.TrainingRecords
                .Where(t => t.IsRecurring && t.IsExpired)
                .ToList();

            if (expiredTraining.Count > 0)
            {
                var courses = string.Join(", ",
                    expiredTraining.Select(t => $"'{t.CourseCode}' (expired {t.ExpiresAt:yyyy-MM-dd})"));
                return Result.Failure<Unit>(Error.HardStop("HS-014",
                    $"Sign-off blocked: certifying engineer has expired recurrent training. " +
                    $"Courses requiring renewal: {courses}."));
            }
        }

        var wo = await workOrders.GetByIdAsync(request.WorkOrderId, orgId, ct);
        if (wo is null)
            return Result.Failure<Unit>(Error.NotFound("WorkOrder", request.WorkOrderId));

        var domainResult = wo.Complete(request.CertifyingStaffUserId, currentUser.UserId!.Value);
        if (domainResult.IsFailure)
            return Result.Failure<Unit>(Error.Validation(domainResult.ErrorMessage!));

        await workOrders.UpdateAsync(wo, ct);
        return Result.Success(Unit.Value);
    }
}
