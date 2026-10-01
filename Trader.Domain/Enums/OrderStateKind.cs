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