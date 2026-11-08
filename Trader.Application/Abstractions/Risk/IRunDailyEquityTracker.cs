namespace Trader.Application.Abstractions.Risk;

public sealed record RunAccountDailyEquityOpening(
    Guid RunId,
    Guid AccountId,
    DateOnly TradingDate,
    decimal OpeningCash,
    decimal OpeningPositionValue,
    decimal OpeningEquity,
    DateTime CapturedAt);

public sealed record RunAccountDailyEquityCurrent(
    RunAccountDailyEquityOpening Opening,
    decimal CurrentCash,
    decimal CurrentPositionValue,
    DateTime LastCashRefreshAt)
{
    public decimal CurrentEquity => CurrentCash + CurrentPositionValue;

    public decimal LossPercent =>
        Opening.OpeningEquity > 0m
            ? Math.Max(0m,
                (Opening.OpeningEquity - CurrentEquity) / Opening.OpeningEquity * 100m)
            : 0m;
}

public interface IRunDailyEquityTracker
{
    Task<RunAccountDailyEquityOpening> GetOrCaptureOpeningAsync(
        Guid runId, Guid accountId, CancellationToken ct = default);

    Task<RunAccountDailyEquityCurrent> GetCurrentAsync(
        Guid runId, Guid accountId, CancellationToken ct = default);
}