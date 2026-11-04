namespace Trader.Application.Dtos.Reconciliation;

public sealed record ReconciliationResult(
    int AccountsChecked,
    int MatchedPositions,
    int QuantityMismatches,
    int DbOnlyPositions,
    int BrokerOnlyPositions,
    int FailedAccounts,
    IReadOnlyList<AccountReconciliation> AccountDetails,
    DateTime StartedAt,
    DateTime CompletedAt,
    long DurationMs);