using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.Instruments;

namespace Trader.Application.Queries.Instrument;

public record GetInstrumentListQuery
    : IRequest<Result<IReadOnlyList<InstrumentInfoView>>>;

public class GetInstrumentListQueryHandler
    : IRequestHandler<GetInstrumentListQuery, Result<IReadOnlyList<InstrumentInfoView>>>
{
    private readonly IInstrumentQueryService _service;
    private readonly ILogger<GetInstrumentListQueryHandler> _logger;

    public GetInstrumentListQueryHandler(
        IInstrumentQueryService service,
        ILogger<GetInstrumentListQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<InstrumentInfoView>>> Handle(
        GetInstrumentListQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var list = await _service.GetListAsync();
            return Result<IReadOnlyList<InstrumentInfoView>>.Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Instrument list");
            return Result<IReadOnlyList<InstrumentInfoView>>.Fail(ex.Message);
        }
    }
}