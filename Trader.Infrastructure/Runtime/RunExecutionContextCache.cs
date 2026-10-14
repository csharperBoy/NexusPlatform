using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.TradingRuns;
using Trader.Application.Runtime;

namespace Trader.Infrastructure.Runtime;

/// <summary>
/// کش In-memory از Run Contextهای فعال. Singleton، Thread-safe.
/// </summary>
public sealed class RunExecutionContextCache : IRunExecutionContextCache
{
    private readonly ConcurrentDictionary<Guid, RunRuntimeContext> _cache = new();
    private readonly ILogger<RunExecutionContextCache> _logger;

    public RunExecutionContextCache(ILogger<RunExecutionContextCache> logger)
    {
        _logger = logger;
    }

    public RunRuntimeContext? Get(Guid runId)
        => _cache.TryGetValue(runId, out var ctx) ? ctx : null;

    public IReadOnlyList<RunRuntimeContext> GetAllActive()
        => _cache.Values.ToList();

    public bool Contains(Guid runId) => _cache.ContainsKey(runId);

    public void Set(RunRuntimeContext context)
    {
        _cache[context.RunId] = context;
        _logger.LogInformation(
            "Run context cached: {RunId} strategy={Strategy} symbols={SymbolCount} accounts={AccountCount}",
            context.RunId, context.StrategyKey,
            context.Symbols.Count, context.AccountIds.Count);
    }

    public void Remove(Guid runId)
    {
        if (_cache.TryRemove(runId, out _))
            _logger.LogInformation("Run context removed from cache: {RunId}", runId);
    }

    public void Refresh(RunRuntimeContext context)
    {
        _cache[context.RunId] = context;
        _logger.LogInformation("Run context refreshed: {RunId}", context.RunId);
    }
}