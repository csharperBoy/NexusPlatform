using Core.Domain.Common.EntityProperties;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;

namespace Trader.Domain.Entities;

public class InstrumentOrderPreset : BaseEntity, IAuditableEntity
{
    public Guid InstrumentId { get; private set; }
    public string Name { get; private set; } = default!;

    /* ═══════════ Price ═══════════ */
    public OrderPriceStrategy PriceStrategy { get; private set; }
    public Price? FixedPrice { get; private set; }

    /* ═══════════ Quantity ═══════════ */
    public OrderQuantityStrategy QuantityStrategy { get; private set; }
    public Quantity? FixedQuantity { get; private set; }

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
        OrderPriceStrategy priceStrategy,
        Price? fixedPrice,
        OrderQuantityStrategy quantityStrategy,
        Quantity? fixedQuantity,
        OrderSide defaultSide,
        int validityType,
        decimal commission,
        OrderMode defaultOrderMode)
    {
        ValidatePrice(priceStrategy, fixedPrice);
        ValidateQuantity(quantityStrategy, fixedQuantity);

        return new InstrumentOrderPreset
        {
            InstrumentId = instrumentId,
            Name = name?.Trim() ?? "",
            PriceStrategy = priceStrategy,
            FixedPrice = fixedPrice,
            QuantityStrategy = quantityStrategy,
            FixedQuantity = fixedQuantity,
            DefaultSide = defaultSide,
            ValidityType = validityType,
            Commission = commission,
            DefaultOrderMode = defaultOrderMode,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void Update(
        string name,
        OrderPriceStrategy priceStrategy,
        Price? fixedPrice,
        OrderQuantityStrategy quantityStrategy,
        Quantity? fixedQuantity,
        OrderSide defaultSide,
        int validityType,
        decimal commission,
        OrderMode defaultOrderMode)
    {
        ValidatePrice(priceStrategy, fixedPrice);
        ValidateQuantity(quantityStrategy, fixedQuantity);

        Name = name?.Trim() ?? "";
        PriceStrategy = priceStrategy;
        FixedPrice = fixedPrice;
        QuantityStrategy = quantityStrategy;
        FixedQuantity = fixedQuantity;
        DefaultSide = defaultSide;
        ValidityType = validityType;
        Commission = commission;
        DefaultOrderMode = defaultOrderMode;
        Touch();
    }

    private static void ValidatePrice(OrderPriceStrategy strategy, Price? fixedPrice)
    {
        if (strategy == OrderPriceStrategy.Fixed)
        {
            if (fixedPrice is null || fixedPrice.Value.Value <= 0m)
                throw new InvalidOperationException(
                    "FixedPrice is required and must be > 0 when PriceStrategy=Fixed.");
        }
        else if (fixedPrice is not null)
        {
            throw new InvalidOperationException(
                "FixedPrice must be null when PriceStrategy != Fixed.");
        }
    }

    private static void ValidateQuantity(
        OrderQuantityStrategy strategy, Quantity? fixedQuantity)
    {
        if (strategy == OrderQuantityStrategy.Fixed)
        {
            if (fixedQuantity is null || fixedQuantity.Value.Value <= 0)
                throw new InvalidOperationException(
                    "FixedQuantity is required and must be > 0 when QuantityStrategy=Fixed.");
        }
        else if (fixedQuantity is not null)
        {
            throw new InvalidOperationException(
                "FixedQuantity must be null when QuantityStrategy != Fixed.");
        }
    }
}