namespace Trader.Application.Dtos.Instruments;

public sealed record OrderPresetView(
    Guid Id,
    Guid InstrumentId,
    string InstrumentIsin,
    string InstrumentSymbolName,
    string Name,
    int PriceStrategy,
    string PriceStrategyName,
    decimal? FixedPrice,
    int QuantityStrategy,
    string QuantityStrategyName,
    long? FixedQuantity,
    int DefaultSide,
    string DefaultSideName,
    int ValidityType,
    decimal Commission,
    int DefaultOrderMode,
    string DefaultOrderModeName,
    DateTime CreatedAt);