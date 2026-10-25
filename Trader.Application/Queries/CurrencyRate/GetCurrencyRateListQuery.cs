using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.CurrencyRates;

namespace Trader.Application.Queries.CurrencyRate;

public record GetCurrencyRateListQuery(
    string? FromCurrency = null,
    string? ToCurrency = null,
    int Limit = 100
) : IRequest<Result<IReadOnlyList<CurrencyRateView>>>;

public class GetCurrencyRateListQueryHandler
    : IRequestHandler<GetCurrencyRateListQuery, Result<IReadOnlyList<CurrencyRateView>>>
{
    private readonly ICurrencyRateService _service;
    private readonly ILogger<GetCurrencyRateListQueryHandler> _logger;

    public GetCurrencyRateListQueryHandler(
        ICurrencyRateService service,
        ILogger<GetCurrencyRateListQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<CurrencyRateView>>> Handle(
        GetCurrencyRateListQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var list = await _service.GetRecentAsync(
                request.FromCurrency, request.ToCurrency, request.Limit);
            return Result<IReadOnlyList<CurrencyRateView>>.Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get CurrencyRate list");
            return Result<IReadOnlyList<CurrencyRateView>>.Fail(ex.Message);
        }
    }
}