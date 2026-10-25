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