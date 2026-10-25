using Core.Domain.Common.EntityProperties;
using Trader.Domain.Enums;
using Trader.Domain.Events;
using Trader.Domain.ValueObjects;

namespace Trader.Domain.Entities;

/// <summary>
/// Aggregate Root اصلی سیستم. یک Strategy + چند Symbol + چند Account
/// با تنظیمات Execution اختصاصی.
/// </summary>
public class TradingRun : BaseEntity, IAuditableEntity, IOwnerableEntity
{
    private readonly List<TradingRunSymbol> _symbols = new();
    private readonly List<TradingRunAccount> _accounts = new();

    public string Name { get; private set; } = default!;
    public TradingRunState State { get; private set; }

    // Strategy
    public string StrategyKey { get; private set; } = default!;
    public string StrategyConfigJson { get; private set; } = default!;
    public TimeFrame PrimaryTimeFrame { get; private set; }

    // Execution
    public CapitalAllocation CapitalAllocation { get; private set; } = default!;
    public StopLossConfig? StopLoss { get; private set; }
    public TakeProfitConfig? TakeProfit { get; private set; }
    public OrderMode DefaultOrderMode { get; private set; }
    public MarketCloseBehavior OnMarketClose { get; private set; }
    public StopRunBehavior OnStop { get; private set; }

    // Lifecycle Timestamps
    public DateTime? StartedAt { get; private set; }
    public DateTime? StoppedAt { get; private set; }

    public IReadOnlyList<TradingRunSymbol> Symbols => _symbols.AsReadOnly();
    public IReadOnlyList<TradingRunAccount> Accounts => _accounts.AsReadOnly();
    public string? TargetCurrency { get; private set; }   // مثلاً "USD" یا null
    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public void Touch() => ModifiedAt = DateTime.UtcNow;

    // IOwnerableEntity
    public Guid? OwnerOrganizationUnitId { get; private set; }
    public Guid? OwnerPositionId { get; private set; }
    public Guid? OwnerPersonId { get; private set; }
    public Guid? OwnerUserId { get; private set; }

    public void SetOwners(Guid? userId, Guid? personId, Guid? positionId, Guid? orgUnitId)
    {
        OwnerUserId = userId;
        OwnerPersonId = personId;
        OwnerPositionId = positionId;
        OwnerOrganizationUnitId = orgUnitId;
        Touch();
    }

    public void SetPersonOwner(Guid personId) { OwnerPersonId = personId; Touch(); }
    public void SetUserOwner(Guid userId) { OwnerUserId = userId; Touch(); }
    public void SetPositionOwner(Guid positionId) { OwnerPositionId = positionId; Touch(); }
    public void SetOrganizationUnitOwner(Guid orgUnitId) { OwnerOrganizationUnitId = orgUnitId; Touch(); }

    private TradingRun() { } // EF

    // ─────────────── Factory ───────────────

    public static TradingRun Create(
        string name,
        Guid ownerPersonId,
        string strategyKey,
        string strategyConfigJson,
        TimeFrame timeFrame,
        CapitalAllocation capitalAllocation,
        StopLossConfig? stopLoss,
        TakeProfitConfig? takeProfit,
        OrderMode defaultOrderMode,
        MarketCloseBehavior onMarketClose,
        StopRunBehavior onStop,
        IEnumerable<(string Isin, SymbolAllocation Allocation)> symbols,
        IEnumerable<Guid> accountIds,
    string? targetCurrency = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Run name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(strategyKey))
            throw new ArgumentException("Strategy key is required.", nameof(strategyKey));

        var run = new TradingRun
        {
            Name = name.Trim(),
            OwnerPersonId = ownerPersonId,
            State = TradingRunState.Created,
            StrategyKey = strategyKey.Trim(),
            StrategyConfigJson = strategyConfigJson,
            PrimaryTimeFrame = timeFrame,
            CapitalAllocation = capitalAllocation,
            StopLoss = stopLoss,
            TakeProfit = takeProfit,
            DefaultOrderMode = defaultOrderMode,
            OnMarketClose = onMarketClose,
            OnStop = onStop,
            CreatedAt = DateTime.UtcNow,
            TargetCurrency = NormalizeCurrency(targetCurrency)   // ← جدید

        };

        foreach (var (isin, allocation) in symbols)
            run.AddSymbolInternal(isin, allocation);

        foreach (var acc in accountIds)
            run.AddAccountInternal(acc);

        run.RecomputeReadiness();
        return run;
    }
    private static string? NormalizeCurrency(string? currency)
    {
        if (string.IsNullOrWhiteSpace(currency)) return null;
        var c = currency.Trim().ToUpperInvariant();
        if (c.Length < 2 || c.Length > 10)
            throw new InvalidOperationException(
                $"TargetCurrency '{currency}' is not a valid ISO code.");
        return c;
    }
    // ─────────────── Settings ───────────────

    public void UpdateSettings(
        string name,
        string strategyKey,
        string strategyConfigJson,
        TimeFrame timeFrame,
        CapitalAllocation capitalAllocation,
        StopLossConfig? stopLoss,
        TakeProfitConfig? takeProfit,
        OrderMode defaultOrderMode,
        MarketCloseBehavior onMarketClose,
        StopRunBehavior onStop,
    string? targetCurrency)
    {
        EnsureNotRunning();

        Name = name.Trim();
        StrategyKey = strategyKey.Trim();
        StrategyConfigJson = strategyConfigJson;
        PrimaryTimeFrame = timeFrame;
        CapitalAllocation = capitalAllocation;
        StopLoss = stopLoss;
        TakeProfit = takeProfit;
        DefaultOrderMode = defaultOrderMode;
        OnMarketClose = onMarketClose;
        OnStop = onStop; 
        TargetCurrency = NormalizeCurrency(targetCurrency);

        Touch();
        RecomputeReadiness();
    }

    // ─────────────── Symbols ───────────────

    public void AddSymbol(string isin, SymbolAllocation allocation)
    {
        EnsureNotRunning();
        AddSymbolInternal(isin, allocation);
        Touch();
        RecomputeReadiness();
    }

    public void RemoveSymbol(string isin)
    {
        EnsureNotRunning();
        var symbol = _symbols.FirstOrDefault(s => s.Isin == isin);
        if (symbol is null) return;
        _symbols.Remove(symbol);
        Touch();
        RecomputeReadiness();
    }

    public void UpdateSymbolAllocation(string isin, SymbolAllocation allocation)
    {
        EnsureNotRunning();
        var symbol = _symbols.FirstOrDefault(s => s.Isin == isin)
            ?? throw new InvalidOperationException($"Symbol {isin} not in run.");
        symbol.UpdateAllocation(allocation);
        Touch();
    }

    private void AddSymbolInternal(string isin, SymbolAllocation allocation)
    {
        if (_symbols.Any(s => s.Isin == isin)) return;
        _symbols.Add(TradingRunSymbol.Create(Id, isin, allocation));
    }

    // ─────────────── Accounts ───────────────

    public void AddAccount(Guid accountId)
    {
        EnsureNotRunning();
        AddAccountInternal(accountId);
        Touch();
        RecomputeReadiness();
    }

    public void RemoveAccount(Guid accountId)
    {
        EnsureNotRunning();
        var acc = _accounts.FirstOrDefault(a => a.TraderAccountId == accountId);
        if (acc is null) return;
        _accounts.Remove(acc);
        Touch();
        RecomputeReadiness();
    }

    private void AddAccountInternal(Guid accountId)
    {
        if (_accounts.Any(a => a.TraderAccountId == accountId)) return;
        _accounts.Add(TradingRunAccount.Create(Id, accountId));
    }

    // ─────────────── Lifecycle ───────────────
    public void Start()
    {
        if (State == TradingRunState.Running)
            throw new InvalidOperationException("Run is already running.");

        if (State != TradingRunState.Ready && State != TradingRunState.Stopped)
            throw new InvalidOperationException($"Cannot start run in state {State}.");

        State = TradingRunState.Running;
        StartedAt = DateTime.UtcNow;
        Touch();

        AddDomainEvent(new TradingRunStartedEvent(Id, StartedAt.Value));
    }

    public void Stop(StopRunBehavior behavior)
    {
        if (State != TradingRunState.Running)
            throw new InvalidOperationException(
                $"Cannot stop run in state {State}.");

        State = TradingRunState.Stopped;
        StoppedAt = DateTime.UtcNow;
        Touch();

        AddDomainEvent(new TradingRunStoppedEvent(Id, StoppedAt.Value, behavior));
    }

    public void MarkFaulted(string reason)
    {
        if (State == TradingRunState.Faulted) return;
        State = TradingRunState.Faulted;
        Touch();
        AddDomainEvent(new TradingRunFaultedEvent(Id, reason, DateTime.UtcNow));
    }

    private void RecomputeReadiness()
    {
        if (State != TradingRunState.Created) return;

        if (_symbols.Count > 0 && _accounts.Count > 0
            && !string.IsNullOrWhiteSpace(StrategyKey)
            && !string.IsNullOrWhiteSpace(StrategyConfigJson))
        {
            State = TradingRunState.Ready;
        }
    }

    private void EnsureNotRunning()
    {
        if (State == TradingRunState.Running)
            throw new InvalidOperationException("Cannot modify a running TradingRun.");
    }
}