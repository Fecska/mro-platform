namespace Mro.Domain.Application;

/// <summary>
/// Minimal success/failure value returned from aggregate methods that enforce invariants.
/// Domain aggregates do not depend on the Application layer's Result type.
///
/// Application handlers check IsFailure and map to Application.Common.Error.
/// </summary>
public readonly struct DomainResult
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? ErrorMessage { get; }

    private DomainResult(bool isSuccess, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static DomainResult Ok() => new(true, null);
    public static DomainResult Failure(string message) => new(false, message);
}
