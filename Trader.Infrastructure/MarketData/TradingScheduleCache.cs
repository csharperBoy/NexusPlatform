using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Specifications.Instruments;
using Trader.Domain.Entities;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.MarketData;

public sealed class TradingScheduleCache : ITradingScheduleCache
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(10);
    private const string DefaultScheduleCode = "TSE-REGULAR";

    private sealed record Entry(TradingSchedule? Schedule, DateTime LoadedAt);

    private readonly ConcurrentDictionary<string, Entry> _cache = new();
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TradingScheduleCache> _logger;

    public TradingScheduleCache(
        IServiceScopeFactory scopeFactory,
        ILogger<TradingScheduleCache> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<TradingSchedule?> GetForIsinAsync(
        string isin, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(isin))
            return null;

        if (_cache.TryGetValue(isin, out var entry)
            && DateTime.UtcNow - entry.LoadedAt < Ttl)
        {
            return entry.Schedule;
        }

        TradingSchedule? schedule = null;

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider
                .GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();

            var instruments = await uow.InstrumentSpecs
                .ListBySpecAsync(new InstrumentByIsinSpec(isin));

            var instrument = instruments.FirstOrDefault();

            if (instrument?.TradingScheduleId is Guid scheduleId)
            {
                schedule = await uow.TradingSchedules.GetByIdAsync(scheduleId);
            }

            // ─── Fallback: اگر Instrument Schedule نداشت → Default ───
            if (schedule is null)
            {
                var all = await uow.TradingSchedules.GetAllAsync();
                schedule = all.FirstOrDefault(s =>
                    s.Code.Equals(DefaultScheduleCode, StringComparison.OrdinalIgnoreCase)
                    && s.IsActive);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to load TradingSchedule for {Isin}. Treating as 'always open'.", isin);
            schedule = null;
        }

        _cache[isin] = new Entry(schedule, DateTime.UtcNow);

        _logger.LogDebug(
            "Schedule cached: isin={Isin} code={Code} source={Source}",
            isin,
            schedule?.Code ?? "NULL (always open)",
            schedule is null ? "fallback-none" : "instrument-or-default");

        return schedule;
    }

    public void Invalidate(string isin) => _cache.TryRemove(isin, out _);
    public void InvalidateAll() => _cache.Clear();
}