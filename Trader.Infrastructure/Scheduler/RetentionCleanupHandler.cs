using Microsoft.Extensions.Logging;
using Scheduler.Application.Abstractions;
using Trader.Application.Scheduler;
using Trader.Infrastructure.Services.Retention;

namespace Trader.Infrastructure.Scheduler;

public class RetentionCleanupHandler : IScheduledJobHandler<RetentionCleanupPayload>
{
    private readonly ITradingEventRetentionService _retention;
    private readonly ILogger<RetentionCleanupHandler> _logger;

    public RetentionCleanupHandler(
        ITradingEventRetentionService retention,
        ILogger<RetentionCleanupHandler> logger)
    {
        _retention = retention;
        _logger = logger;
    }

    public async Task ExecuteAsync(RetentionCleanupPayload payload, CancellationToken ct)
    {
        _logger.LogInformation(
            "Retention cleanup job starting (RetentionDays={Days}).",
            payload.RetentionDays);

        try
        {
            var deleted = await _retention.CleanupAsync(ct);

            _logger.LogInformation(
                "Retention cleanup job completed: {Count} events deleted.",
                deleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Retention cleanup job failed.");
            throw;
        }
    }
}