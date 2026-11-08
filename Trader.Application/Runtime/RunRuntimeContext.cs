using Trader.Application.Abstractions.Strategies;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;

namespace Trader.Application.Runtime;

/// <summary>
/// نمای Immutable از یک Run فعال. در Start اجرای Run ساخته و Cache می‌شود.
/// در هر Tick از Cache خوانده می‌شود — نباید Async I/O داشته باشد.
/// </summary>
public sealed record RunRuntimeContext(
    Guid RunId,
    string Name,
    Guid OwnerPersonId,
    Guid OwnerUserId,
    string StrategyKey,
    IStrategy Strategy,
    StrategyConfig StrategyConfig,
    TimeFrame PrimaryTimeFrame,
    string? TargetCurrency,
    decimal StrengthMultiplier,
    decimal MinDeltaPercent,
    ExecutionMode ExecutionMode,
    CapitalAllocation CapitalAllocation,
    StopLossConfig? StopLoss,
    TakeProfitConfig? TakeProfit,
    OrderMode DefaultOrderMode,
    MarketCloseBehavior OnMarketClose,
    StopRunBehavior OnStop,
    IReadOnlyList<RunSymbolContext> Symbols,
    IReadOnlyList<RunAccountAllocation> AccountAllocations)   // ← changed
{
    /// <summary>Account Ids — derived.</summary>
    public IReadOnlyList<Guid> AccountIds =>
        AccountAllocations.Select(a => a.AccountId).ToList();

    /// <summary>درصد تخصیص برای یک Account خاص.</summary>
    public decimal GetAllocationFor(Guid accountId)
        => AccountAllocations
            .FirstOrDefault(a => a.AccountId == accountId)?.Percent
            ?? throw new InvalidOperationException(
                $"Account {accountId} not in Run {RunId}.");
}