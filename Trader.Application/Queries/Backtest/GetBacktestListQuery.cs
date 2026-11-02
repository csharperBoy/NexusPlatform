using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Backtesting;
using Trader.Application.Dtos.Backtests;

namespace Trader.Application.Queries.Backtest;

public record GetBacktestListQuery(Guid? SourceRunId = null, int Take = 100)
    : IRequest<Result<IReadOnlyList<BacktestRunSummaryView>>>;

public class GetBacktestListQueryHandler
    : IRequestHandler<GetBacktestListQuery, Result<IReadOnlyList<BacktestRunSummaryView>>>
{
    private readonly IBacktestRunQueryService _service;
    private readonly ILogger<GetBacktestListQueryHandler> _logger;

    public GetBacktestListQueryHandler(
        IBacktestRunQueryService service,
        ILogger<GetBacktestListQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<BacktestRunSummaryView>>> Handle(
        GetBacktestListQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var list = await _service.GetListAsync(
                request.SourceRunId, request.Take, cancellationToken);
            return Result<IReadOnlyList<BacktestRunSummaryView>>.Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get backtest list");
            return Result<IReadOnlyList<BacktestRunSummaryView>>.Fail(ex.Message);
        }
    }
}