using Trader.Application.Dtos.Dashboard;

namespace Trader.Application.Abstractions.Dashboard;

public interface IDashboardQueryService
{
    /// <summary>
    /// Aggregate تمام اطلاعات صفحه‌ی Dashboard در یک round-trip.
    /// Read-only — هیچ side effect ندارد (به‌جز RefreshAsync روی RiskStateTracker).
    /// </summary>
    Task<DashboardSummaryView> GetSummaryAsync(CancellationToken ct = default);
}