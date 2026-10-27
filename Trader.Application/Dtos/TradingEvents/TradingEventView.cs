namespace Trader.Application.Dtos.TradingEvents;

public sealed record TradingEventView(
    Guid Id,
    Guid? RunId,
    string? Isin,
    Guid? AccountId,
    Guid? SignalId,
    Guid? OrderIntentId,
    Guid? BrokerOrderId,
    Guid? TradeId,
    Guid? PositionId,
    int Stage,
    string StageName,
    int Type,
    string TypeName,
    int Severity,
    string SeverityName,
    DateTime OccurredAt,
    string Message,
    string? PayloadJson);