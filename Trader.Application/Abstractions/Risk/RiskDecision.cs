namespace Trader.Application.Abstractions.Risk;

public enum RiskDecisionKind
{
    /// <summary>Order می‌تواند به Broker فرستاده شود.</summary>
    Allow = 0,

    /// <summary>Order Block می‌شود — OrderIntent به‌عنوان RiskBlocked ثبت می‌شود.</summary>
    Block = 1,
}

/// <summary>
/// نتیجه‌ی چک یک Guard یا Pipeline.
/// </summary>
public sealed record RiskDecision(
    RiskDecisionKind Kind,
    string? Reason,
    string? GuardName)
{
    public static RiskDecision Allow { get; } = new(RiskDecisionKind.Allow, null, null);

    public static RiskDecision Block(string reason, string guardName) =>
        new(RiskDecisionKind.Block, reason, guardName);
}

/// <summary>
/// ورودی به هر Guard. Guardها ممکن است برای محاسبه‌ی خودشان از
/// I/O استفاده کنند (مثلاً Daily Loss نیاز به خواندن Broker دارد).
/// </summary>
public sealed record RiskCheckContext(
    Trader.Domain.Entities.OrderIntent Intent,
    Trader.Application.Runtime.RunRuntimeContext RunContext,
    Trader.Domain.Entities.TraderAccount Account);