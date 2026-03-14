using Quartz;

namespace Mro.Worker.Jobs;

/// <summary>
/// Scans inventory for items approaching shelf-life or certification expiry and
/// raises alerts.  Runs once daily at 06:00 UTC so maintenance planners start
/// their day with an up-to-date alert list.
///
/// Alert thresholds (configurable via appsettings):
///   - Consumable / shelf-life:     30-day warning, 7-day critical
///   - Personnel licence / rating:  60-day warning, 14-day critical
///   - Part-8130/EASA Form 1:       no expiry concept — alert on missing certificate
/// </summary>
[DisallowConcurrentExecution]
public sealed class ExpiryAlertJob : IJob
{
    private readonly ILogger<ExpiryAlertJob> _logger;

    public ExpiryAlertJob(ILogger<ExpiryAlertJob> logger)
    {
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("ExpiryAlertJob started at {FireTime}", context.FireTimeUtc);

        try
        {
            // TODO (Sprint 4):
            //   1. Query stock_items WHERE expiry_date BETWEEN NOW() AND NOW() + 30 days
            //   2. Query personnel_ratings WHERE expiry_date BETWEEN NOW() AND NOW() + 60 days
            //   3. Upsert into alerts table (entity_type, entity_id, severity, message)
            //   4. Raise domain event ExpiryAlertsRaisedEvent → notification pipeline

            await Task.Delay(TimeSpan.FromMilliseconds(1), context.CancellationToken);

            _logger.LogInformation("ExpiryAlertJob completed");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("ExpiryAlertJob cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ExpiryAlertJob failed");
            throw new JobExecutionException(ex, refireImmediately: false);
        }
    }
}
