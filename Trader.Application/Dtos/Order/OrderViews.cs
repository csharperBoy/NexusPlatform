namespace Trader.Application.Dtos.Orders;

/* ═══════════════ BrokerOrder ═══════════════ */

public sealed record BrokerOrderView(
    Guid Id,
    Guid? OrderIntentId,
    Guid RunId,
    Guid TraderAccountId,
    string? AccountName,
    string Isin,
    string? SymbolName,
    int Side,
    string SideName,
    decimal Price,
    long Quantity,
    long FilledQuantity,
    decimal FilledRatio,
    int State,
    string StateName,
    bool IsTerminal,
    bool IsPaper,
    string? BrokerOrderId,
    DateTime SubmittedAt,
    DateTime? LastSyncedAt,
    DateTime CreatedAt);

public sealed record BrokerOrderDetailView(
    BrokerOrderView Summary,
    IReadOnlyList<OrderTradeView> Trades);

public sealed record OrderTradeView(
    Guid Id,
    long TradeNumber,
    decimal Price,
    long Quantity,
    DateTimeOffset TradeDate,
    bool IsCanceled,
    DateTimeOffset? CancelDateTime,
    DateTime CreatedAt);

/* ═══════════════ OrderIntent ═══════════════ */

public sealed record OrderIntentView(
    Guid Id,
    Guid RunId,
    Guid TraderAccountId,
    string? AccountName,
    Guid? SourceSignalId,
    Guid? SourceEvaluationId,
    string Isin,
    string? SymbolName,
    int Side,
    string SideName,
    decimal Price,
    long Quantity,
    int OrderMode,
    string OrderModeName,
    int State,
    string StateName,
    string? Reason,
    string? FailureReason,
    decimal? StopLossPrice,
    decimal? TakeProfitPrice,
    bool IsPaper,
    DateTime? DispatchedAt,
    DateTime? CompletedAt,
    DateTime CreatedAt);