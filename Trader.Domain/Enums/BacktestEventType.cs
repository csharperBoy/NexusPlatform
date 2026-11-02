namespace Trader.Domain.Enums;

public enum BacktestEventType
{
    RunStarted = 1,
    RunCompleted = 2,
    RunFailed = 3,

    ConfigLoaded = 10,
    InstrumentsLoaded = 11,
    CandlesLoaded = 12,
    EventsBuilt = 13,

    SimulationStarted = 20,
    SignalGenerated = 21,
    FillExecuted = 22,
    PositionOpened = 23,
    PositionIncreased = 24,
    PositionClosed = 25,
    SLTPTriggered = 26,

    MetricsComputed = 30,
}