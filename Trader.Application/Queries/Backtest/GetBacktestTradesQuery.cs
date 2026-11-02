using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Backtesting;
using Trader.Application.Dtos.Backtests;

namespace Trader.Application.Queries.Backtest;

public record GetBacktestTradesQuery(Guid BacktestRunId)
    : IRequest<Result<IReadOnlyList<BacktestTradeView>>>;

public class GetBacktestTradesQueryHandler
    : IRequestHandler<GetBacktestTradesQuery, Result<IReadOnlyList<BacktestTradeView>>>
{
    private readonly IBacktestRunQueryService _service;
    private readonly ILogger<GetBacktestTradesQueryHandler> _logger;

    public GetBacktestTradesQueryHandler(
        IBacktestRunQueryService service,
        ILogger<GetBacktestTradesQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<BacktestTradeView>>> Handle(
        GetBacktestTradesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var list = await _service.GetTradesAsync(
                request.BacktestRunId, cancellationToken);
            return Result<IReadOnlyList<BacktestTradeView>>.Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get trades for {Id}", request.BacktestRunId);
            return Result<IReadOnlyList<BacktestTradeView>>.Fail(ex.Message);
        }
    }
}