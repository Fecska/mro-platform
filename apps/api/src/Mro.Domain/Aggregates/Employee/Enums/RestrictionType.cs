namespace Mro.Domain.Aggregates.Employee.Enums;

public enum RestrictionType
{
    /// <summary>Employee is temporarily suspended from all duties.</summary>
    TemporarySuspension,

    /// <summary>Employee may only work under direct supervision.</summary>
    SupervisedWorkOnly,

    /// <summary>Employee is restricted to a specific station/hangar.</summary>
    StationRestricted,

    /// <summary>Employee may not issue CRS/release certificates.</summary>
    NoReleasePrivilege,

    /// <summary>Any other restriction — details recorded in the Details field.</summary>
    Other,
}
