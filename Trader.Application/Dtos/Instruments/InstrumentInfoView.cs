namespace Trader.Application.Dtos.Instruments;

public sealed record InstrumentInfoView(
    Guid Id,
    string Isin,
    string SymbolName,
    string DisplayName,
    int Type,
    string TypeName,
    int Market,
    string MarketName,
    string? BoardCode,
    Guid? TradingScheduleId,
    string? TradingScheduleCode,
    string? TradingScheduleName,
    decimal BuyCommissionRate,
    decimal SellCommissionRate,
    long MinOrderQuantity,
    long MaxOrderQuantity,
    long? MinOrderValueRial,
    bool IsActive,
    DateTime CreatedAt);