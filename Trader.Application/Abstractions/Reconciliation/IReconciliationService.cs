using Trader.Application.Dtos.Reconciliation;

namespace Trader.Application.Abstractions.Reconciliation;

/// <summary>
/// تطبیق Positionهای DB با Portfolio Broker.
/// Phase A1: Log فقط — بدون Auto-fix.
/// </summary>
public interface IReconciliationService
{
    /// <summary>
    /// همه‌ی Accountهایی که Position باز دارند یا در Runهای فعال هستند را Reconcile می‌کند.
    /// </summary>
    Task<ReconciliationResult> ReconcileAllAsync(CancellationToken ct = default);

    /// <summary>فقط یک Account.</summary>
    Task<AccountReconciliation> ReconcileAccountAsync(
        Guid accountId, CancellationToken ct = default);
}