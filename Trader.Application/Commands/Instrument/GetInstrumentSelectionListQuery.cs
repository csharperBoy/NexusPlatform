using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.Instruments;

namespace Trader.Application.Queries.Instrument;

public record GetInstrumentSelectionListQuery
    : IRequest<Result<IReadOnlyList<InstrumentSelectionView>>>;

public class GetInstrumentSelectionListQueryHandler
    : IRequestHandler<GetInstrumentSelectionListQuery,
        Result<IReadOnlyList<InstrumentSelectionView>>>
{
    private readonly IInstrumentQueryService _service;
    private readonly ILogger<GetInstrumentSelectionListQueryHandler> _logger;

    public GetInstrumentSelectionListQueryHandler(
        IInstrumentQueryService service,
        ILogger<GetInstrumentSelectionListQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<InstrumentSelectionView>>> Handle(
        GetInstrumentSelectionListQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var list = await _service.GetSelectionListAsync();
            return Result<IReadOnlyList<InstrumentSelectionView>>.Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Instrument selection list");
            return Result<IReadOnlyList<InstrumentSelectionView>>.Fail(ex.Message);
        }
    }
}