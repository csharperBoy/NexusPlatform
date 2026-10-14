using Trader.Domain.ValueObjects;

namespace Trader.Application.Runtime;

public sealed record RunSymbolContext(
    string Isin,
    SymbolAllocation Allocation);