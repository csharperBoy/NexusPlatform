namespace Trader.Application.Dtos.Instruments;

/// <summary>نمای سبک برای Dropdown انتخاب Symbol در UI.</summary>
public sealed record InstrumentSelectionView(
    Guid Id,
    string Isin,
    string SymbolName,
    string DisplayName);