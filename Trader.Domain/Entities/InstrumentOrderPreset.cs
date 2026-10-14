using Core.Domain.Common.EntityProperties;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;

namespace Trader.Domain.Entities;

/// <summary>
/// تنظیمات پیش‌فرض سفارش برای یک Instrument.
/// صرفاً در ارسال دستی و سرخطی استفاده می‌شود، نه در Trading Run.
/// </summary>
public class InstrumentOrderPreset : BaseEntity, IAuditableEntity
{
    public Guid InstrumentId { get; private set; }
    public string Name { get; private set; } = default!;
    public Price DefaultPrice { get; private set; }
    public Quantity DefaultQuantity { get; private set; }
    public OrderSide DefaultSide { get; private set; }
    public int ValidityType { get; private set; }
    public decimal Commission { get; private set; }
    public OrderMode DefaultOrderMode { get; private set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public void Touch() => ModifiedAt = DateTime.UtcNow;

    private InstrumentOrderPreset() { } // EF

    public static InstrumentOrderPreset Create(
        Guid instrumentId,
        string name,
        Price defaultPrice,
        Quantity defaultQuantity,
        OrderSide defaultSide,
        int validityType,
        decimal commission,
        OrderMode defaultOrderMode)
    {
        return new InstrumentOrderPreset
        {
            InstrumentId = instrumentId,
            Name = name,
            DefaultPrice = defaultPrice,
            DefaultQuantity = defaultQuantity,
            DefaultSide = defaultSide,
            ValidityType = validityType,
            Commission = commission,
            DefaultOrderMode = defaultOrderMode,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name,
        Price defaultPrice,
        Quantity defaultQuantity,
        OrderSide defaultSide,
        int validityType,
        decimal commission,
        OrderMode defaultOrderMode)
    {
        Name = name;
        DefaultPrice = defaultPrice;
        DefaultQuantity = defaultQuantity;
        DefaultSide = defaultSide;
        ValidityType = validityType;
        Commission = commission;
        DefaultOrderMode = defaultOrderMode;
        Touch();
    }
}