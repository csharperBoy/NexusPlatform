using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.TradingRuns;

namespace Trader.Application.Queries.TradingRun;

public record GetTradingRunByIdQuery(Guid Id)
    : IRequest<Result<TradingRunDetailView>>;

public class GetTradingRunByIdQueryHandler
    : IRequestHandler<GetTradingRunByIdQuery, Result<TradingRunDetailView>>
{
    private readonly ITradingRunQueryService _service;
    private readonly ILogger<GetTradingRunByIdQueryHandler> _logger;

    public GetTradingRunByIdQueryHandler(
        ITradingRunQueryService service,
        ILogger<GetTradingRunByIdQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<TradingRunDetailView>> Handle(
        GetTradingRunByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var detail = await _service.GetByIdAsync(request.Id, cancellationToken);
            if (detail is null)
                return Result<TradingRunDetailView>.Fail(
                    $"TradingRun {request.Id} not found.");

            return Result<TradingRunDetailView>.Ok(detail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get TradingRun {Id}", request.Id);
            return Result<TradingRunDetailView>.Fail(ex.Message);
        }
    }
}