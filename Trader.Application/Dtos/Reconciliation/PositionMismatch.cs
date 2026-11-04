namespace Trader.Application.Dtos.Reconciliation;

public sealed record PositionMismatch(
    string Isin,
    PositionMismatchKind Kind,
    long DbQuantity,
    long BrokerQuantity,
    decimal DbAvgPrice,
    decimal BrokerAvgPrice);