using Trader.Domain.Enums;

namespace Trader.Application.Abstractions.TradingRuns;

/// <summary>
/// کنترل‌کننده‌ی Lifecycle یک TradingRun در Runtime.
/// Idempotent است: Start روی Run فعال → Refresh؛ Stop روی Run غیرفعال → No-op.
/// </summary>
public interface ITradingRunManager
{
    /// <summary>Run را فعال می‌کند: Context ساخته و در Cache قرار می‌گیرد.</summary>
    Task StartAsync(Guid runId, CancellationToken ct = default);

    /// <summary>Run را غیرفعال می‌کند. رفتار ClosePositions در صورت نیاز اجرا می‌شود.</summary>
    Task StopAsync(Guid runId, StopRunBehavior behavior, CancellationToken ct = default);

    /// <summary>آیا Run در حال حاضر در Runtime فعال است.</summary>
    bool IsRunning(Guid runId);
}