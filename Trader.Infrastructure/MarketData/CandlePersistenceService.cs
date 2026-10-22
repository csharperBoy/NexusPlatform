using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Runtime;
using Trader.Application.Specifications.Candles;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.MarketData;

public sealed class CandlePersistenceService : ICandlePersistenceService
{
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly ILogger<CandlePersistenceService> _logger;

    public CandlePersistenceService(
        ITraderUnitOfWork<TraderDbContext> uow,
        ILogger<CandlePersistenceService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<int> PersistAsync(
        string isin,
        int timeFrame,
        IReadOnlyList<Candle> candles,
        string source,
        CancellationToken ct = default)
    {
        if (candles.Count == 0) return 0;

        var minTime = candles.Min(c => c.OpenTime);

        // ─── پیدا کردن OpenTimeهای موجود ───
        HashSet<DateTime> existingTimes;
        try
        {
            var spec = new CandlesByIsinTfAfterSpec(isin, timeFrame, minTime);
            var existing = await _uow.CandleRecordSpecs.ListBySpecAsync(spec);
            existingTimes = existing.Select(c => c.OpenTime).ToHashSet();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to query existing candles for {Isin} tf={Tf}.",
                isin, timeFrame);
            return 0;
        }

        // ─── فیلتر کندل‌های جدید ───
        var newCandles = candles
            .Where(c => !existingTimes.Contains(c.OpenTime))
            .ToList();

        if (newCandles.Count == 0)
            return 0;

        var fetchedAt = DateTime.UtcNow;
        var inserted = 0;

        foreach (var c in newCandles)
        {
            try
            {
                var record = CandleRecordMapper.FromRuntime(
                    c, isin, timeFrame, source, fetchedAt);
                await _uow.CandleRecords.AddAsync(record);
                inserted++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to map candle for {Isin} at {Time}.",
                    isin, c.OpenTime);
            }
        }

        if (inserted == 0) return 0;

        try
        {
            await _uow.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to persist {Count} candles for {Isin} tf={Tf}.",
                inserted, isin, timeFrame);
            return 0;
        }

        _logger.LogInformation(
            "Candles persisted: isin={Isin} tf={Tf} new={Inserted} total={Total}",
            isin, timeFrame, inserted, candles.Count);

        return inserted;
    }
}