using Mro.Api.Extensions;
using Mro.Api.Middleware;
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

    // Authentication/authorisation (JWT) — configured in Sprint 1 auth module
    builder.Services.AddAuthentication();
    builder.Services.AddAuthorization();

    // ── Build ─────────────────────────────────────────────────────────────
    var app = builder.Build();

    // ── Middleware pipeline ───────────────────────────────────────────────

    // Global exception handler — must be first
    app.UseMiddleware<ExceptionMiddleware>();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "MRO Platform API v1");
        });
    }

    app.UseHttpsRedirection();
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
