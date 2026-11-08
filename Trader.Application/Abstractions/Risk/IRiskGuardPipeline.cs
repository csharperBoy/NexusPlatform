namespace Trader.Application.Abstractions.Risk;

/// <summary>
/// Orchestrator — همه‌ی Guardها را به ترتیب اجرا می‌کند و
/// در اولین Block متوقف می‌شود. Allow فقط اگر همه Allow بدهند.
/// </summary>
public interface IRiskGuardPipeline
{
    Task<RiskDecision> CheckAsync(
        RiskCheckContext context,
        CancellationToken ct = default);
}