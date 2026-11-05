using Core.Domain.Common.EntityProperties;
using Trader.Domain.Enums;
using Trader.Domain.Events;
using Trader.Domain.ValueObjects;

namespace Trader.Domain.Entities;

/// <summary>
/// Snapshot کامل از یک Backtest. Immutable بعد از Completed.
/// Config حتی برای Existing Run هم Snapshot می‌شود تا Re-run ممکن باشد.
/// </summary>
public class BacktestRun : BaseEntity, IAuditableEntity, IOwnerableEntity
{
    // ═══════════════ Identity ═══════════════
    public string Name { get; private set; } = default!;

    /// <summary>اگر از Run موجود کپی شده، اینجا ذخیره می‌شود (فقط برای Reference).</summary>
    public Guid? SourceRunId { get; private set; }

    public BacktestStatus Status { get; private set; }

    // ═══════════════ Strategy Config (Snapshot) ═══════════════
    public string StrategyKey { get; private set; } = default!;
    public string StrategyConfigJson { get; private set; } = default!;
    public int PrimaryTimeFrame { get; private set; }
    public string? TargetCurrency { get; private set; }

    // ═══════════════ Execution Config (Snapshot) ═══════════════
    public int CapitalAllocationMode { get; private set; }
    public decimal? FixedCapitalAmount { get; private set; }
    public decimal StrengthMultiplier { get; private set; }
    public decimal MinDeltaPercent { get; private set; }
    public decimal? StopLossPercentage { get; private set; }
    public decimal? StopLossPrice { get; private set; }
    public decimal? TakeProfitPercentage { get; private set; }
    public decimal? TakeProfitPrice { get; private set; }
    public int DefaultOrderMode { get; private set; }
    public int OnMarketClose { get; private set; }
    public int OnStop { get; private set; }

    /// <summary>JSON کامل از تمام Config — Source of Truth برای Re-run.</summary>
    public string ConfigSnapshotJson { get; private set; } = default!;

    // ═══════════════ Backtest-Specific ═══════════════
    public DateTime DateFrom { get; private set; }
    public DateTime DateTo { get; private set; }

    /// <summary>سرمایه‌ی اولیه (مستقل از Run Capital — برای Account مجازی).</summary>
    public decimal InitialCapital { get; private set; }

    /// <summary>درصد Slippage (مثلاً 0.001 = 0.1%).</summary>
    public decimal SlippagePercent { get; private set; } = 0.001m;

    /// <summary>رزولوشن Sub-Candle برای رفع ابهام SL/TP. null = Pessimistic.</summary>
    public int? SubCandleTimeFrame { get; private set; }

    public string TimeZoneId { get; private set; } = "Asia/Tehran";

    // ═══════════════ Result Metrics ═══════════════
    public decimal FinalEquity { get; private set; }
    public decimal TotalReturnPct { get; private set; }
    public decimal MaxDrawdownPct { get; private set; }
    public decimal SharpeRatio { get; private set; }
    public decimal WinRate { get; private set; }
    public int TotalTrades { get; private set; }
    public int WinningTrades { get; private set; }
    public int LosingTrades { get; private set; }
    public decimal ProfitFactor { get; private set; }
    public decimal AvgWin { get; private set; }
    public decimal AvgLoss { get; private set; }
    public decimal TotalCommission { get; private set; }
    public decimal TotalSlippage { get; private set; }

    // ═══════════════ Timing ═══════════════
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public long? DurationMs { get; private set; }
    public string? ErrorMessage { get; private set; }

    // ═══════════════ Children ═══════════════
    private readonly List<BacktestSymbol> _symbols = new();
    public IReadOnlyList<BacktestSymbol> Symbols => _symbols.AsReadOnly();

    // ═══════════════ IAuditableEntity ═══════════════
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public void Touch() => ModifiedAt = DateTime.UtcNow;

    // ═══════════════ IOwnerableEntity ═══════════════
    public Guid? OwnerOrganizationUnitId { get; protected set; }
    public Guid? OwnerPositionId { get; protected set; }
    public Guid? OwnerPersonId { get; protected set; }
    public Guid? OwnerUserId { get; protected set; }

    public void SetOwners(Guid? userId, Guid? personId, Guid? positionId, Guid? orgUnitId)
    {
        OwnerUserId = userId;
        OwnerPersonId = personId;
        OwnerPositionId = positionId;
        OwnerOrganizationUnitId = orgUnitId;
    }
    public void SetPersonOwner(Guid personId) => OwnerPersonId = personId;
    public void SetUserOwner(Guid userId) => OwnerUserId = userId;
    public void SetPositionOwner(Guid positionId) => OwnerPositionId = positionId;
    public void SetOrganizationUnitOwner(Guid orgUnitId) => OwnerOrganizationUnitId = orgUnitId;

    private BacktestRun() { } // EF

    public static BacktestRun Create(
        string name,
        Guid? sourceRunId,
        string strategyKey,
        string strategyConfigJson,
        int primaryTimeFrame,
        string? targetCurrency,
        int capitalAllocationMode,
        decimal? fixedCapitalAmount,
        decimal strengthMultiplier,
        decimal minDeltaPercent,
        decimal? stopLossPercentage,
        decimal? stopLossPrice,
        decimal? takeProfitPercentage,
        decimal? takeProfitPrice,
        int defaultOrderMode,
        int onMarketClose,
        int onStop,
        string configSnapshotJson,
        DateTime dateFrom,
        DateTime dateTo,
        decimal initialCapital,
        decimal slippagePercent,
        int? subCandleTimeFrame,
        string timeZoneId,
        IEnumerable<(string Isin, decimal AllocationPercentage)> symbols)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(strategyKey))
            throw new ArgumentException("StrategyKey is required.", nameof(strategyKey));
        if (string.IsNullOrWhiteSpace(configSnapshotJson))
            throw new ArgumentException("ConfigSnapshotJson is required.", nameof(configSnapshotJson));
        if (dateTo <= dateFrom)
            throw new InvalidOperationException("DateTo must be after DateFrom.");
        if (initialCapital <= 0m)
            throw new ArgumentOutOfRangeException(nameof(initialCapital), "Must be > 0.");
        if (slippagePercent < 0m || slippagePercent > 0.1m)
            throw new ArgumentOutOfRangeException(nameof(slippagePercent), "Must be between 0 and 0.1 (0%-10%).");
        if (strengthMultiplier < 0.1m || strengthMultiplier > 5m)
            throw new ArgumentOutOfRangeException(nameof(strengthMultiplier), "Must be between 0.1 and 5.");
        if (minDeltaPercent < 0m || minDeltaPercent > 1m)
            throw new ArgumentOutOfRangeException(nameof(minDeltaPercent), "Must be between 0 and 1.");

        var run = new BacktestRun
        {
            Name = name.Trim(),
            SourceRunId = sourceRunId,
            Status = BacktestStatus.Pending,
            StrategyKey = strategyKey.Trim(),
            StrategyConfigJson = strategyConfigJson,
            PrimaryTimeFrame = primaryTimeFrame,
            TargetCurrency = CurrencyCode.NormalizeOrNull(targetCurrency, nameof(targetCurrency)),
            CapitalAllocationMode = capitalAllocationMode,
            FixedCapitalAmount = fixedCapitalAmount,
            StrengthMultiplier = strengthMultiplier,
            MinDeltaPercent = minDeltaPercent,
            StopLossPercentage = stopLossPercentage,
            StopLossPrice = stopLossPrice,
            TakeProfitPercentage = takeProfitPercentage,
            TakeProfitPrice = takeProfitPrice,
            DefaultOrderMode = defaultOrderMode,
            OnMarketClose = onMarketClose,
            OnStop = onStop,
            ConfigSnapshotJson = configSnapshotJson,
            DateFrom = dateFrom.Kind == DateTimeKind.Utc ? dateFrom : DateTime.SpecifyKind(dateFrom, DateTimeKind.Utc),
            DateTo = dateTo.Kind == DateTimeKind.Utc ? dateTo : DateTime.SpecifyKind(dateTo, DateTimeKind.Utc),
            InitialCapital = initialCapital,
            SlippagePercent = slippagePercent,
            SubCandleTimeFrame = subCandleTimeFrame,
            TimeZoneId = timeZoneId,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var (isin, pct) in symbols)
        {
            if (string.IsNullOrWhiteSpace(isin))
                throw new ArgumentException("Isin cannot be empty.", nameof(symbols));
            if (pct <= 0m || pct > 100m)
                throw new ArgumentOutOfRangeException(nameof(symbols), $"Allocation for {isin} must be in (0,100].");
            run._symbols.Add(BacktestSymbol.Create(run.Id, isin, pct));
        }

        return run;
    }

    /* ═══════════════ State Transitions ═══════════════ */

    public void MarkRunning()
    {
        if (Status != BacktestStatus.Pending)
            throw new InvalidOperationException($"Cannot start from status {Status}.");
        Status = BacktestStatus.Running;
        StartedAt = DateTime.UtcNow;
        Touch();
    }

    public void MarkCompleted(
        decimal finalEquity,
        decimal totalReturnPct,
        decimal maxDrawdownPct,
        decimal sharpeRatio,
        decimal winRate,
        int totalTrades,
        int winningTrades,
        int losingTrades,
        decimal profitFactor,
        decimal avgWin,
        decimal avgLoss,
        decimal totalCommission,
        decimal totalSlippage)
    {
        if (Status != BacktestStatus.Running)
            throw new InvalidOperationException($"Cannot complete from status {Status}.");

        FinalEquity = finalEquity;
        TotalReturnPct = totalReturnPct;
        MaxDrawdownPct = maxDrawdownPct;
        SharpeRatio = sharpeRatio;
        WinRate = winRate;
        TotalTrades = totalTrades;
        WinningTrades = winningTrades;
        LosingTrades = losingTrades;
        ProfitFactor = profitFactor;
        AvgWin = avgWin;
        AvgLoss = avgLoss;
        TotalCommission = totalCommission;
        TotalSlippage = totalSlippage;

        Status = BacktestStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        DurationMs = StartedAt.HasValue
            ? (long)(CompletedAt.Value - StartedAt.Value).TotalMilliseconds
            : null;
        Touch();
    }

    public void MarkFailed(string errorMessage)
    {
        Status = BacktestStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
        DurationMs = StartedAt.HasValue
            ? (long)(CompletedAt.Value - StartedAt.Value).TotalMilliseconds
            : null;
        Touch();
    }

    public void MarkCancelled()
    {
        Status = BacktestStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
        DurationMs = StartedAt.HasValue
            ? (long)(CompletedAt.Value - StartedAt.Value).TotalMilliseconds
            : null;
        Touch();
    }
}