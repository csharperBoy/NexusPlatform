using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Logging;
using Trader.Application.Abstractions.Risk;
using Trader.Domain.Enums;

namespace Trader.Application.Commands.Risk;

public record ActivateKillSwitchCommand(
    int Scope,
    Guid? AccountId = null,
    string? Reason = null
) : IRequest<Result<bool>>;

public class ActivateKillSwitchCommandHandler
    : IRequestHandler<ActivateKillSwitchCommand, Result<bool>>
{
    private readonly IRiskStateTracker _stateTracker;
    private readonly ITradingEventLogger _eventLogger;
    private readonly ILogger<ActivateKillSwitchCommandHandler> _logger;

    public ActivateKillSwitchCommandHandler(
        IRiskStateTracker stateTracker,
        ITradingEventLogger eventLogger,
        ILogger<ActivateKillSwitchCommandHandler> logger)
    {
        _stateTracker = stateTracker;
        _eventLogger = eventLogger;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        ActivateKillSwitchCommand request, CancellationToken ct)
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
                    $"Invalid scope: {request.Scope} (expected 0=Global or 1=Account).");
            }

            // ── State change ──
            await _stateTracker.ActivateKillSwitchAsync(
                targetAccountId,
                activatedBy: "manual",
                reason: request.Reason,
                ct);

            // ── Event (audit trail) ──
            _eventLogger.Log(
                stage: TradingStage.Risk,
                type: TradingEventType.KillSwitchActivated,
                severity: TradingEventSeverity.Critical,
                message: $"🚨 Kill Switch فعال شد — محدوده: {scopeLabel}. " +
                         (string.IsNullOrWhiteSpace(request.Reason)
                            ? ""
                            : $"دلیل: {request.Reason}"),
                payload: new
                {
                    scope = scopeLabel,
                    accountId = targetAccountId,
                    reason = request.Reason,
                },
                accountId: targetAccountId);

            await _eventLogger.FlushAsync(ct);

            _logger.LogWarning(
                "Kill Switch ACTIVATED — Scope: {Scope}, Reason: {Reason}",
                scopeLabel, request.Reason ?? "—");

            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to activate kill switch.");
            return Result<bool>.Fail(ex.Message);
        }
    }
}