using Quartz;

namespace Mro.Worker.Jobs;

/// <summary>
/// Archives aged audit log rows from the hot table (audit_logs) to the cold table
/// (audit_logs_archive).  The hot table is kept to a rolling 90-day window so
/// primary DB scans remain fast; the archive holds the full immutable history
/// required by Part-145 (5-year retention minimum).
///
/// Runs nightly at 02:00 UTC during low-traffic window.
/// The archive table is append-only: no UPDATE or DELETE is ever issued against it.
/// </summary>
[DisallowConcurrentExecution]
public sealed class AuditArchiveJob : IJob
{
    private readonly ILogger<AuditArchiveJob> _logger;

    public AuditArchiveJob(ILogger<AuditArchiveJob> logger)
    {
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("AuditArchiveJob started at {FireTime}", context.FireTimeUtc);

        try
        {
            // TODO (Sprint 5):
            //   1. BEGIN TRANSACTION
            //   2. INSERT INTO audit_logs_archive SELECT * FROM audit_logs
            //        WHERE created_at < NOW() - INTERVAL '90 days'
            //   3. DELETE FROM audit_logs WHERE id IN (archived ids)
            //   4. COMMIT
            //   Log row count archived.  On failure, ROLLBACK and re-throw so
            //   Quartz records the misfire without deleting anything.

            await Task.Delay(TimeSpan.FromMilliseconds(1), context.CancellationToken);

            _logger.LogInformation("AuditArchiveJob completed");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("AuditArchiveJob cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AuditArchiveJob failed");
            throw new JobExecutionException(ex, refireImmediately: false);
        }
    }
}
