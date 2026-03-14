using FluentValidation;
using MediatR;
using Mro.Application.Common;

namespace Mro.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that runs FluentValidation validators
/// before the command/query handler executes.
/// Returns a VALIDATION_ERROR Result instead of throwing exceptions.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        var errorMessage = string.Join("; ", failures.Select(f => f.ErrorMessage));

        // Return Result.Failure if TResponse is a Result type; otherwise throw
        if (typeof(TResponse) == typeof(Result))
            return (TResponse)(object)Result.Failure(Error.Validation(errorMessage));

        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var error = Error.Validation(errorMessage);
            var failureMethod = typeof(Result)
                .GetMethod(nameof(Result.Failure), 1, [typeof(Error)])!
                .MakeGenericMethod(typeof(TResponse).GetGenericArguments()[0]);
            return (TResponse)failureMethod.Invoke(null, [error])!;
        }

        throw new ValidationException(failures);
    }
}
