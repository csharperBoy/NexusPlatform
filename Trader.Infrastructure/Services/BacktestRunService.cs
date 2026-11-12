using Core.Domain.Specifications;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Backtesting;
using Trader.Application.Dtos.Backtests;
using Trader.Application.Specifications.Backtests;
using Trader.Application.Specifications.TradingRuns;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Services;

public sealed class BacktestRunService
    : IBacktestRunCommandService, IBacktestRunQueryService
{
    private const string DefaultTimeZoneId = "Asia/Tehran";
    private const decimal DefaultSlippagePercent = 0.001m;

    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly ILogger<BacktestRunService> _logger;

    public BacktestRunService(
        ITraderUnitOfWork<TraderDbContext> uow,
        ILogger<BacktestRunService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    /* ═══════════════════ Commands ═══════════════════ */

    public async Task<Guid> CreateAsync(
        RunBacktestModel model, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
            throw new InvalidOperationException("نام Backtest الزامی است.");

        if (model.DateTo <= model.DateFrom)
            throw new InvalidOperationException(
                "تاریخ پایان باید بعد از تاریخ شروع باشد.");

        // ─── ۱. ساخت Config نهایی (SourceRun یا Override) ───
        var effectiveConfig = await BuildEffectiveConfigAsync(model, ct);

        // ─── ۲. Initial Capital ───
        var initialCapital = model.InitialCapital;

        if (!initialCapital.HasValue || initialCapital.Value <= 0m)
            throw new InvalidOperationException(
                "سرمایه‌ی اولیه الزامی است (برای Dynamic Run یا Virtual Backtest).");

        var slippage = model.SlippagePercent ?? DefaultSlippagePercent;
        var tz = string.IsNullOrWhiteSpace(model.TimeZoneId)
            ? DefaultTimeZoneId
            : model.TimeZoneId;

        // ─── ۳. Serialize Snapshot ───
        var configSnapshotJson = JsonSerializer.Serialize(
            new
            {
                effectiveConfig.StrategyKey,
                effectiveConfig.StrategyConfigJson,
                effectiveConfig.PrimaryTimeFrame,
                effectiveConfig.TargetCurrency,
                effectiveConfig.CapitalAllocationMode,
                effectiveConfig.FixedCapitalAmount,
                effectiveConfig.StrengthMultiplier,
                effectiveConfig.MinDeltaPercent,
                effectiveConfig.StopLossPercentage,
                effectiveConfig.StopLossPrice,
                effectiveConfig.TakeProfitPercentage,
                effectiveConfig.TakeProfitPrice,
                effectiveConfig.DefaultOrderMode,
                effectiveConfig.OnMarketClose,
                effectiveConfig.OnStop,
                InitialCapital = initialCapital.Value,
                SlippagePercent = slippage,
                SubCandleTimeFrame = model.SubCandleTimeFrame,
                TimeZoneId = tz,
                Symbols = effectiveConfig.Symbols
            },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        // ─── ۴. ساخت BacktestRun ───
        var run = BacktestRun.Create(
            name: model.Name,
            sourceRunId: model.SourceRunId,
            strategyKey: effectiveConfig.StrategyKey,
            strategyConfigJson: effectiveConfig.StrategyConfigJson,
            primaryTimeFrame: effectiveConfig.PrimaryTimeFrame,
            targetCurrency: effectiveConfig.TargetCurrency,
            capitalAllocationMode: effectiveConfig.CapitalAllocationMode,
            fixedCapitalAmount: effectiveConfig.FixedCapitalAmount,
            strengthMultiplier: effectiveConfig.StrengthMultiplier,
            minDeltaPercent: effectiveConfig.MinDeltaPercent,
            stopLossPercentage: effectiveConfig.StopLossPercentage,
            stopLossPrice: effectiveConfig.StopLossPrice,
            takeProfitPercentage: effectiveConfig.TakeProfitPercentage,
            takeProfitPrice: effectiveConfig.TakeProfitPrice,
            defaultOrderMode: effectiveConfig.DefaultOrderMode,
            onMarketClose: effectiveConfig.OnMarketClose,
            onStop: effectiveConfig.OnStop,
            configSnapshotJson: configSnapshotJson,
            dateFrom: model.DateFrom,
            dateTo: model.DateTo,
            initialCapital: initialCapital.Value,
            slippagePercent: slippage,
            subCandleTimeFrame: model.SubCandleTimeFrame,
            timeZoneId: tz,
            symbols: effectiveConfig.Symbols
                .Select(s => (s.Isin, s.AllocationPercentage)));

        await _uow.BacktestRuns.AddAsync(run);

        _logger.LogInformation(
            "BacktestRun created: {Id} name={Name} strategy={Strategy} " +
            "symbols={Symbols} range={From:yyyy-MM-dd}→{To:yyyy-MM-dd}",
            run.Id, run.Name, run.StrategyKey, run.Symbols.Count,
            run.DateFrom, run.DateTo);

        return run.Id;
    }

    public async Task<bool> CancelAsync(Guid id, CancellationToken ct = default)
    {
        var run = await _uow.BacktestRuns.GetByIdAsync(id);
        if (run is null) return false;

        if (run.Status != BacktestStatus.Pending
            && run.Status != BacktestStatus.Running)
            return false;

        run.MarkCancelled();
        await _uow.BacktestRuns.UpdateAsync(run);

        _logger.LogInformation("BacktestRun {Id} cancelled.", id);
        return true;
    }

    public async Task SaveAsync(CancellationToken ct = default)
    {
        await _uow.SaveChangesAsync(ct);
    }

    /* ═══════════════════ Queries ═══════════════════ */

    
    public async Task<IReadOnlyList<BacktestRunSummaryView>> GetListAsync(
    Guid? sourceRunId, int take = 100, CancellationToken ct = default)
    {
        var takeClamped = Math.Clamp(take, 1, 500);

        // ─── SQL-side filter + order + take ───
        var runs = await _uow.BacktestRuns.GetAllAsync(q =>
        {
            IQueryable<BacktestRun> query = q;
            if (sourceRunId.HasValue)
                query = query.Where(b => b.SourceRunId == sourceRunId.Value);
            return query.OrderByDescending(b => b.CreatedAt).Take(takeClamped);
        });

        return runs.Select(MapToSummary).ToList();
    }
    public async Task<BacktestRunDetailView?> GetByIdAsync(
    Guid id, CancellationToken ct = default)
    {
        var spec = new BacktestRunByIdWithSymbolsSpec(id);
        var run = await _uow.BacktestRunSpecs.GetBySpecAsync(spec);
        return run is null ? null : MapToDetail(run);
    }

    public async Task<IReadOnlyList<BacktestTradeView>> GetTradesAsync(
        Guid backtestRunId, CancellationToken ct = default)
    {
        var spec = new BacktestTradesByRunSpec(backtestRunId);
        var trades = await _uow.BacktestTradeSpecs.ListBySpecAsync(spec);

        return trades
            .OrderBy(t => t.EntryTime)
            .Select(MapToTradeView)
            .ToList();
    }

    public async Task<IReadOnlyList<BacktestDailyEquityView>> GetEquityCurveAsync(
    Guid backtestRunId, CancellationToken ct = default)
    {
        // ─── SQL-side filter + order ───
        var equities = await _uow.BacktestDailyEquities.GetAllAsync(q =>
            q.Where(e => e.BacktestRunId == backtestRunId)
             .OrderBy(e => e.Date));

        return equities
            .Select(e => new BacktestDailyEquityView(
                e.Date, e.Equity, e.Cash, e.PositionValue, e.DailyReturnPct))
            .ToList();
    }

    public async Task<IReadOnlyList<BacktestRunSummaryView>> CompareAsync(
        List<Guid> ids, CancellationToken ct = default)
    {
        if (ids is null || ids.Count == 0) return Array.Empty<BacktestRunSummaryView>();

        var all = await _uow.BacktestRuns.GetAllAsync();
        var set = ids.ToHashSet();

        return all
            .Where(b => set.Contains(b.Id))
            .OrderByDescending(b => b.CreatedAt)
            .Select(MapToSummary)
            .ToList();
    }

    /* ═══════════════════ Config Building ═══════════════════ */

    private sealed record EffectiveConfig(
        string StrategyKey,
        string StrategyConfigJson,
        int PrimaryTimeFrame,
        string? TargetCurrency,
        int CapitalAllocationMode,
        decimal? FixedCapitalAmount,
        decimal StrengthMultiplier,
        decimal MinDeltaPercent,
        decimal? StopLossPercentage,
        decimal? StopLossPrice,
        decimal? TakeProfitPercentage,
        decimal? TakeProfitPrice,
        int DefaultOrderMode,
        int OnMarketClose,
        int OnStop,
        List<BacktestSymbolModel> Symbols);

    private async Task<EffectiveConfig> BuildEffectiveConfigAsync(
        RunBacktestModel model, CancellationToken ct)
    {
        // ─── Mode 1: From Existing Run ───
        if (model.SourceRunId.HasValue)
        {
            var spec = new TradingRunByIdWithChildrenSpec(model.SourceRunId.Value);
            var sourceRun = await _uow.TradingRunSpecs.GetBySpecAsync(spec)
                ?? throw new InvalidOperationException(
                    $"TradingRun {model.SourceRunId.Value} not found.");

            var symbols = sourceRun.Symbols
                .Select(s => new BacktestSymbolModel(s.Isin, s.Allocation.Percentage))
                .ToList();

            // ═══════════════════════════════════════════════════════════════
            // Capital Suggestion: اگر همه‌ی Accountها Fixed بودند، جمعشان.
            // اگر حداقل یکی Dynamic بود، null (کاربر باید خودش سرمایه‌ی
            // Backtest را وارد کند — چون Dynamic در Backtest معنی ندارد).
            // ═══════════════════════════════════════════════════════════════
            var allAccountsFixed = sourceRun.Accounts.Count > 0
                && sourceRun.Accounts.All(a => a.CapitalAllocation.IsFixed);

            var suggestedCapital = allAccountsFixed
                ? sourceRun.Accounts.Sum(a => a.CapitalAllocation.FixedAmount!.Amount)
                : (decimal?)null;

            // اگر suggestion موجود بود، در model.InitialCapital پیشنهاد می‌شود.
            // اما از آنجایی که field از EffectiveConfig حذف شده، از model استفاده می‌کنیم:
            if (!model.InitialCapital.HasValue && suggestedCapital.HasValue)
            {
                // Silent — یا throw کن یا model.InitialCapital را در caller set کن.
                // راه بهتر: throw تا caller آگاهانه مقدار بدهد.
                throw new InvalidOperationException(
                    $"InitialCapital is required for backtest. " +
                    $"Suggested value based on Run's fixed accounts: {suggestedCapital.Value:N0}.");
            }

            var ov = model.ConfigOverride;

            return new EffectiveConfig(
    StrategyKey: ov?.StrategyKey ?? sourceRun.StrategyKey,
    StrategyConfigJson: ov?.StrategyConfigJson ?? sourceRun.StrategyConfigJson,
    PrimaryTimeFrame: ov?.PrimaryTimeFrame ?? (int)sourceRun.PrimaryTimeFrame,
    // BC8 fix: use ?? not `is not null ? :` — so that an override with
    // TargetCurrency=null falls back to source (not silently nullifies it)
    TargetCurrency: ov?.TargetCurrency ?? sourceRun.TargetCurrency,
    CapitalAllocationMode: (int)CapitalAllocationMode.Fixed,
    FixedCapitalAmount: suggestedCapital,
    StrengthMultiplier: ov?.StrengthMultiplier ?? sourceRun.StrengthMultiplier,
    MinDeltaPercent: ov?.MinDeltaPercent ?? sourceRun.MinDeltaPercent,
    StopLossPercentage: ov?.StopLossPercentage ?? sourceRun.StopLoss?.Percentage,
    StopLossPrice: ov?.StopLossPrice ?? sourceRun.StopLoss?.AbsolutePrice?.Value,
    TakeProfitPercentage: ov?.TakeProfitPercentage ?? sourceRun.TakeProfit?.Percentage,
    TakeProfitPrice: ov?.TakeProfitPrice ?? sourceRun.TakeProfit?.AbsolutePrice?.Value,
    DefaultOrderMode: ov?.DefaultOrderMode ?? (int)sourceRun.DefaultOrderMode,
    OnMarketClose: ov?.OnMarketClose ?? (int)sourceRun.OnMarketClose,
    OnStop: ov?.OnStop ?? (int)sourceRun.OnStop,
    Symbols: symbols);
        }

        // ─── Mode 2: Virtual ───
        var c = model.ConfigOverride
            ?? throw new InvalidOperationException(
                "برای Virtual Backtest، ConfigOverride الزامی است.");

        if (string.IsNullOrWhiteSpace(c.StrategyKey))
            throw new InvalidOperationException("StrategyKey الزامی است.");

        if (c.Symbols is null || c.Symbols.Count == 0)
            throw new InvalidOperationException("حداقل یک نماد الزامی است.");

        var totalPct = c.Symbols.Sum(s => s.AllocationPercentage);
        if (Math.Abs(totalPct - 100m) > 0.01m)
            throw new InvalidOperationException(
                $"مجموع درصد تخصیص نمادها باید ۱۰۰ باشد (فعلی: {totalPct:0.##}).");

        return new EffectiveConfig(
            StrategyKey: c.StrategyKey,
            StrategyConfigJson: c.StrategyConfigJson,
            PrimaryTimeFrame: c.PrimaryTimeFrame,
            TargetCurrency: c.TargetCurrency,
            CapitalAllocationMode: (int)CapitalAllocationMode.Fixed,
            FixedCapitalAmount: null,
            StrengthMultiplier: c.StrengthMultiplier,
            MinDeltaPercent: c.MinDeltaPercent,
            StopLossPercentage: c.StopLossPercentage,
            StopLossPrice: c.StopLossPrice,
            TakeProfitPercentage: c.TakeProfitPercentage,
            TakeProfitPrice: c.TakeProfitPrice,
            DefaultOrderMode: c.DefaultOrderMode,
            OnMarketClose: c.OnMarketClose,
            OnStop: c.OnStop,
            Symbols: c.Symbols);
    }

    /* ═══════════════════ Mappers ═══════════════════ */

    private static BacktestRunSummaryView MapToSummary(BacktestRun r) => new(
        r.Id, r.Name,
        (int)r.Status, r.Status.ToString(),
        r.StrategyKey, r.PrimaryTimeFrame, r.TargetCurrency,
        r.InitialCapital, r.FinalEquity, r.TotalReturnPct,
        r.MaxDrawdownPct, r.SharpeRatio, r.WinRate,
        r.TotalTrades, r.WinningTrades, r.LosingTrades,
        r.DateFrom, r.DateTo,
        r.StartedAt, r.CompletedAt, r.DurationMs, r.CreatedAt);

    private static BacktestRunDetailView MapToDetail(BacktestRun r) => new(
        r.Id, r.Name, r.SourceRunId,
        (int)r.Status, r.Status.ToString(),
        r.StrategyKey, r.StrategyConfigJson,
        r.PrimaryTimeFrame, r.TargetCurrency,
        r.CapitalAllocationMode, r.FixedCapitalAmount,
        r.StrengthMultiplier, r.MinDeltaPercent,
        r.StopLossPercentage, r.StopLossPrice,
        r.TakeProfitPercentage, r.TakeProfitPrice,
        r.DefaultOrderMode, r.OnMarketClose, r.OnStop,
        r.ConfigSnapshotJson,
        r.DateFrom, r.DateTo,
        r.InitialCapital, r.SlippagePercent, r.SubCandleTimeFrame, r.TimeZoneId,
        r.FinalEquity, r.TotalReturnPct, r.MaxDrawdownPct,
        r.SharpeRatio, r.WinRate,
        r.TotalTrades, r.WinningTrades, r.LosingTrades,
        r.ProfitFactor, r.AvgWin, r.AvgLoss,
        r.TotalCommission, r.TotalSlippage,
        r.Symbols.Select(s => new BacktestSymbolModel(s.Isin, s.AllocationPercentage)).ToList(),
        r.StartedAt, r.CompletedAt, r.DurationMs, r.ErrorMessage, r.CreatedAt);

    private static BacktestTradeView MapToTradeView(BacktestTrade t) => new(
        t.Id, t.Isin,
        t.EntryTime, t.LastEntryTime,
        t.TotalEntryQuantity, t.WeightedAvgEntryPrice, t.TotalBuyCommission,
        t.ExitTime, t.ExitPrice, t.ExitQuantity, t.TotalSellCommission,
        (int)t.ExitReason, t.ExitReason.ToString(),
        t.GrossPnL, t.NetPnL, t.NetPnLPct, t.HoldingDurationSeconds,
        t.EntryFillsJson);
}