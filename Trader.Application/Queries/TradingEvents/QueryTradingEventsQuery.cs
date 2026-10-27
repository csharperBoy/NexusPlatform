using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos;
using Trader.Application.Dtos.TradingEvents;

namespace Trader.Application.Queries.TradingEvents;

public record QueryTradingEventsQuery(TradingEventQuery Filter)
    : IRequest<Result<PagedResult<TradingEventView>>>;

public class QueryTradingEventsQueryHandler
    : IRequestHandler<QueryTradingEventsQuery, Result<PagedResult<TradingEventView>>>
{
    private readonly ITradingEventQueryService _service;
    private readonly ILogger<QueryTradingEventsQueryHandler> _logger;

    public QueryTradingEventsQueryHandler(
        ITradingEventQueryService service,
        ILogger<QueryTradingEventsQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<PagedResult<TradingEventView>>> Handle(
    QueryTradingEventsQuery request,
    CancellationToken cancellationToken)
    {
        try
        {
            var (items, total) = await _service.QueryAsync(
                request.Filter, cancellationToken);

            var pageSize = Math.Clamp(request.Filter.Take, 1, 1000);
            var page = (request.Filter.Skip / pageSize) + 1;

            var paged = new PagedResult<TradingEventView>
            {
                Items = items.ToList(),
                TotalRecords = total,
                Page = page,
                PageSize = pageSize,
            };

            return Result<PagedResult<TradingEventView>>.Ok(paged);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query TradingEvents");
            return Result<PagedResult<TradingEventView>>.Fail(ex.Message);
        }
    }
}