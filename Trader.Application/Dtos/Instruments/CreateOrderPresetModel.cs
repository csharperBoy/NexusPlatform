namespace Trader.Application.Dtos.Instruments;

public sealed record CreateOrderPresetModel(
    Guid InstrumentId,
    string Name,
    decimal DefaultPrice,
    long DefaultQuantity,
    int DefaultSide,
    int ValidityType,
    decimal Commission,
    int DefaultOrderMode);

public sealed record UpdateOrderPresetModel(
    string Name,
    decimal DefaultPrice,
    long DefaultQuantity,
    int DefaultSide,
    int ValidityType,
    decimal Commission,
    int DefaultOrderMode);