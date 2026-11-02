using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Backtesting;
using Trader.Application.Dtos.Backtests;

namespace Trader.Application.Queries.Backtest;

public record CompareBacktestsQuery(List<Guid> Ids)
    : IRequest<Result<IReadOnlyList<BacktestRunSummaryView>>>;

public class CompareBacktestsQueryHandler
    : IRequestHandler<CompareBacktestsQuery,
        Result<IReadOnlyList<BacktestRunSummaryView>>>
{
    private readonly IBacktestRunQueryService _service;
    private readonly ILogger<CompareBacktestsQueryHandler> _logger;

    public CompareBacktestsQueryHandler(
        IBacktestRunQueryService service,
        ILogger<CompareBacktestsQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<BacktestRunSummaryView>>> Handle(
        CompareBacktestsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var list = await _service.CompareAsync(request.Ids, cancellationToken);
            return Result<IReadOnlyList<BacktestRunSummaryView>>.Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compare backtests");
            return Result<IReadOnlyList<BacktestRunSummaryView>>.Fail(ex.Message);
        }
    }
}