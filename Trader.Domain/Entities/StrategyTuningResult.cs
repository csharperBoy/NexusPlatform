// Trader.Domain/Entities/StrategyTuningResult.cs
using Core.Domain.Common.EntityProperties;
using Trader.Domain.Enums;

namespace Trader.Domain.Entities;

public class StrategyTuningResult : BaseEntity
{
    public Guid TuningRunId { get; private set; }
    public string ConfigJson { get; private set; } = default!;
    public int Generation { get; private set; }
    public TuningResultStatus Status { get; private set; }
    public long RuntimeMs { get; private set; }

    // Yearly
    public decimal? AvgReturnArithmeticYearly { get; private set; }
    public decimal? AvgReturnCompoundYearly { get; private set; }
    public int? ValidWindowsYearly { get; private set; }
    public int? TotalTradesYearly { get; private set; }

    // SemiAnnual
    public decimal? AvgReturnArithmeticSemi { get; private set; }
    public decimal? AvgReturnCompoundSemi { get; private set; }
    public int? ValidWindowsSemi { get; private set; }
    public int? TotalTradesSemi { get; private set; }

    // Quarterly
    public decimal? AvgReturnArithmeticQuarter { get; private set; }
    public decimal? AvgReturnCompoundQuarter { get; private set; }
    public int? ValidWindowsQuarter { get; private set; }
    public int? TotalTradesQuarter { get; private set; }

    // Monthly
    public decimal? AvgReturnArithmeticMonthly { get; private set; }
    public decimal? AvgReturnCompoundMonthly { get; private set; }
    public int? ValidWindowsMonthly { get; private set; }
    public int? TotalTradesMonthly { get; private set; }

    public bool IsTop10InYearly { get; private set; }
    public bool IsTop10InSemi { get; private set; }
    public bool IsTop10InQuarterly { get; private set; }
    public bool IsTop10InMonthly { get; private set; }

    public string? WindowResultsJson { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    private StrategyTuningResult() { }

    public static StrategyTuningResult Create(
        Guid tuningRunId, string configJson, int generation,
        TuningResultStatus status, long runtimeMs,
        ScaleMetrics? yearly, ScaleMetrics? semi, ScaleMetrics? quarterly, ScaleMetrics? monthly,
        string? windowResultsJson = null, string? errorMessage = null)
    {
        return new StrategyTuningResult
        {
            TuningRunId = tuningRunId,
            ConfigJson = configJson,
            Generation = generation,
            Status = status,
            RuntimeMs = runtimeMs,
            AvgReturnArithmeticYearly = yearly?.AvgArithmetic,
            AvgReturnCompoundYearly = yearly?.AvgCompound,
            ValidWindowsYearly = yearly?.ValidWindows,
            TotalTradesYearly = yearly?.TotalTrades,
            AvgReturnArithmeticSemi = semi?.AvgArithmetic,
            AvgReturnCompoundSemi = semi?.AvgCompound,
            ValidWindowsSemi = semi?.ValidWindows,
            TotalTradesSemi = semi?.TotalTrades,
            AvgReturnArithmeticQuarter = quarterly?.AvgArithmetic,
            AvgReturnCompoundQuarter = quarterly?.AvgCompound,
            ValidWindowsQuarter = quarterly?.ValidWindows,
            TotalTradesQuarter = quarterly?.TotalTrades,
            AvgReturnArithmeticMonthly = monthly?.AvgArithmetic,
            AvgReturnCompoundMonthly = monthly?.AvgCompound,
            ValidWindowsMonthly = monthly?.ValidWindows,
            TotalTradesMonthly = monthly?.TotalTrades,
            WindowResultsJson = windowResultsJson,
            ErrorMessage = errorMessage,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void MarkTop10(TuningScale scale)
    {
        if (scale.HasFlag(TuningScale.Yearly)) IsTop10InYearly = true;
        if (scale.HasFlag(TuningScale.SemiAnnual)) IsTop10InSemi = true;
        if (scale.HasFlag(TuningScale.Quarterly)) IsTop10InQuarterly = true;
        if (scale.HasFlag(TuningScale.Monthly)) IsTop10InMonthly = true;
    }

    public void AttachWindowResults(string json) => WindowResultsJson = json;
}

public sealed record ScaleMetrics(
    decimal AvgArithmetic, decimal AvgCompound, int ValidWindows, int TotalTrades);
