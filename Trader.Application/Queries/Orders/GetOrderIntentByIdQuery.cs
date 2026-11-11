using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Orders;
using Trader.Application.Dtos.Orders;

namespace Trader.Application.Queries.Orders;

public record GetOrderIntentByIdQuery(Guid Id)
    : IRequest<Result<OrderIntentView>>;

public class GetOrderIntentByIdQueryHandler
    : IRequestHandler<GetOrderIntentByIdQuery, Result<OrderIntentView>>
{
    private readonly IOrderIntentQueryService _service;
    private readonly ILogger<GetOrderIntentByIdQueryHandler> _logger;

    public GetOrderIntentByIdQueryHandler(
        IOrderIntentQueryService service,
        ILogger<GetOrderIntentByIdQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<OrderIntentView>> Handle(
        GetOrderIntentByIdQuery request, CancellationToken ct)
    {
        try
        {
            var view = await _service.GetByIdAsync(request.Id, ct);
            if (view is null)
                return Result<OrderIntentView>.Fail(
                    $"OrderIntent {request.Id} not found.");

            return Result<OrderIntentView>.Ok(view);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to get OrderIntent {Id}", request.Id);
            return Result<OrderIntentView>.Fail(ex.Message);
        }
    }
}