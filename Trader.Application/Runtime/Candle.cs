namespace Trader.Application.Runtime;

/// <summary>
/// کندل خالص برای Strategy. مستقل از DTOهای Broker.
/// </summary>
public sealed record Candle(
    DateTime OpenTime,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    long Volume);