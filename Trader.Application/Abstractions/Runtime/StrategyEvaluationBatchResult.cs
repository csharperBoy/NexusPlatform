using Trader.Domain.Entities;

namespace Trader.Application.Abstractions.Runtime;

/// <summary>
/// خروجی یک Evaluate از StrategyEvaluationService.
///
/// Evaluation null یعنی هیچ evaluation ذخیره نشد (snapshot null، تبدیل ارز
/// fail شد، یا feature null). Signals خالی یعنی هیچ سیگنالی صادر نشد
/// (strategy Hold برگرداند یا strength کافی نبود).
/// </summary>
public sealed record StrategyEvaluationBatchResult(
    StrategyEvaluation? Evaluation,
    IReadOnlyList<Signal> Signals)
{
    public static readonly StrategyEvaluationBatchResult Empty =
        new(null, Array.Empty<Signal>());
}