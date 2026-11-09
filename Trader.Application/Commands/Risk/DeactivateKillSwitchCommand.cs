using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Logging;
using Trader.Application.Abstractions.Risk;
using Trader.Domain.Enums;

namespace Trader.Application.Commands.Risk;

public record DeactivateKillSwitchCommand(
    int Scope,
    Guid? AccountId = null
) : IRequest<Result<bool>>;

public class DeactivateKillSwitchCommandHandler
    : IRequestHandler<DeactivateKillSwitchCommand, Result<bool>>
{
    private readonly IRiskStateTracker _stateTracker;
    private readonly ITradingEventLogger _eventLogger;
    private readonly ILogger<DeactivateKillSwitchCommandHandler> _logger;

    public DeactivateKillSwitchCommandHandler(
        IRiskStateTracker stateTracker,
        ITradingEventLogger eventLogger,
        ILogger<DeactivateKillSwitchCommandHandler> logger)
    {
        _stateTracker = stateTracker;
        _eventLogger = eventLogger;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        DeactivateKillSwitchCommand request, CancellationToken ct)
    {
        try
        {
            Guid? targetAccountId = null;
            string scopeLabel;

            if (request.Scope == 0)
            {
                scopeLabel = "Global";
            }
            else if (request.Scope == 1)
            {
                if (!request.AccountId.HasValue || request.AccountId == Guid.Empty)
                    return Result<bool>.Fail(
                        "AccountId is required for Account-scoped kill switch.");
                targetAccountId = request.AccountId.Value;
                scopeLabel = $"Account {targetAccountId}";
            }
            else
            {
                return Result<bool>.Fail(
                    $"Invalid scope: {request.Scope}.");
            }

            // ── State change ──
            await _stateTracker.DeactivateKillSwitchAsync(
                targetAccountId, deactivatedBy: "manual", ct);

            // ── Event (audit trail) ──
            _eventLogger.Log(
                stage: TradingStage.Risk,
                type: TradingEventType.KillSwitchDeactivated,
                severity: TradingEventSeverity.Info,
                message: $"✅ Kill Switch غیرفعال شد — محدوده: {scopeLabel}.",
                payload: new
                {
                    scope = scopeLabel,
                    accountId = targetAccountId,
                },
                accountId: targetAccountId);

            await _eventLogger.FlushAsync(ct);

            _logger.LogWarning(
                "Kill Switch DEACTIVATED — Scope: {Scope}", scopeLabel);

            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deactivate kill switch.");
            return Result<bool>.Fail(ex.Message);
        }
    }
}