namespace Trader.ExternalApiClient.EasyTrader;

public enum EasyTraderOrderStateKind
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