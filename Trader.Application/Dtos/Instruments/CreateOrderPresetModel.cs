namespace Trader.Application.Dtos.Instruments;

public sealed record CreateOrderPresetModel(
    Guid InstrumentId,
    string Name,
    int PriceStrategy,              // OrderPriceStrategy
    decimal? FixedPrice,            // null unless Fixed
    int QuantityStrategy,           // OrderQuantityStrategy
    long? FixedQuantity,            // null unless Fixed
    int DefaultSide,
    int ValidityType,
    decimal Commission,
    int DefaultOrderMode);

public sealed record UpdateOrderPresetModel(
    string Name,
    int PriceStrategy,
    decimal? FixedPrice,
    int QuantityStrategy,
    long? FixedQuantity,
    int DefaultSide,
    int ValidityType,
    decimal Commission,
    int DefaultOrderMode);