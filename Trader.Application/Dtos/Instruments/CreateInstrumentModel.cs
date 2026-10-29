namespace Trader.Application.Dtos.Instruments;

public sealed record CreateInstrumentModel(
    string Isin,
    string SymbolName,
    string DisplayName,
    int Type,
    int Market,
    string? BoardCode,
    Guid? TradingScheduleId,
    decimal BuyCommissionRate,
    decimal SellCommissionRate,
    long MinOrderQuantity,
    long MaxOrderQuantity,
    long? MinOrderValueRial,
    OptionContractModel? OptionContract);

public sealed record OptionContractModel(
    int OptionType,
    decimal StrikePrice,
    DateOnly ExpirationDate,
    string UnderlyingIsin);