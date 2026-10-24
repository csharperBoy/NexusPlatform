using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Runtime;
using Trader.Application.Specifications.Candles;
using Trader.Domain.Enums;
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

        // ─── فقط کندل‌های بسته‌شده ───
        var tfEnum = (TimeFrame)timeFrame;
        var tfDuration = tfEnum.ToDuration();
        var now = DateTime.UtcNow;

        var closedCandles = candles
            .Where(c => c.OpenTime + tfDuration <= now)
            .ToList();

        if (closedCandles.Count == 0)
        {
            _logger.LogDebug(
                "No closed candles yet for {Isin} tf={Tf} (all {Count} are in-progress).",
                isin, tfEnum, candles.Count);
            return 0;
        }

        var minTime = closedCandles.Min(c => c.OpenTime);

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
                "Failed to query existing candles for {Isin} tf={Tf}.", isin, timeFrame);
            return 0;
        }

        // ─── فیلتر کندل‌های جدید ───
        var newCandles = closedCandles
            .Where(c => !existingTimes.Contains(c.OpenTime))
            .ToList();

        if (newCandles.Count == 0) return 0;

        var fetchedAt = DateTime.UtcNow;
        var inserted = 0;

        foreach (var c in newCandles)
        {
            var record = CandleRecordMapper.FromRuntime(
                c, isin, timeFrame, source, fetchedAt);
            await _uow.CandleRecords.AddAsync(record);
            inserted++;
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

        _logger.LogDebug(
            "Candles persisted: isin={Isin} tf={Tf} new={Inserted} closed={Closed} total={Total}",
            isin, timeFrame, inserted, closedCandles.Count, candles.Count);

        return inserted;
    }
}