namespace Mro.Domain.Aggregates.Employee.Enums;

public enum AssessmentType
{
    /// <summary>Practical skill demonstration on actual equipment or task.</summary>
    Practical,

    /// <summary>Written knowledge examination.</summary>
    Written,

    /// <summary>Verbal/oral questioning by assessor.</summary>
    Oral,

    /// <summary>Full-flight simulator or maintenance task trainer session.</summary>
    Simulator,

    /// <summary>Observation of the employee performing work on the line.</summary>
    OnTheJob,

    /// <summary>Composite assessment covering multiple methods.</summary>
    Composite,
}
