using Trader.Application.Runtime;

namespace Trader.Application.Abstractions.Runtime;

/// <summary>
/// منطق ارزیابی استراتژی برای یک (Run, Symbol) در یک لحظه.
///
/// Pure-ish: تنها I/O انجام‌شده، خواندن از ICandleCache، IMarketSnapshotStore،
/// و ICandleTransformer است. هیچ‌کدام به UoW یا Signal Channel دسترسی ندارند.
///
/// Caller (StrategyEvaluationStage) مسئول Persist کردن
/// StrategyEvaluation و Signal، و Push کردن Envelope به Channel است.
/// </summary>
public interface IStrategyEvaluationService
{
    Task<StrategyEvaluationBatchResult> EvaluateAsync(
        RunRuntimeContext context,
        RunSymbolContext symbol,
        DateTime now,
        CancellationToken ct = default);
}