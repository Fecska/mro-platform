namespace Mro.Application.Common;

/// <summary>
/// Structured error returned inside Result.
/// Every error has a machine-readable code (maps to API error codes in api-standards.md)
/// and a human-readable message.
/// </summary>
public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    // ── Validation ─────────────────────────────────────────────────────────
    public static Error Validation(string message) =>
        new("VALIDATION_ERROR", message);

    // ── Not found ──────────────────────────────────────────────────────────
    public static Error NotFound(string entityType, Guid id) =>
        new("NOT_FOUND", $"{entityType} with id '{id}' was not found.");

    // ── Forbidden / unauthorised ────────────────────────────────────────────
    public static Error Forbidden(string reason) =>
        new("FORBIDDEN", reason);

    // ── State machine ──────────────────────────────────────────────────────
    public static Error InvalidTransition(string from, string to) =>
        new("STATE_TRANSITION_INVALID",
            $"Transition from '{from}' to '{to}' is not permitted.");

    // ── Hard stops (compliance-critical blocks) ─────────────────────────────
    /// <param name="hardStopId">Reference from hard-stop-rules.md, e.g. "HS-001"</param>
    public static Error HardStop(string hardStopId, string message) =>
        new("HARD_STOP", $"{hardStopId}: {message}");

    // ── Conflict ───────────────────────────────────────────────────────────
    public static Error Conflict(string message) =>
        new("CONFLICT", message);
}
