using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.Instruments;

namespace Trader.Application.Commands.InstrumentOrderPreset;

public record ResolveOrderPresetCommand(Guid Id)
    : IRequest<Result<ResolvedOrderPresetView>>;

public class ResolveOrderPresetCommandHandler
    : IRequestHandler<ResolveOrderPresetCommand, Result<ResolvedOrderPresetView>>
{
    private readonly IInstrumentOrderPresetService _service;
    private readonly ILogger<ResolveOrderPresetCommandHandler> _logger;

    public ResolveOrderPresetCommandHandler(
        IInstrumentOrderPresetService service,
        ILogger<ResolveOrderPresetCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<ResolvedOrderPresetView>> Handle(
        ResolveOrderPresetCommand request, CancellationToken ct)
    {
        try
        {
            var view = await _service.ResolveAsync(request.Id, ct);
            return Result<ResolvedOrderPresetView>.Ok(view);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve OrderPreset {Id}", request.Id);
            return Result<ResolvedOrderPresetView>.Fail(ex.Message);
        }
    }
}