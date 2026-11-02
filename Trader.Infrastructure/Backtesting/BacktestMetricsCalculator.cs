using Trader.Application.Abstractions.Backtesting;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Backtesting;

public static class BacktestMetricsCalculator
{
    public static BacktestMetrics Calculate(
        decimal initialCapital,
        decimal finalEquity,
        IReadOnlyList<BacktestTrade> trades,
        IReadOnlyList<BacktestDailyEquity> dailyEquities)
    {
        // ─── Total Return ───
        var totalReturnPct = initialCapital > 0m
            ? (finalEquity - initialCapital) / initialCapital * 100m
            : 0m;

        // ─── Max Drawdown ───
        var maxDrawdownPct = 0m;
        var peak = initialCapital;
        foreach (var eq in dailyEquities.OrderBy(e => e.Date))
        {
            if (eq.Equity > peak) peak = eq.Equity;
            if (peak > 0m)
            {
                var dd = (peak - eq.Equity) / peak * 100m;
                if (dd > maxDrawdownPct) maxDrawdownPct = dd;
            }
        }

        // ─── Sharpe Ratio (annualized) ───
        var sharpe = 0m;
        var dailyReturns = dailyEquities
            .OrderBy(e => e.Date)
            .Select(e => (double)e.DailyReturnPct / 100.0)
            .ToList();

        if (dailyReturns.Count >= 2)
        {
            var mean = dailyReturns.Average();
            var variance = dailyReturns
                .Sum(r => (r - mean) * (r - mean)) / (dailyReturns.Count - 1);
            var std = Math.Sqrt(variance);

            if (std > 1e-10)
                sharpe = (decimal)(mean / std * Math.Sqrt(252.0));
        }

        // ─── Trades ───
        var wins = trades.Where(t => t.NetPnL > 0m).ToList();
        var losses = trades.Where(t => t.NetPnL < 0m).ToList();
        var flat = trades.Count - wins.Count - losses.Count;

        var winRate = trades.Count > 0
            ? (decimal)wins.Count / trades.Count * 100m
            : 0m;

        var totalWins = wins.Sum(t => t.NetPnL);
        var totalLosses = losses.Sum(t => t.NetPnL);

        var profitFactor = totalLosses != 0m
            ? totalWins / Math.Abs(totalLosses)
            : (totalWins > 0m ? 999m : 0m);

        var avgWin = wins.Count > 0 ? totalWins / wins.Count : 0m;
        var avgLoss = losses.Count > 0 ? totalLosses / losses.Count : 0m;

        return new BacktestMetrics(
            TotalReturnPct: Math.Round(totalReturnPct, 4),
            MaxDrawdownPct: Math.Round(maxDrawdownPct, 4),
            SharpeRatio: Math.Round(sharpe, 4),
            WinRate: Math.Round(winRate, 4),
            TotalTrades: trades.Count,
            WinningTrades: wins.Count,
            LosingTrades: losses.Count,
            ProfitFactor: Math.Round(profitFactor, 4),
            AvgWin: Math.Round(avgWin, 4),
            AvgLoss: Math.Round(Math.Abs(avgLoss), 4));
    }
}