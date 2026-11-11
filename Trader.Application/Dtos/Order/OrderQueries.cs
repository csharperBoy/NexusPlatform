namespace Trader.Application.Dtos.Orders;

public sealed record BrokerOrderQuery(
    Guid? RunId = null,
    Guid? AccountId = null,
    string? Isin = null,
    int? State = null,
    bool? IsPaper = null,
    DateTime? SubmittedFrom = null,
    DateTime? SubmittedTo = null,
    string? Search = null,           // ISIN یا BrokerOrderId substring
    int Skip = 0,
    int Take = 100);

public sealed record OrderIntentQuery(
    Guid? RunId = null,
    Guid? AccountId = null,
    string? Isin = null,
    int? State = null,
    bool? IsPaper = null,
    DateTime? From = null,
    DateTime? To = null,
    string? Search = null,           // ISIN یا FailureReason substring
    int Skip = 0,
    int Take = 100);