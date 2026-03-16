using Microsoft.EntityFrameworkCore;
using Mro.Api.Extensions;
using Mro.Api.Middleware;
using Mro.Infrastructure.Persistence;
using Serilog;

// ── Bootstrap Serilog ───────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ───────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, services, config) =>
    {
        config
            .ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "Mro.Api")
            .WriteTo.Console();
    });

    // ── CORS ─────────────────────────────────────────────────────────────
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? [];

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("MroFrontend", policy =>
        {
            if (allowedOrigins.Length > 0)
                policy.WithOrigins(allowedOrigins);
            else
                policy.AllowAnyOrigin();

            policy
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    // ── Services ──────────────────────────────────────────────────────────
    builder.Services.AddMroPlatform(builder.Configuration);

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "MRO Platform API",
            Version = "v1",
            Description = "Aircraft-first Maintenance, Repair & Overhaul platform API"
        });
    });

    // Authentication/authorisation (JWT) — wired in AddMroPlatform → AddJwtAuthentication

    // ── Build ─────────────────────────────────────────────────────────────
    var app = builder.Build();

    // ── Auto-migrate database on startup ─────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        Log.Information("Database migrations applied");
    }

    // ── Middleware pipeline ───────────────────────────────────────────────

    // Global exception handler — must be first
    app.UseMiddleware<ExceptionMiddleware>();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MRO Platform API v1");
        options.RoutePrefix = "swagger";
    });

    // Only redirect HTTPS in non-Railway environments (Railway handles TLS at load balancer)
    if (!app.Environment.IsProduction())
        app.UseHttpsRedirection();

    app.UseCors("MroFrontend");
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Health check endpoint
    app.MapGet("/health", () => Results.Ok(new
    {
        status = "healthy",
        timestamp = DateTimeOffset.UtcNow
    }))
    .AllowAnonymous()
    .WithName("HealthCheck");

    Log.Information("MRO Platform API starting up");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
