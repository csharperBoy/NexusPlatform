using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.PriceCollection;
using Trader.Application.Dtos.PriceCollection;

namespace Trader.Application.Queries.PriceCollection;

public record GetPriceSourceListQuery : IRequest<Result<IReadOnlyList<PriceSourceView>>>;

public class GetPriceSourceListQueryHandler
    : IRequestHandler<GetPriceSourceListQuery, Result<IReadOnlyList<PriceSourceView>>>
{
    private readonly IPriceSourceQueryService _service;
    private readonly ILogger<GetPriceSourceListQueryHandler> _logger;

    public GetPriceSourceListQueryHandler(
        IPriceSourceQueryService service,
        ILogger<GetPriceSourceListQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<PriceSourceView>>> Handle(
        GetPriceSourceListQuery request, CancellationToken ct)
    {
        try
        {
            var list = await _service.GetListAsync(ct);
            return Result<IReadOnlyList<PriceSourceView>>.Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get PriceSource list");
            return Result<IReadOnlyList<PriceSourceView>>.Fail(ex.Message);
        }
    }
}