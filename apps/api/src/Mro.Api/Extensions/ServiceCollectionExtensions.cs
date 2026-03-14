using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mro.Application.Abstractions;
using Mro.Application.Behaviors;
using Mro.Infrastructure.Audit;
using Mro.Infrastructure.Persistence;
using Mro.Infrastructure.Security;

namespace Mro.Api.Extensions;

/// <summary>
/// Extension methods for IServiceCollection that register all platform services.
/// Called once from Program.cs to keep the entry point clean.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMroPlatform(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddDatabase(configuration)
            .AddApplicationServices()
            .AddInfrastructureServices()
            .AddHttpContextAccessor();

        return services;
    }

    // ── Database ────────────────────────────────────────────────────────────

    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException(
                "Connection string 'Postgres' is not configured.");

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                npgsql.EnableRetryOnFailure(3);
            });

            // Enable detailed errors only in development
            if (sp.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

        return services;
    }

    // ── Application ─────────────────────────────────────────────────────────

    private static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // MediatR — discovers all handlers in Mro.Application
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(
                typeof(Mro.Application.Abstractions.ICurrentUserService).Assembly);
        });

        // FluentValidation — discovers all validators in Mro.Application
        services.AddValidatorsFromAssembly(
            typeof(Mro.Application.Abstractions.ICurrentUserService).Assembly);

        // MediatR pipeline behaviors
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        return services;
    }

    // ── Infrastructure ───────────────────────────────────────────────────────

    private static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services)
    {
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<AuditInterceptor>();

        return services;
    }
}
