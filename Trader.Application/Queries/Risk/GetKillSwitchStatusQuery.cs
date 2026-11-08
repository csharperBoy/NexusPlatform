using Core.Shared.Results;
using MediatR;
using Trader.Application.Abstractions.Risk;

namespace Trader.Application.Queries.Risk;

public sealed record KillSwitchStatusView(
    bool GlobalActive,
    IReadOnlyList<Guid> ActiveAccountIds);

public record GetKillSwitchStatusQuery : IRequest<Result<KillSwitchStatusView>>;

public class GetKillSwitchStatusQueryHandler
    : IRequestHandler<GetKillSwitchStatusQuery, Result<KillSwitchStatusView>>
{
    private readonly IRiskStateTracker _stateTracker;

    public GetKillSwitchStatusQueryHandler(IRiskStateTracker stateTracker)
    {
        _stateTracker = stateTracker;
    }

    public async Task<Result<KillSwitchStatusView>> Handle(
        GetKillSwitchStatusQuery request, CancellationToken ct)
    {
        await _stateTracker.RefreshAsync(ct);

        var view = new KillSwitchStatusView(
            GlobalActive: _stateTracker.IsGlobalKillSwitchActive,
            ActiveAccountIds: _stateTracker.ActiveAccountKillSwitches.ToList());

        return Result<KillSwitchStatusView>.Ok(view);
    }
}