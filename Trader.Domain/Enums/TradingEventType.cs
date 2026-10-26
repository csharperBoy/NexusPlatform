namespace Trader.Domain.Enums;

/// <summary>
/// نوع رویداد — برای Query دقیق و Frontend Filtering.
/// شماره‌گذاری بر اساس Stage، تا Readability بالا باشد.
/// </summary>
public enum TradingEventType
{
    // ─── Lifecycle (1-9) ───
    RunStarted = 1,
    RunStopped = 2,
    RunFaulted = 3,
    RunRestoredFromRecovery = 4,

    // ─── Stage 1: MarketData (10-19) ───
    MarketDataFetched = 10,
    MarketDataFetchFailed = 11,
    MarketClosedSkipped = 12,
    SnapshotUpdated = 13,

    // ─── Stage 2: StrategyEvaluation (20-29) ───
    CandlesLoaded = 20,
    CandlesLoadFailed = 21,
    CurrencyTransformed = 22,
    IndicatorCalculated = 23,
    SignalGenerated = 24,
    NoSignalGenerated = 25,
    StrategyEvaluationFailed = 26,

    // ─── Stage 3: Execution (30-39) ───
    SignalConsumed = 30,
    OrderIntentCreated = 31,
    OrderIntentSkipped = 32,
    CapitalFetched = 33,
    PositionSized = 34,
    OrderDispatched = 35,
    OrderDispatchFailed = 36,

    // ─── Stage 4: OrderLifecycle (40-49) ───
    OrderStatusUpdated = 40,
    TradeReceived = 41,
    TradeCanceled = 42,
    PositionOpened = 43,
    PositionIncreased = 44,
    PositionReduced = 45,
    PositionClosed = 46,
    StopLossTriggered = 47,
    TakeProfitTriggered = 48,
    EvaluationOutcomeRecorded = 49,

    // ─── Recovery (50-59) ───
    RecoveryStarted = 50,
    RecoveryCompleted = 51,
    RecoveryFailed = 52,

    // ─── System (60-69) ───
    WorkerTickStarted = 60,
    WorkerTickCompleted = 61,
    WorkerTickFailed = 62,
    RetentionCleanupCompleted = 63,
}