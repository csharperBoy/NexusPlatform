using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.Instruments;

namespace Trader.Application.Queries.InstrumentOrderPreset;

public record GetOrderPresetListQuery
    : IRequest<Result<IReadOnlyList<OrderPresetView>>>;

public class GetOrderPresetListQueryHandler
    : IRequestHandler<GetOrderPresetListQuery, Result<IReadOnlyList<OrderPresetView>>>
{
    private readonly IInstrumentOrderPresetService _service;
    private readonly ILogger<GetOrderPresetListQueryHandler> _logger;

    public GetOrderPresetListQueryHandler(
        IInstrumentOrderPresetService service,
        ILogger<GetOrderPresetListQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<OrderPresetView>>> Handle(
        GetOrderPresetListQuery request, CancellationToken ct)
    {
        try
        {
            var list = await _service.GetListAsync();
            return Result<IReadOnlyList<OrderPresetView>>.Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get OrderPreset list");
            return Result<IReadOnlyList<OrderPresetView>>.Fail(ex.Message);
        }
    }
}