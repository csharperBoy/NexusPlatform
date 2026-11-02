namespace Trader.Domain.Enums;

public enum BacktestEventType
{
    // ─── Lifecycle (1-9) ───
    RunStarted = 1,
    RunCompleted = 2,
    RunFailed = 3,

    // ─── Setup (10-19) ───
    ConfigLoaded = 10,
    InstrumentsLoaded = 11,
    CandlesLoaded = 12,
    SubCandlesLoaded = 13,
    EventsBuilt = 14,

    // ─── Simulation (20-39) ───
    SimulationStarted = 20,
    StrategyEvaluated = 21,
    SignalGenerated = 22,
    OrderPending = 23,
    FillExecuted = 24,
    PositionOpened = 25,
    PositionIncreased = 26,
    PositionClosed = 27,
    SLTPTriggered = 28,

    // ─── Finalization (30-49) ───
    PositionForceClosed = 30,
    MetricsComputed = 40,
}