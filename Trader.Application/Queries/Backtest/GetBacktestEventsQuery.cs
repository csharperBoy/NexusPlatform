using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Backtesting;
using Trader.Application.Dtos;
using Trader.Application.Dtos.Backtests;

namespace Trader.Application.Queries.Backtest;

public record GetBacktestEventsQuery(BacktestEventQuery Filter)
    : IRequest<Result<PagedResult<BacktestEventView>>>;

public class GetBacktestEventsQueryHandler
    : IRequestHandler<GetBacktestEventsQuery, Result<PagedResult<BacktestEventView>>>
{
    private readonly IBacktestEventQueryService _service;
    private readonly ILogger<GetBacktestEventsQueryHandler> _logger;

    public GetBacktestEventsQueryHandler(
        IBacktestEventQueryService service,
        ILogger<GetBacktestEventsQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<PagedResult<BacktestEventView>>> Handle(
        GetBacktestEventsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var (items, total) = await _service.QueryAsync(
                request.Filter, cancellationToken);

            var pageSize = Math.Clamp(request.Filter.Take, 1, 5000);
            var page = (request.Filter.Skip / pageSize) + 1;

            var paged = new PagedResult<BacktestEventView>
            {
                Items = items.ToList(),
                TotalRecords = total,
                Page = page,
                PageSize = pageSize,
            };

            return Result<PagedResult<BacktestEventView>>.Ok(paged);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to query backtest events for {Id}",
                request.Filter.BacktestRunId);
            return Result<PagedResult<BacktestEventView>>.Fail(ex.Message);
        }
    }
}