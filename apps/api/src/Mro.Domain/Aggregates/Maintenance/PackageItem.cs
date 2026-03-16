using Mro.Domain.Aggregates.Maintenance.Enums;
using Mro.Domain.Application;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Maintenance;

/// <summary>
/// A single line item within a work package.
/// May reference a DueItem (scheduled task) or be a standalone task card.
/// </summary>
public sealed class PackageItem : AuditableEntity
{
    public Guid WorkPackageId { get; private set; }

    /// <summary>Optional link to the governed DueItem — null for ad-hoc items.</summary>
    public Guid? DueItemId { get; private set; }

    public string Description { get; private set; } = null!;

    /// <summary>AMM / task card reference (e.g. "AMM 5-20-00 TASK 01").</summary>
    public string? TaskReference { get; private set; }

    public PackageItemStatus Status { get; private set; } = PackageItemStatus.Pending;

    public decimal? EstimatedManHours { get; private set; }
    public decimal? ActualManHours { get; private set; }

    /// <summary>Work order raised to execute this item; set when accomplished.</summary>
    public Guid? RelatedWorkOrderId { get; private set; }

    public string? DeferralReason { get; private set; }
    public string? NotApplicableReason { get; private set; }

    private PackageItem() { }

    internal static PackageItem Create(
        Guid workPackageId,
        string description,
        Guid organisationId,
        Guid actorId,
        Guid? dueItemId = null,
        string? taskReference = null,
        decimal? estimatedManHours = null) => new()
    {
        WorkPackageId      = workPackageId,
        DueItemId          = dueItemId,
        Description        = description,
        TaskReference      = taskReference,
        EstimatedManHours  = estimatedManHours,
        OrganisationId     = organisationId,
        CreatedAt          = DateTimeOffset.UtcNow,
        CreatedBy          = actorId,
    };

    internal DomainResult Accomplish(Guid workOrderId, decimal? actualHours, Guid actorId)
    {
        if (Status == PackageItemStatus.Accomplished)
            return DomainResult.Failure("Item is already accomplished.");
        if (Status == PackageItemStatus.NotApplicable)
            return DomainResult.Failure("Cannot accomplish a N/A item.");

        Status              = PackageItemStatus.Accomplished;
        RelatedWorkOrderId  = workOrderId;
        ActualManHours      = actualHours;
        UpdatedAt           = DateTimeOffset.UtcNow;
        UpdatedBy           = actorId;
        return DomainResult.Ok();
    }

    internal DomainResult Defer(string reason, Guid actorId)
    {
        if (Status == PackageItemStatus.Accomplished)
            return DomainResult.Failure("Cannot defer an already accomplished item.");
        if (string.IsNullOrWhiteSpace(reason))
            return DomainResult.Failure("Deferral reason is required.");

        Status         = PackageItemStatus.Deferred;
        DeferralReason = reason;
        UpdatedAt      = DateTimeOffset.UtcNow;
        UpdatedBy      = actorId;
        return DomainResult.Ok();
    }

    internal DomainResult MarkNotApplicable(string reason, Guid actorId)
    {
        if (Status == PackageItemStatus.Accomplished)
            return DomainResult.Failure("Cannot mark an accomplished item as N/A.");
        if (string.IsNullOrWhiteSpace(reason))
            return DomainResult.Failure("Reason is required.");

        Status                = PackageItemStatus.NotApplicable;
        NotApplicableReason   = reason;
        UpdatedAt             = DateTimeOffset.UtcNow;
        UpdatedBy             = actorId;
        return DomainResult.Ok();
    }
}
