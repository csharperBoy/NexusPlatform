using Trader.Application.Runtime;

namespace Trader.Application.Abstractions.TradingRuns;

/// <summary>
/// کش In-memory از Run Contextهای فعال. Singleton، Thread-safe.
/// Stage 2 و Stage 3 مستقیم از این می‌خوانند.
/// </summary>
public interface IRunExecutionContextCache
{
    RunRuntimeContext? Get(Guid runId);
    IReadOnlyList<RunRuntimeContext> GetAllActive();
    bool Contains(Guid runId);

    void Set(RunRuntimeContext context);
    void Remove(Guid runId);
    void Refresh(RunRuntimeContext context);
}