using Trader.Domain.Enums;

namespace Trader.Application.Dtos.Reconciliation;

public sealed record AccountReconciliation(
    Guid AccountId,
    string AccountName,
    BrokerType Broker,
    bool Success,
    string? ErrorMessage,
    int DbPositionCount,
    int BrokerPositionCount,
    int MatchedCount,
    IReadOnlyList<PositionMismatch> Mismatches);