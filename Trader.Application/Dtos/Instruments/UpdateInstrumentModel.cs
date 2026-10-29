namespace Trader.Application.Dtos.Instruments;

public sealed record UpdateInstrumentModel(
    string SymbolName,
    string DisplayName,
    string? BoardCode,
    Guid? TradingScheduleId,
    decimal BuyCommissionRate,
    decimal SellCommissionRate,
    long MinOrderQuantity,
    long MaxOrderQuantity,
    long? MinOrderValueRial);