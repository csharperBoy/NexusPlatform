namespace Trader.Application.Dtos.Positions;

public sealed record PositionView(
    Guid Id,
    Guid RunId,
    Guid TraderAccountId,
    string Isin,
    int State,
    string StateName,
    long Quantity,
    decimal AverageEntryPrice,
    decimal RealizedPnL,
    decimal? UnrealizedPnL,
    decimal? StopLossPrice,
    decimal? TakeProfitPrice,
    DateTime OpenedAt,
    DateTime? ClosedAt);