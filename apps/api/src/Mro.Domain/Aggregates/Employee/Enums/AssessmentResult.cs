namespace Mro.Domain.Aggregates.Employee.Enums;

public enum AssessmentResult
{
    /// <summary>Employee met all required standards.</summary>
    Pass,

    /// <summary>Employee did not meet required standards; re-assessment required.</summary>
    Fail,

    /// <summary>Performance was acceptable but borderline; monitor at next review.</summary>
    Satisfactory,

    /// <summary>Performance below standard; remedial action required before re-assessment.</summary>
    Unsatisfactory,

    /// <summary>Assessment deferred; employee referred for additional training first.</summary>
    Referred,
}
