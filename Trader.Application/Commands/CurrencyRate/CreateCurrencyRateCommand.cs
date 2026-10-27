using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.CurrencyRates;

namespace Trader.Application.Commands.CurrencyRate;

public record CreateCurrencyRateCommand(
    string FromCurrency,
    string ToCurrency,
    decimal Rate,
    DateTime EffectiveAt,
    string Source = "Manual"
) : IRequest<Result<Guid>>;

public class CreateCurrencyRateCommandHandler
    : IRequestHandler<CreateCurrencyRateCommand, Result<Guid>>
{
    private readonly ICurrencyRateService _service;
    private readonly ILogger<CreateCurrencyRateCommandHandler> _logger;

    public CreateCurrencyRateCommandHandler(
        ICurrencyRateService service,
        ILogger<CreateCurrencyRateCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        CreateCurrencyRateCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // ─── Sanity check ───
            var strongerCurrencies = new[] { "USD", "EUR", "GBP", "CHF", "AED" };
            var from = request.FromCurrency.ToUpperInvariant();
            var to = request.ToCurrency.ToUpperInvariant();

            if (strongerCurrencies.Contains(to) && !strongerCurrencies.Contains(from))
            {
                // از ارز ضعیف‌تر (IRR) به قوی‌تر (USD)
                if (request.Rate >= 1m)
                {
                    return Result<Guid>.Fail(
                        $"نرخ {from}→{to} = {request.Rate} غیرمنطقی است. " +
                        $"۱ {from} بسیار کمتر از ۱ {to} ارزش دارد، پس نرخ باید کوچک باشد " +
                        $"(مثلاً {1m / 2_650_000m:E6}). " +
                        $"اگر می‌خواستید ۱ {to} = {request.Rate:N0} {from} ثبت کنید، " +
                        $"لطفاً جهت را برعکس کنید: {to}→{from}.");
                }
            }

            if (strongerCurrencies.Contains(from) && !strongerCurrencies.Contains(to))
            {
                // از USD به IRR — نرخ باید بزرگ باشد
                if (request.Rate < 1m)
                {
                    return Result<Guid>.Fail(
                        $"نرخ {from}→{to} = {request.Rate} غیرمنطقی است. " +
                        $"۱ {from} بسیار بیشتر از ۱ {to} ارزش دارد، پس نرخ باید بزرگ باشد.");
                }
            }
            var id = await _service.CreateAsync(new CreateCurrencyRateModel(
                request.FromCurrency,
                request.ToCurrency,
                request.Rate,
                request.EffectiveAt,
                request.Source));

            return Result<Guid>.Ok(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create CurrencyRate {From}→{To}",
                request.FromCurrency, request.ToCurrency);
            return Result<Guid>.Fail(ex.Message);
        }
    }
}