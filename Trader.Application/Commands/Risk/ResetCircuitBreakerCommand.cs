using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Risk;

namespace Trader.Application.Commands.Risk;

/// <summary>
/// Reset Circuit Breaker.
///   • (RunId, AccountId) هر دو → فقط همان
///   • فقط RunId → همه‌ی Accountهای آن Run
///   • فقط AccountId → همه‌ی Runهای آن Account
///   • هیچ‌کدام → همه
/// </summary>
public record ResetCircuitBreakerCommand(
    Guid? RunId = null,
    Guid? AccountId = null,
    string? Reason = null
) : IRequest<Result<int>>;   // خروجی: تعداد Resetها

public class ResetCircuitBreakerCommandHandler
    : IRequestHandler<ResetCircuitBreakerCommand, Result<int>>
{
    private readonly IRunAccountLossStreakTracker _tracker;
    private readonly ILogger<ResetCircuitBreakerCommandHandler> _logger;

    public ResetCircuitBreakerCommandHandler(
        IRunAccountLossStreakTracker tracker,
        ILogger<ResetCircuitBreakerCommandHandler> logger)
    {
        _tracker = tracker;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(
        ResetCircuitBreakerCommand request, CancellationToken ct)
    {
        try
        {
            var reason = request.Reason ?? "manual";

            int count;
            if (request.RunId.HasValue && request.AccountId.HasValue)
            {
                await _tracker.ResetAsync(
                    request.RunId.Value, request.AccountId.Value, reason, ct);
                count = 1;
            }
            else if (request.RunId.HasValue)
            {
                count = await _tracker.ResetRunAsync(request.RunId.Value, reason, ct);
            }
            else if (request.AccountId.HasValue)
            {
                count = await _tracker.ResetAccountAsync(request.AccountId.Value, reason, ct);
            }
            else
            {
                // همه
                var all = await _tracker.GetActiveCircuitsAsync(ct);
                foreach (var s in all)
                {
                    await _tracker.ResetAsync(s.RunId, s.AccountId, reason, ct);
                }
                count = all.Count;
            }

            return Result<int>.Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset circuit breaker.");
            return Result<int>.Fail(ex.Message);
        }
    }
}