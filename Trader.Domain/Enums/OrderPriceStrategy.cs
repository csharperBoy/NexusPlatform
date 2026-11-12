namespace Trader.Domain.Enums;

public enum OrderPriceStrategy
{
    /// <summary>قیمت ثابت (FixedPrice).</summary>
    Fixed = 0,

    /// <summary>سقف مجاز روزانه از API لحظه‌ای.</summary>
    HighAllowed = 1,

    /// <summary>کف مجاز روزانه از API لحظه‌ای.</summary>
    LowAllowed = 2,

    /// <summary>آخرین قیمت معامله‌شده.</summary>
    LastTraded = 3,

    /// <summary>قیمت پایانی (Closing).</summary>
    Closing = 4,

    /// <summary>اولین قیمت معامله‌شده.</summary>
    FirstTraded = 5,
}