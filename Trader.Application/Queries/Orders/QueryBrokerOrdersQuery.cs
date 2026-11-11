using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Orders;
using Trader.Application.Dtos;
using Trader.Application.Dtos.Orders;

namespace Trader.Application.Queries.Orders;

public record QueryBrokerOrdersQuery(BrokerOrderQuery Filter)
    : IRequest<Result<PagedResult<BrokerOrderView>>>;

public class QueryBrokerOrdersQueryHandler
    : IRequestHandler<QueryBrokerOrdersQuery, Result<PagedResult<BrokerOrderView>>>
{
    private readonly IBrokerOrderQueryService _service;
    private readonly ILogger<QueryBrokerOrdersQueryHandler> _logger;

    public QueryBrokerOrdersQueryHandler(
        IBrokerOrderQueryService service,
        ILogger<QueryBrokerOrdersQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<PagedResult<BrokerOrderView>>> Handle(
        QueryBrokerOrdersQuery request, CancellationToken ct)
    {
        try
        {
            var (items, total) = await _service.QueryAsync(request.Filter, ct);
            var pageSize = Math.Clamp(request.Filter.Take, 1, 500);
            var page = (request.Filter.Skip / pageSize) + 1;

            return Result<PagedResult<BrokerOrderView>>.Ok(new PagedResult<BrokerOrderView>
            {
                Items = items.ToList(),
                TotalRecords = total,
                Page = page,
                PageSize = pageSize,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query BrokerOrders");
            return Result<PagedResult<BrokerOrderView>>.Fail(ex.Message);
        }
    }
}