namespace Mro.Domain.Aggregates.Employee.Enums;

/// <summary>
/// Classification of a document attached to an employee record.
/// </summary>
public enum AttachmentType
{
    /// <summary>Scanned copy of an aviation licence (Part-66, FAA A&P, etc.).</summary>
    Licence,

    /// <summary>Training completion or attendance certificate.</summary>
    TrainingCertificate,

    /// <summary>Authorisation or approval document issued by the organisation.</summary>
    Authorisation,

    /// <summary>Any other supporting document (e.g. medical certificate, passport copy).</summary>
    General,
}
