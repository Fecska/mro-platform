namespace Mro.Domain.Aggregates.Maintenance.Enums;

public enum WorkPackageStatus
{
    /// <summary>Package is being prepared; items can be added or removed.</summary>
    Draft,

    /// <summary>Released to the hangar; items are locked for execution.</summary>
    Released,

    /// <summary>Maintenance is actively being carried out.</summary>
    InProgress,

    /// <summary>All items accomplished, deferred, or marked N/A; awaiting sign-off.</summary>
    Completed,

    /// <summary>Package fully closed with CRS / release documentation.</summary>
    Closed,

    /// <summary>Package cancelled before completion.</summary>
    Cancelled,
}
