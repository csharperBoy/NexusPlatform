namespace Trader.Application.Runtime;

/// <summary>
/// نمای لحظه‌ای یک Instrument برای Strategy.
/// Immutable، مستقل از DTO کارگزاری. Stage 1 آن را می‌سازد.
/// </summary>
public sealed record MarketSnapshot(
    /// <summary>ISIN نماد — کلید اصلی در Cache.</summary>
    string Isin,

    /// <summary>نام نماد — برای Display در Logs.</summary>
    string SymbolName,

    /// <summary>آخرین قیمت معامله‌شده.</summary>
    long LastPrice,

    /// <summary>قیمت پایانی روز قبل.</summary>
    long ClosePrice,

    /// <summary>اولین قیمت معامله‌شده امروز.</summary>
    long FirstPrice,

    /// <summary>حداکثر قیمت مجاز امروز.</summary>
    long HighAllowedPrice,

    /// <summary>حداقل قیمت مجاز امروز.</summary>
    long LowAllowedPrice,

    /// <summary>حجم معاملات امروز (اگر موجود).</summary>
    long Volume,

    /// <summary>زمان Snapshot در سرور ما (UTC).</summary>
    DateTime CapturedAt,

    /// <summary>منبع داده — BrokerType یا نام Provider.</summary>
    string Source,

    /// <summary>فیلدهای اضافی مخصوص هر Broker — برای Strategyهای خاص.</summary>
    IReadOnlyDictionary<string, object?>? Extra = null);