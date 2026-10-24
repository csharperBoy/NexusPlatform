using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.TradingSchedules;

namespace Trader.Application.Queries.TradingSchedule;

public record GetTradingScheduleListQuery
    : IRequest<Result<IReadOnlyList<TradingScheduleInfoView>>>;

public class GetTradingScheduleListQueryHandler
    : IRequestHandler<GetTradingScheduleListQuery,
        Result<IReadOnlyList<TradingScheduleInfoView>>>
{
    private readonly ITradingScheduleQueryService _service;
    private readonly ILogger<GetTradingScheduleListQueryHandler> _logger;

    public GetTradingScheduleListQueryHandler(
        ITradingScheduleQueryService service,
        ILogger<GetTradingScheduleListQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<TradingScheduleInfoView>>> Handle(
        GetTradingScheduleListQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var list = await _service.GetListAsync(cancellationToken);
            return Result<IReadOnlyList<TradingScheduleInfoView>>.Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get TradingSchedule list");
            return Result<IReadOnlyList<TradingScheduleInfoView>>.Fail(ex.Message);
        }
    }
}