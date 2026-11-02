using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Backtesting;
using Trader.Application.Dtos.Backtests;

namespace Trader.Application.Queries.Backtest;

public record GetBacktestByIdQuery(Guid Id)
    : IRequest<Result<BacktestRunDetailView>>;

public class GetBacktestByIdQueryHandler
    : IRequestHandler<GetBacktestByIdQuery, Result<BacktestRunDetailView>>
{
    private readonly IBacktestRunQueryService _service;
    private readonly ILogger<GetBacktestByIdQueryHandler> _logger;

    public GetBacktestByIdQueryHandler(
        IBacktestRunQueryService service,
        ILogger<GetBacktestByIdQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<BacktestRunDetailView>> Handle(
        GetBacktestByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var detail = await _service.GetByIdAsync(request.Id, cancellationToken);
            if (detail is null)
                return Result<BacktestRunDetailView>.Fail(
                    $"BacktestRun {request.Id} not found.");

            return Result<BacktestRunDetailView>.Ok(detail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get BacktestRun {Id}", request.Id);
            return Result<BacktestRunDetailView>.Fail(ex.Message);
        }
    }
}