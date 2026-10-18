using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.Instruments;

namespace Trader.Application.Queries.InstrumentOrderPreset;

public record GetOrderPresetByInstrumentQuery(string Isin)
    : IRequest<Result<OrderPresetView>>;

public class GetOrderPresetByInstrumentQueryHandler
    : IRequestHandler<GetOrderPresetByInstrumentQuery, Result<OrderPresetView>>
{
    private readonly IInstrumentOrderPresetService _service;
    private readonly ILogger<GetOrderPresetByInstrumentQueryHandler> _logger;

    public GetOrderPresetByInstrumentQueryHandler(
        IInstrumentOrderPresetService service,
        ILogger<GetOrderPresetByInstrumentQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<OrderPresetView>> Handle(
        GetOrderPresetByInstrumentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var preset = await _service.GetByInstrumentAsync(request.Isin);
            if (preset is null)
                return Result<OrderPresetView>.Fail(
                    $"OrderPreset for {request.Isin} not found.");
            return Result<OrderPresetView>.Ok(preset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to get OrderPreset for {Isin}", request.Isin);
            return Result<OrderPresetView>.Fail(ex.Message);
        }
    }
}