using Core.Domain.Common;
using Core.Domain.Common.EntityProperties;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;

namespace Trader.Domain.Entities;

public class Instrument : BaseEntity, IAuditableEntity
{
    public string Isin { get; private set; } = default!;
    public string SymbolName { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public InstrumentType Type { get; private set; }
    public MarketType Market { get; private set; }
    public string? BoardCode { get; private set; }
    public OptionContract? OptionContract { get; private set; }
    public Guid? TradingScheduleId { get; private set; }
    public TradingSchedule? TradingSchedule { get; private set; }
    public bool IsActive { get; private set; }

    // ═══════════════ NEW: Commission + Order Limits ═══════════════
    /// <summary>کمیسیون خرید (نسبت به مبلغ سفارش). مثلاً 0.0037 = 0.37%</summary>
    public decimal BuyCommissionRate { get; private set; } = 0.0037m;

    /// <summary>کمیسیون فروش.</summary>
    public decimal SellCommissionRate { get; private set; } = 0.0037m;

    /// <summary>حداقل تعداد سهم در یک سفارش.</summary>
    public long MinOrderQuantity { get; private set; } = 1;

    /// <summary>حداکثر تعداد سهم در یک سفارش.</summary>
    public long MaxOrderQuantity { get; private set; } = 100000;

    /// <summary>حداقل ارزش ریالی یک سفارش خرید (اختیاری — مثلاً برای اهرم).</summary>
    public long? MinOrderValueRial { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public void Touch() => ModifiedAt = DateTime.UtcNow;

    private Instrument() { } // EF

    public static Instrument Create(
        string isin,
        string symbolName,
        string displayName,
        InstrumentType type,
        MarketType market,
        string? boardCode = null,
        OptionContract? optionContract = null,
        Guid? tradingScheduleId = null,
        decimal buyCommissionRate = 0.0037m,
        decimal sellCommissionRate = 0.0037m,
        long minOrderQuantity = 1,
        long maxOrderQuantity = 100000,
        long? minOrderValueRial = null)
    {
        if (string.IsNullOrWhiteSpace(isin))
            throw new ArgumentException("ISIN is required.", nameof(isin));

        if (type == InstrumentType.Option && optionContract is null)
            throw new InvalidOperationException(
                "Option contract details are required for Option instruments.");

        ValidateCommission(buyCommissionRate, nameof(buyCommissionRate));
        ValidateCommission(sellCommissionRate, nameof(sellCommissionRate));
        ValidateOrderLimits(minOrderQuantity, maxOrderQuantity, minOrderValueRial);

        return new Instrument
        {
            Isin = isin.Trim(),
            SymbolName = symbolName.Trim(),
            DisplayName = displayName.Trim(),
            Type = type,
            Market = market,
            BoardCode = boardCode,
            OptionContract = optionContract,
            TradingScheduleId = tradingScheduleId,
            BuyCommissionRate = buyCommissionRate,
            SellCommissionRate = sellCommissionRate,
            MinOrderQuantity = minOrderQuantity,
            MaxOrderQuantity = maxOrderQuantity,
            MinOrderValueRial = minOrderValueRial,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public bool ApplyChange(
        Optional<string?> symbolName,
        Optional<string?> displayName,
        Optional<string?> boardCode,
        Optional<Guid?> tradingScheduleId,
        Optional<decimal> buyCommissionRate,
        Optional<decimal> sellCommissionRate,
        Optional<long> minOrderQuantity,
        Optional<long> maxOrderQuantity,
        Optional<long?> minOrderValueRial)
    {
        bool hasChange = false;

        if (symbolName.IsSet)
        {
            var v = symbolName.Value?.Trim();
            if (string.IsNullOrWhiteSpace(v))
                throw new InvalidOperationException("SymbolName cannot be empty.");
            if (v != SymbolName) { SymbolName = v; hasChange = true; }
        }

        if (displayName.IsSet)
        {
            var v = displayName.Value?.Trim();
            if (string.IsNullOrWhiteSpace(v))
                throw new InvalidOperationException("DisplayName cannot be empty.");
            if (v != DisplayName) { DisplayName = v; hasChange = true; }
        }

        if (boardCode.IsSet)
        {
            var v = boardCode.Value?.Trim();
            if (v != BoardCode) { BoardCode = v; hasChange = true; }
        }

        if (tradingScheduleId.IsSet && tradingScheduleId.Value != TradingScheduleId)
        {
            TradingScheduleId = tradingScheduleId.Value;
            hasChange = true;
        }

        if (buyCommissionRate.IsSet)
        {
            ValidateCommission(buyCommissionRate.Value, nameof(buyCommissionRate));
            if (buyCommissionRate.Value != BuyCommissionRate)
            {
                BuyCommissionRate = buyCommissionRate.Value;
                hasChange = true;
            }
        }

        if (sellCommissionRate.IsSet)
        {
            ValidateCommission(sellCommissionRate.Value, nameof(sellCommissionRate));
            if (sellCommissionRate.Value != SellCommissionRate)
            {
                SellCommissionRate = sellCommissionRate.Value;
                hasChange = true;
            }
        }

        // ─── Order limits — validate ترکیبی ───
        var newMin = minOrderQuantity.IsSet ? minOrderQuantity.Value : MinOrderQuantity;
        var newMax = maxOrderQuantity.IsSet ? maxOrderQuantity.Value : MaxOrderQuantity;
        var newMinValue = minOrderValueRial.IsSet ? minOrderValueRial.Value : MinOrderValueRial;

        if (minOrderQuantity.IsSet || maxOrderQuantity.IsSet || minOrderValueRial.IsSet)
        {
            ValidateOrderLimits(newMin, newMax, newMinValue);

            if (newMin != MinOrderQuantity) { MinOrderQuantity = newMin; hasChange = true; }
            if (newMax != MaxOrderQuantity) { MaxOrderQuantity = newMax; hasChange = true; }
            if (newMinValue != MinOrderValueRial) { MinOrderValueRial = newMinValue; hasChange = true; }
        }

        if (hasChange) Touch();
        return hasChange;
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    /* ═══════════════ Validation Helpers ═══════════════ */

    private static void ValidateCommission(decimal rate, string paramName)
    {
        if (rate < 0m || rate > 0.1m)
            throw new ArgumentOutOfRangeException(paramName,
                "Commission rate must be between 0 and 0.1 (0%-10%).");
    }

    private static void ValidateOrderLimits(
        long minQty, long maxQty, long? minValueRial)
    {
        if (minQty <= 0)
            throw new ArgumentOutOfRangeException(nameof(minQty),
                "MinOrderQuantity must be greater than zero.");

        if (maxQty < minQty)
            throw new ArgumentException(
                "MaxOrderQuantity must be >= MinOrderQuantity.");

        if (minValueRial.HasValue && minValueRial.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(minValueRial),
                "MinOrderValueRial must be greater than zero when provided.");
    }
}