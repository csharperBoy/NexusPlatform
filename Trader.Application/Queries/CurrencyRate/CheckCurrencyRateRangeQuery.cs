using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Specifications.CurrencyRates;

namespace Trader.Application.Queries.CurrencyRate;

public record CheckCurrencyRateRangeQuery(
    string FromCurrency,
    string ToCurrency,
    DateTime From,
    DateTime To) : IRequest<Result<bool>>;

public class CheckCurrencyRateRangeQueryHandler
    : IRequestHandler<CheckCurrencyRateRangeQuery, Result<bool>>
{
    private readonly ITraderUnitOfWork<Trader.Infrastructure.Data.TraderDbContext> _uow;
    private readonly ILogger<CheckCurrencyRateRangeQueryHandler> _logger;

    public CheckCurrencyRateRangeQueryHandler(
        ITraderUnitOfWork<Trader.Infrastructure.Data.TraderDbContext> uow,
        ILogger<CheckCurrencyRateRangeQueryHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        CheckCurrencyRateRangeQuery request, CancellationToken ct)
    {
        try
        {
            var from = request.FromCurrency.Trim().ToUpperInvariant();
            var to = request.ToCurrency.Trim().ToUpperInvariant();
            var fromUtc = ToUtc(request.From);
            var toUtc = ToUtc(request.To);

            // direct pair
            var direct = await _uow.CurrencyRateSpecs.ListBySpecAsync(
                new CurrencyRatesPairRangeSpec(from, to, fromUtc, toUtc));
            if (direct.Count > 0)
                return Result<bool>.Ok(true);

            // reverse pair
            var reverse = await _uow.CurrencyRateSpecs.ListBySpecAsync(
                new CurrencyRatesPairRangeSpec(to, from, fromUtc, toUtc));
            return Result<bool>.Ok(reverse.Count > 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check currency rate range");
            return Result<bool>.Fail(ex.Message);
        }
    }

    private static DateTime ToUtc(DateTime dt) =>
        dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
}