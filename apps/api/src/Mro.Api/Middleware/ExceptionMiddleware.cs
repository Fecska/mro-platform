using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Mro.Api.Middleware;

/// <summary>
/// Global exception handler middleware.
/// Catches all unhandled exceptions and returns a structured JSON error response.
///
/// Expected errors (validation, hard stops, state violations) are handled
/// via Result pattern in Application layer and never reach this middleware.
/// This middleware only handles unexpected infrastructure failures.
/// </summary>
public sealed class ExceptionMiddleware(
    RequestDelegate next,
    ILogger<ExceptionMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Unhandled exception on {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await WriteErrorResponseAsync(context, ex);
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var response = new
        {
            error = new
            {
                code = "INTERNAL_ERROR",
                message = "An unexpected error occurred. Please try again or contact support.",
                // Only expose detail in development
                detail = context.RequestServices
                    .GetRequiredService<IWebHostEnvironment>()
                    .IsDevelopment()
                    ? ex.Message
                    : null
            }
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonOptions));
    }
}
