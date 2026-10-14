using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.TradingRuns;

namespace Trader.Application.Queries.TradingRun;

public record GetTradingRunListQuery
    : IRequest<Result<IReadOnlyList<TradingRunInfoView>>>;

public class GetTradingRunListQueryHandler
    : IRequestHandler<GetTradingRunListQuery, Result<IReadOnlyList<TradingRunInfoView>>>
{
    private readonly ITradingRunQueryService _service;
    private readonly ILogger<GetTradingRunListQueryHandler> _logger;

    public GetTradingRunListQueryHandler(
        ITradingRunQueryService service,
        ILogger<GetTradingRunListQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<TradingRunInfoView>>> Handle(
        GetTradingRunListQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var runs = await _service.GetListAsync(cancellationToken);
            return Result<IReadOnlyList<TradingRunInfoView>>.Ok(runs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get TradingRun list");
            return Result<IReadOnlyList<TradingRunInfoView>>.Fail(ex.Message);
        }
    }
}