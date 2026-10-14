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
    bool IsActive,
    DateTime CreatedAt);