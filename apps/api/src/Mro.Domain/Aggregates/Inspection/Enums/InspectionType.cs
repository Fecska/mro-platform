namespace Mro.Domain.Aggregates.Inspection.Enums;

public enum InspectionType
{
    /// <summary>Independent duplicate inspection of a critical task.</summary>
    DuplicateInspection,

    /// <summary>Functional check verifying correct operation after maintenance.</summary>
    FunctionalCheck,

    /// <summary>Aircraft engine ground run.</summary>
    GroundRun,

    /// <summary>Post-maintenance flight test.</summary>
    FlightTest,

    /// <summary>Review of maintenance documentation and records.</summary>
    DocumentReview,

    /// <summary>Pre-departure airworthiness inspection.</summary>
    PreDepartureCheck,
}
