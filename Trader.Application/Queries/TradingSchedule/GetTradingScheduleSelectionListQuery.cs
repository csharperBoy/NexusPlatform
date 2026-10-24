using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.TradingSchedules;

namespace Trader.Application.Queries.TradingSchedule;

public record GetTradingScheduleSelectionListQuery
    : IRequest<Result<IReadOnlyList<TradingScheduleSelectionView>>>;

public class GetTradingScheduleSelectionListQueryHandler
    : IRequestHandler<GetTradingScheduleSelectionListQuery,
        Result<IReadOnlyList<TradingScheduleSelectionView>>>
{
    private readonly ITradingScheduleQueryService _service;
    private readonly ILogger<GetTradingScheduleSelectionListQueryHandler> _logger;

    public GetTradingScheduleSelectionListQueryHandler(
        ITradingScheduleQueryService service,
        ILogger<GetTradingScheduleSelectionListQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<TradingScheduleSelectionView>>> Handle(
        GetTradingScheduleSelectionListQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var list = await _service.GetSelectionListAsync(cancellationToken);
            return Result<IReadOnlyList<TradingScheduleSelectionView>>.Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get TradingSchedule selection");
            return Result<IReadOnlyList<TradingScheduleSelectionView>>.Fail(ex.Message);
        }
    }
}