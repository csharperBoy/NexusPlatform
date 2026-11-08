using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Risk;

namespace Trader.Application.Commands.Risk;

public record DeactivateKillSwitchCommand(
    int Scope,
    Guid? AccountId = null
) : IRequest<Result<bool>>;

public class DeactivateKillSwitchCommandHandler
    : IRequestHandler<DeactivateKillSwitchCommand, Result<bool>>
{
    private readonly IRiskStateTracker _stateTracker;
    private readonly ILogger<DeactivateKillSwitchCommandHandler> _logger;

    public DeactivateKillSwitchCommandHandler(
        IRiskStateTracker stateTracker,
        ILogger<DeactivateKillSwitchCommandHandler> logger)
    {
        _stateTracker = stateTracker;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        DeactivateKillSwitchCommand request, CancellationToken ct)
    {
        try
        {
            if (request.Scope == 0)
            {
                await _stateTracker.DeactivateKillSwitchAsync(
                    accountId: null, deactivatedBy: "manual", ct);
            }
            else if (request.Scope == 1)
            {
                if (!request.AccountId.HasValue || request.AccountId == Guid.Empty)
                    return Result<bool>.Fail("AccountId is required for Account-scoped kill switch.");

                await _stateTracker.DeactivateKillSwitchAsync(
                    request.AccountId.Value, "manual", ct);
            }
            else
            {
                return Result<bool>.Fail($"Invalid scope: {request.Scope}.");
            }

            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deactivate kill switch.");
            return Result<bool>.Fail(ex.Message);
        }
    }
}