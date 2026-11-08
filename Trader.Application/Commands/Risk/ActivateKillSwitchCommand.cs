using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Risk;

namespace Trader.Application.Commands.Risk;

/// <summary>
/// Scope 0 = Global، Scope 1 = Account (AccountId الزامی).
/// </summary>
public record ActivateKillSwitchCommand(
    int Scope,
    Guid? AccountId = null,
    string? Reason = null
) : IRequest<Result<bool>>;

public class ActivateKillSwitchCommandHandler
    : IRequestHandler<ActivateKillSwitchCommand, Result<bool>>
{
    private readonly IRiskStateTracker _stateTracker;
    private readonly ILogger<ActivateKillSwitchCommandHandler> _logger;

    public ActivateKillSwitchCommandHandler(
        IRiskStateTracker stateTracker,
        ILogger<ActivateKillSwitchCommandHandler> logger)
    {
        _stateTracker = stateTracker;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        ActivateKillSwitchCommand request, CancellationToken ct)
    {
        try
        {
            if (request.Scope == 0)
            {
                await _stateTracker.ActivateKillSwitchAsync(
                    accountId: null,
                    activatedBy: "manual",
                    reason: request.Reason,
                    ct);
            }
            else if (request.Scope == 1)
            {
                if (!request.AccountId.HasValue || request.AccountId == Guid.Empty)
                    return Result<bool>.Fail("AccountId is required for Account-scoped kill switch.");

                await _stateTracker.ActivateKillSwitchAsync(
                    request.AccountId.Value,
                    "manual",
                    request.Reason,
                    ct);
            }
            else
            {
                return Result<bool>.Fail($"Invalid scope: {request.Scope} (expected 0 or 1).");
            }

            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to activate kill switch.");
            return Result<bool>.Fail(ex.Message);
        }
    }
}