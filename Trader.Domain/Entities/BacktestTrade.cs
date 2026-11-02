using Core.Domain.Common.EntityProperties;
using Trader.Domain.Enums;

namespace Trader.Domain.Entities;

/// <summary>
/// یک Round-Trip (چرخه‌ی کامل) در Backtest.
/// Entry = مجموع چند Fill (برای Stepped Buying).
/// Exit = یک Fill (SELL کامل Position).
/// </summary>
public class BacktestTrade : BaseEntity
{
    public Guid BacktestRunId { get; private set; }
    public string Isin { get; private set; } = default!;

    // ═══════════════ Entry ═══════════════
    public DateTime EntryTime { get; private set; }
    public DateTime LastEntryTime { get; private set; }
    public long TotalEntryQuantity { get; private set; }
    public decimal WeightedAvgEntryPrice { get; private set; }
    public decimal TotalBuyCommission { get; private set; }

    /// <summary>JSON array از Fillهای ورود: [{time, price, qty}, ...]</summary>
    public string EntryFillsJson { get; private set; } = "[]";

    // ═══════════════ Exit ═══════════════
    public DateTime ExitTime { get; private set; }
    public decimal ExitPrice { get; private set; }
    public long ExitQuantity { get; private set; }
    public decimal TotalSellCommission { get; private set; }
    public BacktestExitReason ExitReason { get; private set; }

    // ═══════════════ PnL ═══════════════
    public decimal GrossPnL { get; private set; }
    public decimal NetPnL { get; private set; }
    public decimal NetPnLPct { get; private set; }
    public long HoldingDurationSeconds { get; private set; }

    private BacktestTrade() { } // EF

    public static BacktestTrade Create(
        Guid backtestRunId,
        string isin,
        DateTime entryTime,
        DateTime lastEntryTime,
        long totalEntryQuantity,
        decimal weightedAvgEntryPrice,
        decimal totalBuyCommission,
        string entryFillsJson,
        DateTime exitTime,
        decimal exitPrice,
        long exitQuantity,
        decimal totalSellCommission,
        BacktestExitReason exitReason)
    {
        var grossPnl = (exitPrice - weightedAvgEntryPrice) * exitQuantity;
        var netPnl = grossPnl - totalBuyCommission - totalSellCommission;
        var totalInvested = weightedAvgEntryPrice * totalEntryQuantity + totalBuyCommission;
        var netPnlPct = totalInvested > 0m ? (netPnl / totalInvested) * 100m : 0m;
        var duration = (long)(exitTime - entryTime).TotalSeconds;

        return new BacktestTrade
        {
            BacktestRunId = backtestRunId,
            Isin = isin,
            EntryTime = entryTime,
            LastEntryTime = lastEntryTime,
            TotalEntryQuantity = totalEntryQuantity,
            WeightedAvgEntryPrice = weightedAvgEntryPrice,
            TotalBuyCommission = totalBuyCommission,
            EntryFillsJson = entryFillsJson,
            ExitTime = exitTime,
            ExitPrice = exitPrice,
            ExitQuantity = exitQuantity,
            TotalSellCommission = totalSellCommission,
            ExitReason = exitReason,
            GrossPnL = grossPnl,
            NetPnL = netPnl,
            NetPnLPct = netPnlPct,
            HoldingDurationSeconds = duration
        };
    }
}