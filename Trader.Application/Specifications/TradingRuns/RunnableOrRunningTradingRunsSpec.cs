using Core.Domain.Specifications;
using Trader.Domain.Entities;
using Trader.Domain.Enums;

namespace Trader.Application.Specifications.TradingRuns;

/// <summary>
/// Runهایی که برای Reconciliation باید بررسی شوند:
/// Running (در حال اجرا) یا Ready (آماده اجرا).
/// </summary>
public sealed class RunnableOrRunningTradingRunsSpec : BaseSpecification<TradingRun>
{
    public RunnableOrRunningTradingRunsSpec()
        : base(r => r.State == TradingRunState.Running
                 || r.State == TradingRunState.Ready)
    {
        AddInclude(r => r.Accounts);
    }
}