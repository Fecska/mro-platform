using Quartz;

namespace Mro.Worker.Jobs;

/// <summary>
/// Recalculates due-date flags for work orders, tasks, and scheduled maintenance items.
/// Runs every 15 minutes so the dashboard "Due Items" widget stays current without
/// requiring a full DB scan on every API request.
/// </summary>
[DisallowConcurrentExecution]
public sealed class DueItemsRecalculationJob : IJob
{
    private readonly ILogger<DueItemsRecalculationJob> _logger;

    public DueItemsRecalculationJob(ILogger<DueItemsRecalculationJob> logger)
    {
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("DueItemsRecalculationJob started at {FireTime}", context.FireTimeUtc);

        try
        {
            // TODO (Sprint 3): query work_orders + tasks where due_date <= NOW() + 7 days
            //   and status NOT IN (closed, cancelled).
            //   Update a computed column or a separate due_items_cache table.
            //   Send domain event DueItemsFlaggedEvent for the notification pipeline.

            await Task.Delay(TimeSpan.FromMilliseconds(1), context.CancellationToken);

            _logger.LogInformation("DueItemsRecalculationJob completed");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DueItemsRecalculationJob cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DueItemsRecalculationJob failed");
            throw new JobExecutionException(ex, refireImmediately: false);
        }
    }
}
