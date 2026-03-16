using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that enforces permission checks before
/// the command/query handler executes.
///
/// Checks <see cref="PermissionRequirements"/> for the incoming request type.
/// If a permission is required and the current user lacks it,
/// returns a FORBIDDEN Result instead of calling the handler.
///
/// Runs after ValidationBehavior so that format/input errors are reported
/// before permission errors (fail fast on bad input).
/// </summary>
public sealed class PermissionBehaviour<TRequest, TResponse>(
    ICurrentUserService currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var required = PermissionRequirements.For(typeof(TRequest));
        if (required is null)
            return await next();

        if (!currentUser.HasPermission(required.Value))
        {
            var error = Error.Forbidden($"Permission '{required.Value.Code}' is required.");
            return CreateFailure(error);
        }

        return await next();
    }

    private static TResponse CreateFailure(Error error)
    {
        if (typeof(TResponse) == typeof(Result))
            return (TResponse)(object)Result.Failure(error);

        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var failureMethod = typeof(Result)
                .GetMethod(nameof(Result.Failure), 1, [typeof(Error)])!
                .MakeGenericMethod(typeof(TResponse).GetGenericArguments()[0]);
            return (TResponse)failureMethod.Invoke(null, [error])!;
        }

        // Non-Result return type — throw so the caller gets an HTTP 403
        throw new UnauthorizedAccessException(error.Message);
    }
}
