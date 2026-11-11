using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Orders;
using Trader.Application.Dtos.Orders;

namespace Trader.Application.Queries.Orders;

public record GetBrokerOrderByIdQuery(Guid Id)
    : IRequest<Result<BrokerOrderDetailView>>;

public class GetBrokerOrderByIdQueryHandler
    : IRequestHandler<GetBrokerOrderByIdQuery, Result<BrokerOrderDetailView>>
{
    private readonly IBrokerOrderQueryService _service;
    private readonly ILogger<GetBrokerOrderByIdQueryHandler> _logger;

    public GetBrokerOrderByIdQueryHandler(
        IBrokerOrderQueryService service,
        ILogger<GetBrokerOrderByIdQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<BrokerOrderDetailView>> Handle(
        GetBrokerOrderByIdQuery request, CancellationToken ct)
    {
        try
        {
            var detail = await _service.GetByIdAsync(request.Id, ct);
            if (detail is null)
                return Result<BrokerOrderDetailView>.Fail(
                    $"BrokerOrder {request.Id} not found.");

            return Result<BrokerOrderDetailView>.Ok(detail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to get BrokerOrder {Id}", request.Id);
            return Result<BrokerOrderDetailView>.Fail(ex.Message);
        }
    }
}