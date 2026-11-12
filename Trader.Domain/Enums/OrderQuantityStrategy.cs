namespace Trader.Domain.Enums;

public enum OrderQuantityStrategy
{
    /// <summary>تعداد ثابت (FixedQuantity).</summary>
    Fixed = 0,

    /// <summary>حداکثر تعداد سفارش مجاز برای این نماد (Instrument.MaxOrderQuantity).</summary>
    MaxOrderQuantity = 1,

    /// <summary>حداقل تعداد سفارش مجاز برای این نماد (Instrument.MinOrderQuantity).</summary>
    MinOrderQuantity = 2,
}