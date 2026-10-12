using Trader.ExternalApiClient.EasyTrader;

namespace Trader.Domain.Enums;

public enum OrderStateKind
{
    Unknown = 0,
    Pending,
    Open,
    PartiallyExecuted,
    FullyExecuted,
    Canceled,
    Rejected,
    Expired,
}
public static class OrderStateKindExtensions
{
    public static OrderStateKind FromEasyTrader(EasyTraderOrderStateKind state) => state switch
    {
        EasyTraderOrderStateKind.Rejected => OrderStateKind.Rejected,
        EasyTraderOrderStateKind.PartiallyExecuted => OrderStateKind.PartiallyExecuted,
        EasyTraderOrderStateKind.FullyExecuted => OrderStateKind.FullyExecuted,
        EasyTraderOrderStateKind.Canceled => OrderStateKind.Canceled,
        EasyTraderOrderStateKind.Unknown => OrderStateKind.Unknown,
        EasyTraderOrderStateKind.Pending => OrderStateKind.Pending,
        EasyTraderOrderStateKind.Open => OrderStateKind.Open,
        EasyTraderOrderStateKind.Expired => OrderStateKind.Expired,
        _ => OrderStateKind.Unknown
    };

    public static OrderStateKind FromEasyTrader(int state)
    {

       return FromEasyTrader( EasyTraderOrderStateMap.ToKind(state));

    }
}