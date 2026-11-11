using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Orders;
using Trader.Application.Dtos;
using Trader.Application.Dtos.Orders;

namespace Trader.Application.Queries.Orders;

public record QueryOrderIntentsQuery(OrderIntentQuery Filter)
    : IRequest<Result<PagedResult<OrderIntentView>>>;

public class QueryOrderIntentsQueryHandler
    : IRequestHandler<QueryOrderIntentsQuery, Result<PagedResult<OrderIntentView>>>
{
    private readonly IOrderIntentQueryService _service;
    private readonly ILogger<QueryOrderIntentsQueryHandler> _logger;

    public QueryOrderIntentsQueryHandler(
        IOrderIntentQueryService service,
        ILogger<QueryOrderIntentsQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<PagedResult<OrderIntentView>>> Handle(
        QueryOrderIntentsQuery request, CancellationToken ct)
    {
        try
        {
            var (items, total) = await _service.QueryAsync(request.Filter, ct);
            var pageSize = Math.Clamp(request.Filter.Take, 1, 500);
            var page = (request.Filter.Skip / pageSize) + 1;

            return Result<PagedResult<OrderIntentView>>.Ok(new PagedResult<OrderIntentView>
            {
                Items = items.ToList(),
                TotalRecords = total,
                Page = page,
                PageSize = pageSize,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query OrderIntents");
            return Result<PagedResult<OrderIntentView>>.Fail(ex.Message);
        }
    }
}