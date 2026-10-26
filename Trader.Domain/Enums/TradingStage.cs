namespace Trader.Domain.Enums;

/// <summary>
/// مرحله‌ای از Pipeline که رویداد در آن رخ داده است.
/// </summary>
public enum TradingStage
{
    Lifecycle = 0,           // Run Start/Stop/Fault
    MarketData = 1,          // Stage 1
    StrategyEvaluation = 2,  // Stage 2
    Execution = 3,           // Stage 3
    OrderLifecycle = 4,      // Stage 4
    Recovery = 5,            // Startup Recovery
    System = 6,              // Worker-level (Tick, Retention, ...)
}