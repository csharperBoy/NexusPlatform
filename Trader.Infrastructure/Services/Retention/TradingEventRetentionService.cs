using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trader.Application.Abstractions;
using Trader.Application.Runtime;
using Trader.Domain.Enums;
using Trader.Domain.Entities;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Services.Retention;

public sealed class TradingEventRetentionService : ITradingEventRetentionService
{
    private const int BatchSize = 5000;

    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly IOptionsMonitor<TradingLoggingOptions> _options;
    private readonly ILogger<TradingEventRetentionService> _logger;

    public TradingEventRetentionService(
        ITraderUnitOfWork<TraderDbContext> uow,
        IOptionsMonitor<TradingLoggingOptions> options,
        ILogger<TradingEventRetentionService> logger)
    {
        _uow = uow;
        _options = options;
        _logger = logger;
    }

    public async Task<int> CleanupAsync(CancellationToken ct = default)
    {
        var days = _options.CurrentValue.RetentionDays;
        if (days <= 0)
        {
            _logger.LogWarning(
                "RetentionDays is {Days}; skipping cleanup.", days);
            return 0;
        }

        var cutoff = DateTime.UtcNow.AddDays(-days);
        var totalDeleted = 0;
        var startTime = DateTime.UtcNow;

        _logger.LogInformation(
            "Retention cleanup starting: deleting events older than {Cutoff:u}",
            cutoff);

        try
        {
            // ─── حذف دسته‌ای (Batch) برای جلوگیری از Lock طولانی ───
            while (!ct.IsCancellationRequested)
            {
                var ids = await _uow.Context.TradingEvents
                    .Where(e => e.OccurredAt < cutoff)
                    .OrderBy(e => e.OccurredAt)
                    .Select(e => e.Id)
                    .Take(BatchSize)
                    .ToListAsync(ct);

                if (ids.Count == 0) break;

                // حذف با ExecuteDelete (EF Core 7+) — بسیار سریع‌تر از RemoveRange
                var deleted = await _uow.Context.TradingEvents
                    .Where(e => ids.Contains(e.Id))
                    .ExecuteDeleteAsync(ct);

                totalDeleted += deleted;

                _logger.LogDebug(
                    "Retention batch: {Count} events deleted (total: {Total})",
                    deleted, totalDeleted);
            }

            var elapsed = DateTime.UtcNow - startTime;

            // ثبت رویداد نتیجه‌ی Retention
            var retentionEvent = TradingEvent.Create(
                stage: TradingStage.System,
                type: TradingEventType.RetentionCleanupCompleted,
                severity: TradingEventSeverity.Info,
                message: $"پاک‌سازی رویدادهای قدیمی انجام شد. " +
                         $"{totalDeleted:N0} رویداد (قبل از {cutoff:yyyy-MM-dd}) حذف شد " +
                         $"در مدت {elapsed.TotalSeconds:F1} ثانیه.",
                payloadJson: $"{{\"deletedCount\":{totalDeleted}," +
                             $"\"cutoff\":\"{cutoff:O}\"," +
                             $"\"elapsedSeconds\":{elapsed.TotalSeconds:F2}," +
                             $"\"retentionDays\":{days}}}");

            await _uow.Context.TradingEvents.AddAsync(retentionEvent, ct);
            await _uow.Context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Retention cleanup completed: {Count} events deleted in {Elapsed:F1}s",
                totalDeleted, elapsed.TotalSeconds);

            return totalDeleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Retention cleanup failed after deleting {Count} events.",
                totalDeleted);
            throw;
        }
    }
}