using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;

namespace Trader.Application.Queries.CurrencyRate;

public record CheckCurrencyRateRangeQuery(
    string FromCurrency,
    string ToCurrency,
    DateTime From,
    DateTime To) : IRequest<Result<bool>>;

public class CheckCurrencyRateRangeQueryHandler
    : IRequestHandler<CheckCurrencyRateRangeQuery, Result<bool>>
{
    private readonly ICurrencyRateService _service;
    private readonly ILogger<CheckCurrencyRateRangeQueryHandler> _logger;

    public CheckCurrencyRateRangeQueryHandler(
        ICurrencyRateService service,
        ILogger<CheckCurrencyRateRangeQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        CheckCurrencyRateRangeQuery request, CancellationToken ct)
    {
        try
        {
            var exists = await _service.CheckRangeExistsAsync(
                request.FromCurrency,
                request.ToCurrency,
                request.From,
                request.To);

            return Result<bool>.Ok(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check currency rate range");
            return Result<bool>.Fail(ex.Message);
        }
    }
}