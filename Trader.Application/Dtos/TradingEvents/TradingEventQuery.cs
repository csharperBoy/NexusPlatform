namespace Trader.Application.Dtos.TradingEvents;

public sealed record TradingEventQuery(
    Guid? RunId = null,
    string? Isin = null,
    Guid? AccountId = null,
    int? Stage = null,
    int? Severity = null,
    int? MinSeverity = null,
    DateTime? From = null,
    DateTime? To = null,
    string? Search = null,
    int Skip = 0,
    int Take = 100);