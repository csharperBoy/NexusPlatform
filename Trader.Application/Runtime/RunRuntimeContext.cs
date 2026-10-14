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
    string StrategyKey,
    IStrategy Strategy,
    StrategyConfig StrategyConfig,
    TimeFrame PrimaryTimeFrame,
    CapitalAllocation CapitalAllocation,
    StopLossConfig? StopLoss,
    TakeProfitConfig? TakeProfit,
    OrderMode DefaultOrderMode,
    MarketCloseBehavior OnMarketClose,
    StopRunBehavior OnStop,
    IReadOnlyList<RunSymbolContext> Symbols,
    IReadOnlyList<Guid> AccountIds);