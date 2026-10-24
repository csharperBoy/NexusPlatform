using Core.Domain.Common.EntityProperties;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;

namespace Trader.Domain.Entities;

/// <summary>
/// هویت یک نماد قابل معامله. تنها اطلاعات ثابت.
/// تنظیمات سفارش در InstrumentOrderPreset جدا شده است.
/// </summary>
public class Instrument : BaseEntity, IAuditableEntity
{
    public string Isin { get; private set; } = default!;
    public string SymbolName { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public InstrumentType Type { get; private set; }
    public MarketType Market { get; private set; }
    public string? BoardCode { get; private set; }
    public OptionContract? OptionContract { get; private set; }
    public bool IsActive { get; private set; }
    public Guid? TradingScheduleId { get; private set; }
    public TradingSchedule? TradingSchedule { get; private set; }
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
    Guid? tradingScheduleId = null)
    {
        if (string.IsNullOrWhiteSpace(isin))
            throw new ArgumentException("ISIN is required.", nameof(isin));

        if (type == InstrumentType.Option && optionContract is null)
            throw new InvalidOperationException(
                "Option contract details are required for Option instruments.");

        return new Instrument
        {
            Isin = isin.Trim(),
            SymbolName = symbolName.Trim(),
            DisplayName = displayName.Trim(),
            Type = type,
            Market = market,
            BoardCode = boardCode,
            OptionContract = optionContract,
            TradingScheduleId = tradingScheduleId,   // ← جدید
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string symbolName,
        string displayName,
        string? boardCode,
        Guid? tradingScheduleId)   // ← جدید
    {
        SymbolName = symbolName.Trim();
        DisplayName = displayName.Trim();
        BoardCode = boardCode;
        TradingScheduleId = tradingScheduleId;
        Touch();
    }

    public void SetTradingSchedule(Guid? scheduleId)
    {
        TradingScheduleId = scheduleId;
        Touch();
    }
    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }
}