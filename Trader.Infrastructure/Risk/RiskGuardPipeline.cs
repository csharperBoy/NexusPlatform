using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trader.Application.Abstractions.Risk;

namespace Trader.Infrastructure.Risk;

public sealed class RiskGuardPipeline : IRiskGuardPipeline
{
    private readonly IReadOnlyList<IRiskGuard> _guards;
    private readonly IOptionsMonitor<TradingRiskOptions> _options;
    private readonly ILogger<RiskGuardPipeline> _logger;

    public RiskGuardPipeline(
        IEnumerable<IRiskGuard> guards,
        IOptionsMonitor<TradingRiskOptions> options,
        ILogger<RiskGuardPipeline> logger)
    {
        _guards = guards.ToList();
        _options = options;
        _logger = logger;

        if (_guards.Count > 0)
        {
            _logger.LogInformation(
                "RiskGuardPipeline initialized with {Count} guards: {Names}",
                _guards.Count, string.Join(", ", _guards.Select(g => g.Name)));
        }
    }

    public async Task<RiskDecision> CheckAsync(
        RiskCheckContext context, CancellationToken ct = default)
    {
        // ── Master switch ──
        if (!_options.CurrentValue.Enabled)
            return RiskDecision.Allow;

        // ── هیچ Guard ثبت‌نشده = Allow (safe default) ──
        if (_guards.Count == 0)
            return RiskDecision.Allow;

        foreach (var guard in _guards)
        {
            try
            {
                var decision = await guard.CheckAsync(context, ct);
                if (decision.Kind == RiskDecisionKind.Block)
                {
                    _logger.LogWarning(
                        "Risk blocked by {Guard}: {Reason} (run={RunId}, account={AccountId}, isin={Isin})",
                        guard.Name, decision.Reason,
                        context.RunContext.RunId, context.Account.Id, context.Intent.Isin);
                    return decision;
                }
            }
            catch (Exception ex)
            {
                // ── Fail-Safe: خطای Guard = Block (نه Allow) ──
                _logger.LogError(ex,
                    "Risk guard {Guard} threw exception. Blocking as fail-safe.",
                    guard.Name);

                return RiskDecision.Block(
                    $"Guard '{guard.Name}' error: {ex.Message}",
                    guard.Name);
            }
        }

        return RiskDecision.Allow;
    }
}