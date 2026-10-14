namespace Trader.Application.Dtos.Instruments;

public sealed record OrderPresetView(
    Guid Id,
    Guid InstrumentId,
    string Name,
    decimal DefaultPrice,
    long DefaultQuantity,
    int DefaultSide,
    int ValidityType,
    decimal Commission,
    int DefaultOrderMode);