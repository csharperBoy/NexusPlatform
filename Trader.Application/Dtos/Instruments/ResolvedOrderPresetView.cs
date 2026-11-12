using Trader.Domain.Enums;

namespace Trader.Application.Dtos.Instruments;

public sealed record ResolvedOrderPresetView(
    Guid PresetId,
    Guid InstrumentId,
    string InstrumentIsin,
    string InstrumentSymbolName,

    decimal Price,
    string PriceSource,
    long Quantity,
    string QuantitySource,
    int Side,
    string SideName,
    int DefaultOrderMode,
    decimal Commission,

    long? LiveHighAllowed,
    long? LiveLowAllowed,
    long? LiveLastTraded,
    long? LiveClosing,
    long? LiveFirstTraded,

    Guid? UsedAccountId,
    string? UsedAccountName,
    string? Warning);

public static class OrderPresetStrategyNames
{
    public static string PriceStrategy(OrderPriceStrategy s) => s switch
    {
        OrderPriceStrategy.Fixed => "ثابت",
        OrderPriceStrategy.HighAllowed => "سقف مجاز روزانه",
        OrderPriceStrategy.LowAllowed => "کف مجاز روزانه",
        OrderPriceStrategy.LastTraded => "آخرین معامله",
        OrderPriceStrategy.Closing => "قیمت پایانی",
        OrderPriceStrategy.FirstTraded => "اولین معامله",
        _ => s.ToString(),
    };

    public static string QuantityStrategy(OrderQuantityStrategy s) => s switch
    {
        OrderQuantityStrategy.Fixed => "ثابت",
        OrderQuantityStrategy.MaxOrderQuantity => "حداکثر مجاز نماد",
        OrderQuantityStrategy.MinOrderQuantity => "حداقل مجاز نماد",
        _ => s.ToString(),
    };
}