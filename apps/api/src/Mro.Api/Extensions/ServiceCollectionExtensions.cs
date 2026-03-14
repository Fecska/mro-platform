using System.Text;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Mro.Application.Abstractions;
using Mro.Application.Behaviors;
using Mro.Infrastructure.Audit;
using Mro.Infrastructure.Persistence;
using Mro.Infrastructure.Persistence.Repositories;
using Mro.Infrastructure.Security;
using Mro.Infrastructure.Storage;

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
            .AddJwtAuthentication(configuration)
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
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAircraftRepository, AircraftRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IDocumentStorageService, S3DocumentStorageService>();
        services.AddAWSService<Amazon.S3.IAmazonS3>();
        services.AddScoped<AuditInterceptor>();

        return services;
    }

    // ── JWT authentication ───────────────────────────────────────────────────

    private static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var secretKey = configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");
        var issuer    = configuration["Jwt:Issuer"]   ?? "mro-platform";
        var audience  = configuration["Jwt:Audience"] ?? "mro-platform-api";

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = new SymmetricSecurityKey(
                                                  Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer   = true,
                    ValidIssuer      = issuer,
                    ValidateAudience = true,
                    ValidAudience    = audience,
                    ValidateLifetime = true,
                    ClockSkew        = TimeSpan.FromSeconds(30),
                    RoleClaimType    = System.Security.Claims.ClaimTypes.Role,
                };
            });

        services.AddAuthorization();

        return services;
    }
}
