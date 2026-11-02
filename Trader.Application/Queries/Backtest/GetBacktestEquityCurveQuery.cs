using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Backtesting;
using Trader.Application.Dtos.Backtests;

namespace Trader.Application.Queries.Backtest;

public record GetBacktestEquityCurveQuery(Guid BacktestRunId)
    : IRequest<Result<IReadOnlyList<BacktestDailyEquityView>>>;

public class GetBacktestEquityCurveQueryHandler
    : IRequestHandler<GetBacktestEquityCurveQuery,
        Result<IReadOnlyList<BacktestDailyEquityView>>>
{
    private readonly IBacktestRunQueryService _service;
    private readonly ILogger<GetBacktestEquityCurveQueryHandler> _logger;

    public GetBacktestEquityCurveQueryHandler(
        IBacktestRunQueryService service,
        ILogger<GetBacktestEquityCurveQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<BacktestDailyEquityView>>> Handle(
        GetBacktestEquityCurveQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var list = await _service.GetEquityCurveAsync(
                request.BacktestRunId, cancellationToken);
            return Result<IReadOnlyList<BacktestDailyEquityView>>.Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get equity curve for {Id}", request.BacktestRunId);
            return Result<IReadOnlyList<BacktestDailyEquityView>>.Fail(ex.Message);
        }
    }
}