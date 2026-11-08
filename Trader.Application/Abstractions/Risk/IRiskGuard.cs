namespace Trader.Application.Abstractions.Risk;

/// <summary>
/// یک چک مستقل ریسک. Guards به ترتیب در IRiskGuardPipeline اجرا می‌شوند.
/// اگر هر Guard Block برگرداند، بقیه اجرا نمی‌شوند.
/// </summary>
public interface IRiskGuard
{
    string Name { get; }

    Task<RiskDecision> CheckAsync(
        RiskCheckContext context,
        CancellationToken ct = default);
}