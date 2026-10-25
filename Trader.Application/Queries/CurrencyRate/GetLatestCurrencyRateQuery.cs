using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;

namespace Trader.Application.Queries.CurrencyRate;

public record GetLatestCurrencyRateQuery(
    string FromCurrency,
    string ToCurrency,
    DateTime? AtTime = null
) : IRequest<Result<decimal?>>;

public class GetLatestCurrencyRateQueryHandler
    : IRequestHandler<GetLatestCurrencyRateQuery, Result<decimal?>>
{
    private readonly ICurrencyRateService _service;
    private readonly ILogger<GetLatestCurrencyRateQueryHandler> _logger;

    public GetLatestCurrencyRateQueryHandler(
        ICurrencyRateService service,
        ILogger<GetLatestCurrencyRateQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<decimal?>> Handle(
        GetLatestCurrencyRateQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var at = request.AtTime ?? DateTime.UtcNow;
            var rate = await _service.GetLatestAsync(
                request.FromCurrency, request.ToCurrency, at);
            return Result<decimal?>.Ok(rate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to get latest rate {From}→{To}",
                request.FromCurrency, request.ToCurrency);
            return Result<decimal?>.Fail(ex.Message);
        }
    }
}