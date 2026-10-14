using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.Signals;

namespace Trader.Application.Queries.Signal;

public record GetRecentSignalsQuery(Guid RunId, int Take = 100)
    : IRequest<Result<IReadOnlyList<SignalView>>>;

public class GetRecentSignalsQueryHandler
    : IRequestHandler<GetRecentSignalsQuery, Result<IReadOnlyList<SignalView>>>
{
    private readonly ISignalQueryService _service;
    private readonly ILogger<GetRecentSignalsQueryHandler> _logger;

    public GetRecentSignalsQueryHandler(
        ISignalQueryService service,
        ILogger<GetRecentSignalsQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<SignalView>>> Handle(
        GetRecentSignalsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var signals = await _service.GetRecentByRunAsync(
                request.RunId, request.Take, cancellationToken);
            return Result<IReadOnlyList<SignalView>>.Ok(signals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to get signals for run {RunId}", request.RunId);
            return Result<IReadOnlyList<SignalView>>.Fail(ex.Message);
        }
    }
}