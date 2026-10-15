namespace Trader.Application.Abstractions.Runtime;

public interface IStrategyEvaluationStage
{
    Task RunOnceAsync(CancellationToken ct = default);
}