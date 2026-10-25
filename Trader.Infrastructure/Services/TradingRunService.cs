using Core.Application.Context;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.TradingRuns;
using Trader.Application.Dtos.TradingRuns;
using Trader.Application.Specifications.TradingRuns;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Services;

public class TradingRunService : ITradingRunCommandService, ITradingRunQueryService
{
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly ITradingRunManager _runManager;
    private readonly ILogger<TradingRunService> _logger;

    public TradingRunService(
        ITraderUnitOfWork<TraderDbContext> uow,
        ITradingRunManager runManager,
        ILogger<TradingRunService> logger)
    {
        _uow = uow;
        _runManager = runManager;
        _logger = logger;
    }


    /* ═══════════════════ Commands ═══════════════════ */

    public async Task<Guid> CreateAsync(CreateTradingRunModel model)
    {
        var run = TradingRun.Create(
            model.Name,
            ownerPersonId: Guid.Empty,
            strategyKey: model.StrategyKey,
            strategyConfigJson: model.StrategyConfigJson,
            timeFrame: (TimeFrame)model.PrimaryTimeFrame,
            capitalAllocation: BuildCapitalAllocation(model.Settings),
            stopLoss: BuildStopLoss(model.Settings),
            takeProfit: BuildTakeProfit(model.Settings),
            defaultOrderMode: (OrderMode)model.Settings.DefaultOrderMode,
            onMarketClose: (MarketCloseBehavior)model.Settings.OnMarketClose,
            onStop: (StopRunBehavior)model.Settings.OnStop,
            symbols: model.Symbols.Select(s =>
                (s.Isin, new SymbolAllocation(s.AllocationPercentage))),
            accountIds: model.AccountIds,
    targetCurrency: model.TargetCurrency);

        await _uow.TradingRuns.AddAsync(run);
        await _uow.SaveChangesAsync();

        _logger.LogInformation(
            "Created TradingRun {Id} name={Name} strategy={Strategy}",
            run.Id, run.Name, run.StrategyKey);

        return run.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateTradingRunModel model)
    {
        var spec = new TradingRunByIdWithChildrenSpec(id);
        var run = await _uow.TradingRunSpecs.GetBySpecAsync(spec)
            ?? throw new InvalidOperationException($"TradingRun {id} not found.");

        run.UpdateSettings(
            model.Name,
            model.StrategyKey,
            model.StrategyConfigJson,
            (TimeFrame)model.PrimaryTimeFrame,
            BuildCapitalAllocation(model.Settings),
            BuildStopLoss(model.Settings),
            BuildTakeProfit(model.Settings),
            (OrderMode)model.Settings.DefaultOrderMode,
            (MarketCloseBehavior)model.Settings.OnMarketClose,
            (StopRunBehavior)model.Settings.OnStop,
    model.TargetCurrency);

        // Sync Symbols
        var incomingIsins = model.Symbols.Select(s => s.Isin).ToHashSet();
        foreach (var existing in run.Symbols.ToList())
        {
            if (!incomingIsins.Contains(existing.Isin))
                run.RemoveSymbol(existing.Isin);
        }
        foreach (var s in model.Symbols)
        {
            var existing = run.Symbols.FirstOrDefault(x => x.Isin == s.Isin);
            if (existing is null)
                run.AddSymbol(s.Isin, new SymbolAllocation(s.AllocationPercentage));
            else
                run.UpdateSymbolAllocation(s.Isin, new SymbolAllocation(s.AllocationPercentage));
        }

        // Sync Accounts
        var incomingAccountIds = model.AccountIds.ToHashSet();
        foreach (var existing in run.Accounts.ToList())
        {
            if (!incomingAccountIds.Contains(existing.TraderAccountId))
                run.RemoveAccount(existing.TraderAccountId);
        }
        foreach (var accId in model.AccountIds)
            run.AddAccount(accId);

        await _uow.TradingRuns.UpdateAsync(run);
        await _uow.SaveChangesAsync();

        // اگر Run فعال است، Context را Refresh کن
        if (_runManager.IsRunning(id))
        {
            await _runManager.StartAsync(id); // Idempotent → Refresh
        }

        _logger.LogInformation("Updated TradingRun {Id}", id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var run = await _uow.TradingRuns.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"TradingRun {id} not found.");

        if (run.State == TradingRunState.Running)
            throw new InvalidOperationException("Cannot delete a running TradingRun.");

        await _uow.TradingRuns.DeleteAsync(run);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Deleted TradingRun {Id}", id);
    }
    public async Task StartAsync(Guid id)
    {
        var spec = new TradingRunByIdWithChildrenSpec(id);
        var run = await _uow.TradingRunSpecs.GetBySpecAsync(spec)
            ?? throw new InvalidOperationException($"TradingRun {id} not found.");

        // ─── اطمینان از وجود OwnerUserId ───
        if (run.OwnerUserId is null || run.OwnerUserId == Guid.Empty)
            throw new InvalidOperationException(
                $"TradingRun {id} has no OwnerUserId; cannot start.");

        run.Start();
        await _uow.TradingRuns.UpdateAsync(run);
        await _uow.SaveChangesAsync();

        try
        {
            await _runManager.StartAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Runtime activation failed for TradingRun {Id}", id);
            run.MarkFaulted($"Runtime activation failed: {ex.Message}");
            await _uow.TradingRuns.UpdateAsync(run);
            await _uow.SaveChangesAsync();
            throw;
        }

        _logger.LogInformation(
            "Started TradingRun {Id} owner={OwnerId}", id, run.OwnerUserId);
    }


    public async Task StopAsync(Guid id, StopRunBehavior behavior)
    {
        // ۱. Runtime را متوقف کن تا دیگر Tick نزند
        await _runManager.StopAsync(id, behavior);

        // ۲. DB را به‌روز کن
        var run = await _uow.TradingRuns.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"TradingRun {id} not found.");

        run.Stop(behavior);

        await _uow.TradingRuns.UpdateAsync(run);
        await _uow.SaveChangesAsync();

        _logger.LogInformation(
            "Stopped TradingRun {Id} behavior={Behavior}", id, behavior);
    }

    /* ═══════════════════ Queries ═══════════════════ */

    public async Task<IReadOnlyList<TradingRunInfoView>> GetListAsync(
        CancellationToken ct = default)
    {
        var runs = await _uow.TradingRuns.GetAllAsync(
            q => q.OrderByDescending(r => r.CreatedAt),
            r => r.Symbols,
            r => r.Accounts);

        return runs.Select(MapToInfoView).ToList();
    }

    public async Task<TradingRunDetailView?> GetByIdAsync(
        Guid id, CancellationToken ct = default)
    {
        var spec = new TradingRunByIdWithChildrenSpec(id);
        var run = await _uow.TradingRunSpecs.GetBySpecAsync(spec);
        return run is null ? null : MapToDetailView(run);
    }

    /* ═══════════════════ Helpers ═══════════════════ */

    private static CapitalAllocation BuildCapitalAllocation(TradingRunSettingsModel s)
    {
        if (s.CapitalAllocationMode == (int)CapitalAllocationMode.Fixed)
        {
            if (!s.FixedCapitalAmount.HasValue)
                throw new InvalidOperationException(
                    "FixedCapitalAmount is required when CapitalAllocationMode is Fixed.");
            return CapitalAllocation.Fixed(new Money(s.FixedCapitalAmount.Value));
        }
        return CapitalAllocation.Dynamic();
    }

    private static StopLossConfig? BuildStopLoss(TradingRunSettingsModel s)
    {
        if (s.StopLossPercentage.HasValue)
            return StopLossConfig.FromPercentage(s.StopLossPercentage.Value);
        if (s.StopLossPrice.HasValue)
            return StopLossConfig.FromPrice(new Price(s.StopLossPrice.Value));
        return null;
    }

    private static TakeProfitConfig? BuildTakeProfit(TradingRunSettingsModel s)
    {
        if (s.TakeProfitPercentage.HasValue)
            return TakeProfitConfig.FromPercentage(s.TakeProfitPercentage.Value);
        if (s.TakeProfitPrice.HasValue)
            return TakeProfitConfig.FromPrice(new Price(s.TakeProfitPrice.Value));
        return null;
    }

    private static TradingRunInfoView MapToInfoView(TradingRun r) => new(
    r.Id,
    r.Name,
    (int)r.State,
    r.State.ToString(),
    r.StrategyKey,
    (int)r.PrimaryTimeFrame,
    r.PrimaryTimeFrame.ToCode(),
    (int)r.CapitalAllocation.Mode,
    r.CapitalAllocation.FixedAmount?.Amount,
    r.TargetCurrency,                                     // ← جدید
    r.Symbols.Count,
    r.Accounts.Count,
    r.StartedAt,
    r.StoppedAt,
    r.CreatedAt);

    private static TradingRunDetailView MapToDetailView(TradingRun r) => new(
        r.Id,
        r.Name,
        (int)r.State,
        r.State.ToString(),
        r.StrategyKey,
        r.StrategyConfigJson,
        (int)r.PrimaryTimeFrame,
    r.TargetCurrency,
        new TradingRunSettingsDto(
            (int)r.CapitalAllocation.Mode,
            r.CapitalAllocation.FixedAmount?.Amount,
            r.StopLoss?.Percentage,
            r.StopLoss?.AbsolutePrice?.Value,
            r.TakeProfit?.Percentage,
            r.TakeProfit?.AbsolutePrice?.Value,
            (int)r.DefaultOrderMode,
            (int)r.OnMarketClose,
            (int)r.OnStop),
        r.Symbols.Select(s => new TradingRunSymbolDto(s.Isin, s.Allocation.Percentage)).ToList(),
        r.Accounts.Select(a => a.TraderAccountId).ToList(),
        r.StartedAt,
        r.StoppedAt,
        r.CreatedAt,
        r.ModifiedAt);
}