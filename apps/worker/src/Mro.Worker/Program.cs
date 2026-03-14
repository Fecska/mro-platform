using Mro.Worker.Jobs;
using Quartz;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Mro.Worker");

    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSerilog((services, cfg) => cfg
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/worker-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30));

    // Register Quartz scheduler
    // TODO (Sprint 3): switch to UsePersistentStore → UsePostgres once DB migrations are in place.
    builder.Services.AddQuartz(q =>
    {
        // Due-items recalculation — every 15 minutes
        var dueItemsKey = new JobKey(nameof(DueItemsRecalculationJob));
        q.AddJob<DueItemsRecalculationJob>(opts => opts.WithIdentity(dueItemsKey));
        q.AddTrigger(opts => opts
            .ForJob(dueItemsKey)
            .WithIdentity($"{nameof(DueItemsRecalculationJob)}-trigger")
            .WithSimpleSchedule(s => s
                .WithIntervalInMinutes(15)
                .RepeatForever()));

        // Expiry alert scan — daily at 06:00 UTC
        var expiryKey = new JobKey(nameof(ExpiryAlertJob));
        q.AddJob<ExpiryAlertJob>(opts => opts.WithIdentity(expiryKey));
        q.AddTrigger(opts => opts
            .ForJob(expiryKey)
            .WithIdentity($"{nameof(ExpiryAlertJob)}-trigger")
            .WithCronSchedule("0 0 6 * * ?"));

        // Audit archive — daily at 02:00 UTC
        var archiveKey = new JobKey(nameof(AuditArchiveJob));
        q.AddJob<AuditArchiveJob>(opts => opts.WithIdentity(archiveKey));
        q.AddTrigger(opts => opts
            .ForJob(archiveKey)
            .WithIdentity($"{nameof(AuditArchiveJob)}-trigger")
            .WithCronSchedule("0 0 2 * * ?"));
    });

    builder.Services.AddQuartzHostedService(opts =>
    {
        opts.WaitForJobsToComplete = true;
    });

    // Register jobs as transient (Quartz resolves them via DI)
    builder.Services.AddTransient<DueItemsRecalculationJob>();
    builder.Services.AddTransient<ExpiryAlertJob>();
    builder.Services.AddTransient<AuditArchiveJob>();

    var host = builder.Build();
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Worker terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

return 0;
